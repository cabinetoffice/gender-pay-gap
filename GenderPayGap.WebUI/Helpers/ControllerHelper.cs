using System.Security.Claims;
using GenderPayGap.Core;
using GenderPayGap.Core.Helpers;
using GenderPayGap.Core.Interfaces;
using GenderPayGap.Database;
using GenderPayGap.Extensions;
using GenderPayGap.WebUI.ErrorHandling;
using Microsoft.AspNetCore.Mvc;

namespace GenderPayGap.WebUI.Helpers
{
    public static class ControllerHelper
    {

        public static User GetGpgUserFromAspNetUser(ClaimsPrincipal principal, IDataRepository dataRepository)
        {
            if (principal != null && principal.Identity.IsAuthenticated)
            {
                // Get the claim called "user_id"
                Claim claim = principal.Claims.FirstOrDefault(c => c.Type.ToLower() == "user_id");
                
                if (claim != null)
                {
                    // The claim value is a number which is the user ID
                    if (long.TryParse(claim.Value, out long userId))
                    {
                        return dataRepository.Get<User>(userId);
                    }
                }
            }

            return null;
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

            // If UserOrganisation exists, but isn't active
            if (!userOrg.HasBeenActivated())
            {
                throw new UserNotRegisteredToReportForOrganisationException();
            }
        }

        public static void ThrowIfUserIsNotAwaitingPinInThePostForGivenOrganisation(ClaimsPrincipal aspDotNetUser, IDataRepository dataRepository, long organisationId)
        {
            User gpgUser = GetGpgUserFromAspNetUser(aspDotNetUser, dataRepository);

            Organisation organisation = dataRepository.Get<Organisation>(organisationId);
            UserOrganisation userOrg = gpgUser.UserOrganisations.FirstOrDefault(uo => uo.OrganisationId == organisationId);
           
            // If there's no organisation with the ID provided
            if (organisation == null)
            {
                throw new PageNotFoundException();
            }
            
            // If the organisation isn't active
            if (organisation.Status != OrganisationStatuses.Active)
            {
                throw new UserNotRegisteredToReportForOrganisationException();
            }
            
            // If the UserOrganisation doesn't exist
            if (userOrg == null)
            {
                throw new UserNotRegisteredToReportForOrganisationException();
            }

            // If UserOrganisation exists, but isn't awaiting PIN in the Post
            if (!userOrg.IsAwaitingActivationPIN())
            {
                throw new PageNotFoundException();
            }
        }

        public static long DecryptOrganisationIdOrThrow404(string encryptedOrganisationId)
        {
            if (!DecryptToId(encryptedOrganisationId, out long organisationId))
            {
                throw new PageNotFoundException();
            }

            return organisationId;
        }

        public static Organisation LoadOrganisationOrThrow404(long organisationId, IDataRepository dataRepository)
        {
            var organisation = dataRepository.Get<Organisation>(organisationId);

            if (organisation == null)
            {
                throw new PageNotFoundException();
            }

            return organisation;
        }

        public static void Throw404IfOrganisationIsNotSearchable(Organisation organisation)
        {
            if (!organisation.IsSearchable())
            {
                throw new PageNotFoundException();
            }
        }

        public static long DecryptUserIdOrThrow404(string encryptedUserId)
        {
            if (!DecryptToId(encryptedUserId, out long userId))
            {
                throw new PageNotFoundException();
            }

            return userId;
        }

        public static User LoadUserOrThrow404(long userId, IDataRepository dataRepository)
        {
            var user = dataRepository.Get<User>(userId);

            if (user == null)
            {
                throw new PageNotFoundException();
            }

            return user;
        }

        public static void ThrowIfReportingYearIsOutsideOfRangeForAnyOrganisation(int reportingYear)
        {
            if (!ReportingYearsHelper.GetReportingYears().Contains(reportingYear))
            {
                throw new PageNotFoundException();
            }
        }

        public static void ThrowIfReportingYearIsOutsideOfRange(int reportingYear, long organisationId, IDataRepository dataRepository)
        {
            Organisation organisation = dataRepository.Get<Organisation>(organisationId);
            
            if (!ReportingYearsHelper.GetReportingYears(organisation.SectorType).Contains(reportingYear))
            {
                throw new PageNotFoundException();
            }
        }

        public static Return LoadReturnForYearOrThrow404(Organisation organisation, int reportingYear)
        {
            Return returnForYear = organisation.GetReturn(reportingYear);
            
            if (returnForYear == null)
            {
                throw new PageNotFoundException();
            }

            return returnForYear;
        }

        public static void ThrowIfAdminIsImpersonatingUser(ClaimsPrincipal aspDotNetUser)
        {
            if (LoginHelper.IsUserBeingImpersonated(aspDotNetUser))
            {
                throw new AdminCannotTakeActionIfImpersonatingUserException();
            }
        }

        public static void RedirectIfUserNeedsToReadPrivacyPolicy(ClaimsPrincipal aspDotNetUser, User gpgUser, IUrlHelper url)
        {
            // Show the privacy policy to non-admin users (if they're not being impersonated) if they haven't read it yet
            if (!LoginHelper.IsUserBeingImpersonated(aspDotNetUser) && !gpgUser.IsFullOrReadOnlyAdministrator())
            {
                DateTime? hasReadPrivacy = gpgUser.AcceptedPrivacyStatement;
                if (hasReadPrivacy == null || hasReadPrivacy.Value < Global.PrivacyChangedDate)
                {
                    throw new RedirectToPrivacyPolicyException(url);
                }
            }
        }

        private static bool DecryptToId(string encryptedId, out long decryptedId)
        {
            decryptedId = 0;
            if (string.IsNullOrWhiteSpace(encryptedId))
            {
                return false;
            }

            long id;
            try
            {
                id = Encryption.DecryptId(encryptedId);
            }
            catch (Exception e)
            {
                return false;
            }
            
            if (id <= 0)
            {
                return false;
            }

            decryptedId = id;
            return true;
        }

    }
}
