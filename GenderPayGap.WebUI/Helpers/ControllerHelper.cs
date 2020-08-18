using System;
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

        public static void ThrowIfUserAccountRetired(ClaimsPrincipal aspDotNetUser, IDataRepository dataRepository)
        {
            User gpgUser = GetGpgUserFromAspNetUser(aspDotNetUser, dataRepository);

            if (gpgUser.Status == UserStatuses.Retired ||
                gpgUser.Status == UserStatuses.Unknown)
            {
                throw new UserAccountRetiredException();
            }
        }

        public static void ThrowIfEmailNotVerified(ClaimsPrincipal aspDotNetUser, IDataRepository dataRepository)
        {
            User gpgUser = GetGpgUserFromAspNetUser(aspDotNetUser, dataRepository);

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

    }
}
