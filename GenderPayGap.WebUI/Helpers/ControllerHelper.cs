using System;
using System.Linq;
using System.Security.Claims;
using GenderPayGap.Core;
using GenderPayGap.Core.Interfaces;
using GenderPayGap.Core.Models.HttpResultModels;
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

            UserOrganisation userOrg = gpgUser.UserOrganisations.FirstOrDefault(uo => uo.OrganisationId == organisationId);
            if (userOrg == null)
            {
                throw new IncorrectPermissionsException();
            }

            if (!userOrg.PINConfirmedDate.HasValue)
            {
                // TODO Add sensible error
            }
        }

    }
}
