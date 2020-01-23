using System;
using System.Threading.Tasks;
using GenderPayGap.Core;
using GenderPayGap.Core.Classes;
using GenderPayGap.Core.Classes.Queues;
using GenderPayGap.Core.Models;
using GenderPayGap.Database;
using GenderPayGap.Extensions;

namespace GenderPayGap.BusinessLogic.LogRecords
{

    public interface IRegistrationLogRecord : ILogRecordLogger
    {

        Task LogUnregisteredAsync(UserOrganisation unregisteredUserOrg, string actionByEmailAddress);

        Task LogUnregisteredSelfAsync(UserOrganisation unregisteredUserOrg, string actionByEmailAddress);

        Task LogUserAccountClosedAsync(UserOrganisation userOrgToUnregister, string actionByEmailAddress);

    }

    public class RegistrationLogRecord : LogRecordLogger, IRegistrationLogRecord
    {

        public RegistrationLogRecord(LogRecordQueue queue)
            : base(queue, AppDomain.CurrentDomain.FriendlyName, Filenames.RegistrationLog) { }

        public async Task LogUnregisteredAsync(UserOrganisation unregisteredUserOrg, string actionByEmailAddress)
        {
            await LogAsync(unregisteredUserOrg, "Unregistered", actionByEmailAddress);
        }

        public async Task LogUnregisteredSelfAsync(UserOrganisation unregisteredUserOrg, string actionByEmailAddress)
        {
            await LogAsync(unregisteredUserOrg, "Unregistered self", actionByEmailAddress);
        }

        public async Task LogUserAccountClosedAsync(UserOrganisation retiredUserOrg, string actionByEmailAddress)
        {
            await LogAsync(retiredUserOrg, "Unregistered closed account", actionByEmailAddress);
        }

        private async Task LogAsync(UserOrganisation logUserOrg, string status, string actionByEmailAddress)
        {
            Organisation logOrg = logUserOrg.Organisation;
            User logUser = logUserOrg.User;
            OrganisationAddress logAddress = logUserOrg.Address;

            if (logUser.EmailAddress.StartsWithI(Global.TestPrefix))
            {
                return;
            }

            await WriteAsync(
                new RegisterLogModel {
                    StatusDate = VirtualDateTime.Now,
                    Status = status,
                    ActionBy = actionByEmailAddress,
                    Details = "",
                    Sector = logOrg.SectorType,
                    Organisation = logOrg.OrganisationName,
                    CompanyNo = logOrg.CompanyNumber,
                    Address = logAddress.GetAddressString(),
                    SicCodes = logOrg.GetSicCodeIdsString(),
                    UserFirstname = logUser.Firstname,
                    UserLastname = logUser.Lastname,
                    UserJobtitle = logUser.JobTitle,
                    UserEmail = logUser.EmailAddress,
                    ContactFirstName = logUser.ContactFirstName,
                    ContactLastName = logUser.ContactLastName,
                    ContactJobTitle = logUser.ContactJobTitle,
                    ContactOrganisation = logUser.ContactOrganisation,
                    ContactPhoneNumber = logUser.ContactPhoneNumber
                });
        }

    }

}
