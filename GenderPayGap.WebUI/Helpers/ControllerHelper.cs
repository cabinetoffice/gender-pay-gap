using System;
using System.Linq;
using System.Security.Claims;
using GenderPayGap.Core;
using GenderPayGap.Core.Interfaces;
using GenderPayGap.Database;
using GenderPayGap.WebUI.Classes;
using GenderPayGap.WebUI.ErrorHandling;

namespace GenderPayGap.WebUI.Helpers
{
    public static class ControllerHelper
    {

        public static User GetGpgUserFromAspNetUser(ClaimsPrincipal user, IDataRepository dataRepository)
        {
            if (user != null && user.Identity.IsAuthenticated)
            {
                return dataRepository.FindUser(user);
            }
            else
            {
                return null;
            }
        }

        public static void ThrowIfUserAccountRetiredOrEmailNotVerified(ClaimsPrincipal aspDotNetUser, IDataRepository dataRepository)
        {
            User gpgUser = GetGpgUserFromAspNetUser(aspDotNetUser, dataRepository);
            ThrowIfUserAccountRetiredOrEmailNotVerified(gpgUser);
        }

        public static void ThrowIfUserAccountRetiredOrEmailNotVerified(User gpgUser)
        {
            if (gpgUser.Status == UserStatuses.Retired ||
                gpgUser.Status == UserStatuses.Unknown)
            {
                throw new UserAccountRetiredException();
            }

            if (gpgUser.EmailVerifiedDate == null ||
                gpgUser.EmailVerifiedDate == DateTime.MinValue)
            {
                throw new EmailNotVerifiedException
                {
                    EmailAddress = gpgUser.EmailAddress,
                    EmailVerifySendDate = gpgUser.EmailVerifySendDate
                };
            }
        }

        public static void Throw404IfFeatureDisabled(FeatureFlag featureFlag)
        {
            if (!FeatureFlagHelper.IsFeatureEnabled(featureFlag))
            {
                throw new PageNotFoundException();
            }
        }

        public static void ThrowIfUserDoesNotHavePermissionsForGivenOrganisation(ClaimsPrincipal aspDotNetUser, IDataRepository dataRepository, long organisationId)
        {
            User gpgUser = GetGpgUserFromAspNetUser(aspDotNetUser, dataRepository);

            Organisation dbOrg = dataRepository.Get<Organisation>(organisationId);
            UserOrganisation userOrg = gpgUser.UserOrganisations.FirstOrDefault(uo => uo.OrganisationId == organisationId);
           
            // If there's no organisation with the ID provided
            if (dbOrg == null)
            {
                throw new PageNotFoundException();
            }
            
            // If the organisation isn't active
            if (dbOrg.Status != OrganisationStatuses.Active)
            {
                throw new UserNotRegisteredToReportForOrganisationException();
            }
            
            // If the UserOrganisation doesn't exist
            if (userOrg == null)
            {
                throw new UserNotRegisteredToReportForOrganisationException();
            }

            // If organisation exists, but isn't active
            if (!userOrg.PINConfirmedDate.HasValue)
            {
                throw new UserNotRegisteredToReportForOrganisationException();
            }
        }

        public static long DecryptOrganisationIdOrThrow404(string encryptedOrganisationId)
        {
            if (!encryptedOrganisationId.DecryptToId(out long organisationId))
            {
                throw new PageNotFoundException();
            }

            return organisationId;
        }

        public static void ThrowIfReportingYearIsOutsideOfRange(int reportingYear)
        {
            if (!ReportingYearsHelper.GetReportingYears().Contains(reportingYear))
            {
                throw new PageNotFoundException();
            }
        }

        public static void ThrowIfAdminIsImpersonatingUser(ClaimsPrincipal aspDotNetUser)
        {
            if (LoginHelper.IsUserBeingImpersonated(aspDotNetUser))
            {
                throw new AdminCannotTakeActionIfImpersonatingUserException();
            }
        }

    }
}
