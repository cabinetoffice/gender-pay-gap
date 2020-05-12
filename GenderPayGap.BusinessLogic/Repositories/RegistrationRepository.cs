using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GenderPayGap.BusinessLogic.Account.Abstractions;
using GenderPayGap.BusinessLogic.Services;
using GenderPayGap.Core;
using GenderPayGap.Core.Interfaces;
using GenderPayGap.Database;

namespace GenderPayGap.BusinessLogic.Repositories
{

    public class RegistrationRepository
    {

        public RegistrationRepository(IDataRepository dataRepository, AuditLogger auditLogger)
        {
            DataRepository = dataRepository ?? throw new ArgumentNullException(nameof(dataRepository));
            AuditLogger = auditLogger ?? throw new ArgumentNullException(nameof(auditLogger));
        }

        public async Task RemoveRetiredUserRegistrationsAsync(User userToRetire)
        {
            // We extract this list of Organisations BEFORE deleting the UserOrganisations to prevent an Entity Framework error:
            // "Attempted to update or delete an entity that does not exist in the store."
            List<Organisation> organisationsToAuditLog = userToRetire.UserOrganisations
                .Select(userOrg => userOrg.Organisation)
                .ToList();

            foreach (UserOrganisation userOrgToUnregister in userToRetire.UserOrganisations)
            {
                // remove all the user registrations associated with the organisation
                userOrgToUnregister.Organisation.UserOrganisations.Remove(userOrgToUnregister);

                // Remove user organisation
                DataRepository.Delete(userOrgToUnregister);
            }

            foreach (Organisation organisation in organisationsToAuditLog)
            {
                // log unregistered via closed account
                AuditLogger.AuditChangeToOrganisation(
                    AuditedAction.RegistrationLog,
                    organisation,
                    new { Status = "Unregistered closed account" },
                    userToRetire);
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

            // We extract these two variables BEFORE deleting the UserOrganisations to prevent an Entity Framework error:
            // "Attempted to update or delete an entity that does not exist in the store."
            Organisation organisationToLog = userOrgToUnregister.Organisation;
            string status = (userOrgToUnregister.UserId == actionByUser.UserId) ? "Unregistered self" : "Unregistered";

            // Remove user organisation
            DataRepository.Delete(userOrgToUnregister);

            // Save changes to database
            await DataRepository.SaveChangesAsync();


            AuditLogger.AuditChangeToOrganisation(
                AuditedAction.RegistrationLog,
                organisationToLog,
                new { Status = status },
                userOrgToUnregister.User);
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
