using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GenderPayGap.BusinessLogic.Account.Abstractions;
using GenderPayGap.BusinessLogic.LogRecords;
using GenderPayGap.Core.Interfaces;
using GenderPayGap.Database;
using GenderPayGap.Extensions;

namespace GenderPayGap.BusinessLogic.Repositories
{

    public class RegistrationRepository : IRegistrationRepository
    {

        public RegistrationRepository(IDataRepository dataRepository, IRegistrationLogRecord registrationLog)
        {
            DataRepository = dataRepository ?? throw new ArgumentNullException(nameof(dataRepository));
            RegistrationLog = registrationLog ?? throw new ArgumentNullException(nameof(registrationLog));
        }

        public async Task RemoveRetiredUserRegistrationsAsync(User userToRetire, User actionByUser)
        {
            // remove all the user registrations associated with the organisation
            userToRetire.UserOrganisations.ForEach(uo => uo.Organisation.UserOrganisations.Remove(uo));

            // get all latest registrations assigned to this user or latest registration is null
            IEnumerable<UserOrganisation> latestRegistrationsByUser = userToRetire.UserOrganisations
                .Where(
                    uo => uo.Organisation.LatestRegistration == null || uo.Organisation.LatestRegistration.UserId == userToRetire.UserId);

            // update those latest registrations
            latestRegistrationsByUser.ForEach(
                async userOrgToUnregister => {
                    Organisation sourceOrg = userOrgToUnregister.Organisation;

                    // update latest registration (if one exists)
                    UserOrganisation newLatestReg = sourceOrg.GetLatestRegistration();
                    if (newLatestReg != null)
                    {
                        sourceOrg.LatestRegistration = newLatestReg;
                    }

                    // log unregistered via closed account
                    await RegistrationLog.LogUserAccountClosedAsync(userOrgToUnregister, actionByUser.EmailAddress);

                    // Remove user organisation
                    DataRepository.Delete(userOrgToUnregister);
                });

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

            // update latest registration (if one exists)
            if (sourceOrg.LatestRegistration == null || sourceOrg.LatestRegistration.UserId == userOrgToUnregister.UserId)
            {
                UserOrganisation newLatestReg = sourceOrg.GetLatestRegistration();
                if (newLatestReg != null)
                {
                    sourceOrg.LatestRegistration = newLatestReg;
                }
            }

            // log record
            if (userOrgToUnregister.UserId == actionByUser.UserId)
            {
                // unregistered self
                await RegistrationLog.LogUnregisteredSelfAsync(userOrgToUnregister, actionByUser.EmailAddress);
            }
            else
            {
                // unregistered by someone else
                await RegistrationLog.LogUnregisteredAsync(userOrgToUnregister, actionByUser.EmailAddress);
            }

            // Remove user organisation
            DataRepository.Delete(userOrgToUnregister);

            // Save changes to database
            await DataRepository.SaveChangesAsync();
        }

        #region Dependencies

        public IDataRepository DataRepository { get; }

        public IRegistrationLogRecord RegistrationLog { get; }

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
