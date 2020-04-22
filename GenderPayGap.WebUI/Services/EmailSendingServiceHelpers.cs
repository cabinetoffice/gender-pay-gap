using System.Collections.Generic;
using System.Linq;
using GenderPayGap.Database;

namespace GenderPayGap.WebUI.Services
{
    public class EmailSendingServiceHelpers
    {

        public static void SendUserAddedEmailToExistingUsers(Organisation organisation, User addedUser, EmailSendingService emailSendingService)
        {
            IEnumerable<string> emailAddressesForOrganisation = organisation.UserOrganisations
                .Select(uo => uo.User.EmailAddress)
                .Where(ea => ea != addedUser.EmailAddress);

            foreach (string emailAddress in emailAddressesForOrganisation)
            {
                emailSendingService.SendUserAddedToOrganisationEmail(emailAddress, organisation.OrganisationName, addedUser.Fullname);
            }
        }

        public static void SendSuccessfulSubmissionEmailToRegisteredUsers(Return postedReturn, string reportLink, string submittedOrUpdated, EmailSendingService emailSendingService)
        {
            IEnumerable<string> emailAddressesForOrganisation = postedReturn.Organisation.UserOrganisations
                .Select(uo => uo.User.EmailAddress);

            foreach (string emailAddress in emailAddressesForOrganisation)
            {
                emailSendingService.SendSuccessfulSubmissionEmail(
                    emailAddress,
                    postedReturn.Organisation.OrganisationName,
                    submittedOrUpdated,
                    postedReturn.GetReportingPeriod(),
                    reportLink);
            }
        }

    }
}
