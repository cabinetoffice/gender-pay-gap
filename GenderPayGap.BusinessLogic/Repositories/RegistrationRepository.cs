using System;
using System.Threading.Tasks;
using GenderPayGap.BusinessLogic.Account.Abstractions;
using GenderPayGap.BusinessLogic.Services;
using GenderPayGap.Core;
using GenderPayGap.Core.Interfaces;
using GenderPayGap.Database;

namespace GenderPayGap.BusinessLogic.Repositories
{

    public class RegistrationRepository : IRegistrationRepository
    {

        public RegistrationRepository(IDataRepository dataRepository, AuditLogger auditLogger)
        {
            DataRepository = dataRepository ?? throw new ArgumentNullException(nameof(dataRepository));
            AuditLogger = auditLogger ?? throw new ArgumentNullException(nameof(auditLogger));
        }

        public async Task RemoveRetiredUserRegistrationsAsync(User userToRetire, User actionByUser)
        {
            foreach (UserOrganisation userOrgToUnregister in userToRetire.UserOrganisations)
            {
                // remove all the user registrations associated with the organisation
                userOrgToUnregister.Organisation.UserOrganisations.Remove(userOrgToUnregister);

                // log unregistered via closed account
                AuditLogger.AuditChangeToUser(
                    AuditedAction.RegistrationLog,
                    userToRetire,
                    new { Status = "Unregistered closed account" },
                    actionByUser);

                // Remove user organisation
                DataRepository.Delete(userOrgToUnregister);
            }

            // save changes to database
            await DataRepository.SaveChangesAsync();
        }

        public async Task RemoveRegistrationAsync(UserOrganisation userOrgToUnregister, User actionByUser)
        {
            if (userOrgToUnregister is null)
            {
                throw new ArgumentNullException(nameof(userOrgToUnregister));
            }

            if (actionByUser is null)
            {
                throw new ArgumentNullException(nameof(actionByUser));
            }

            Organisation sourceOrg = userOrgToUnregister.Organisation;

            // Remove the user registration from the organisation
            sourceOrg.UserOrganisations.Remove(userOrgToUnregister);

            // log record
            if (userOrgToUnregister.UserId == actionByUser.UserId)
            {
                // unregistered self
                AuditLogger.AuditChangeToUser(
                    AuditedAction.RegistrationLog,
                    userOrgToUnregister.User,
                    new { Status = "Unregistered self" },
                    actionByUser);
            }
            else
            {
                // unregistered by someone else
                AuditLogger.AuditChangeToUser(
                    AuditedAction.RegistrationLog,
                    userOrgToUnregister.User,
                    new { Status = "Unregistered" },
                    actionByUser);
            }

            // Remove user organisation
            DataRepository.Delete(userOrgToUnregister);

            // Save changes to database
            await DataRepository.SaveChangesAsync();
        }

        #region Dependencies

        public IDataRepository DataRepository { get; }

        public AuditLogger AuditLogger { get; set; }

        #endregion

        #region IDataTransaction

        public async Task BeginTransactionAsync(Func<Task> delegateAction)
        {
            await DataRepository.BeginTransactionAsync(delegateAction);
        }

        public void CommitTransaction()
        {
            DataRepository.CommitTransaction();
        }

        public void RollbackTransaction()
        {
            DataRepository.RollbackTransaction();
        }

        #endregion

    }

}
