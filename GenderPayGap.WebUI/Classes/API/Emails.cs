using System;
using System.Threading.Tasks;
using GenderPayGap.Core;
using GenderPayGap.Core.Abstractions;
using GenderPayGap.Core.Classes;
using GenderPayGap.Core.Models;
using GenderPayGap.Extensions;
using GenderPayGap.Extensions.AspNetCore;
using GenderPayGap.WebUI;
using Microsoft.Extensions.Logging;

namespace GenderPayGap
{

    public static class Emails
    {

        /// <summary>
        ///     Send a message to GEO distribution list
        /// </summary>
        /// <param name="subject"></param>
        /// <param name="message"></param>
        public static async Task<bool> SendGeoMessageAsync(string subject, string message, bool test = false)
        {
            try
            {
                await Program.MvcApplication.SendEmailQueue.AddMessageAsync(
                    new QueueWrapper(new SendGeoMessageModel {subject = subject, message = message, test = test}));
                return true;
            }
            catch (Exception ex)
            {
                Program.MvcApplication.Logger.LogError(ex, ex.Message);
            }

            return false;
        }
        
        public static async Task<bool> SendChangePasswordNotificationAsync(string emailAddress)
        {
            var changePasswordCompleted = new ChangePasswordCompletedTemplate {
                RecipientEmailAddress = emailAddress, Test = emailAddress.StartsWithI(Global.TestPrefix)
            };

            return await QueueEmailAsync(changePasswordCompleted);
        }

        public static async Task<bool> SendResetPasswordNotificationAsync(string resetUrl, string emailAddress)
        {
            var resetPasswordVerification = new ResetPasswordVerificationTemplate {
                Url = resetUrl,
                RecipientEmailAddress = emailAddress,
                Test = emailAddress.StartsWithI(Global.TestPrefix),
                Simulate = emailAddress.StartsWithI(Global.TestPrefix)
            };

            return await QueueEmailAsync(resetPasswordVerification);
        }

        public static async Task<bool> SendResetPasswordCompletedAsync(string emailAddress)
        {
            var resetPasswordCompleted = new ResetPasswordCompletedTemplate {
                RecipientEmailAddress = emailAddress,
                Test = emailAddress.StartsWithI(Global.TestPrefix),
                Simulate = emailAddress.StartsWithI(Global.TestPrefix)
            };

            return await QueueEmailAsync(resetPasswordCompleted);
        }

        public static async Task<bool> SendAccountClosedNotificationAsync(string emailAddress, bool test)
        {
            var closeAccountCompleted = new CloseAccountCompletedTemplate {RecipientEmailAddress = emailAddress, Test = test};

            return await QueueEmailAsync(closeAccountCompleted);
        }

        public static async Task<bool> SendGEOOrphanOrganisationNotificationAsync(string organisationName, bool test)
        {
            var orphanOrganisationTemplate = new OrphanOrganisationTemplate {
                RecipientEmailAddress = Config.GetAppSetting("GEODistributionList"), OrganisationName = organisationName, Test = test
            };

            return await QueueEmailAsync(orphanOrganisationTemplate);
        }

        public static async Task<bool> SendGEORegistrationRequestAsync(string reviewUrl,
            string contactName,
            string reportingOrg,
            string reportingAddress,
            bool test = false)
        {
            var geoOrganisationRegistrationRequest = new GeoOrganisationRegistrationRequestTemplate {
                RecipientEmailAddress = Config.GetAppSetting("GEODistributionList"),
                Test = test,
                Name = contactName,
                Org2 = reportingOrg,
                Address = reportingAddress,
                Url = reviewUrl
            };

            return await QueueEmailAsync(geoOrganisationRegistrationRequest);
        }

        public static async Task<bool> SendRegistrationApprovedAsync(string returnUrl, string emailAddress, bool test = false)
        {
            var organisationRegistrationApproved = new OrganisationRegistrationApprovedTemplate {
                RecipientEmailAddress = emailAddress,
                Test = emailAddress.StartsWithI(Global.TestPrefix),
                Simulate = emailAddress.StartsWithI(Global.TestPrefix),
                Url = returnUrl
            };

            return await QueueEmailAsync(organisationRegistrationApproved);
        }

        public static async Task<bool> SendRegistrationDeclinedAsync(string emailAddress, string reason)
        {
            var organisationRegistrationDeclined = new OrganisationRegistrationDeclinedTemplate {
                RecipientEmailAddress = emailAddress,
                Test = emailAddress.StartsWithI(Global.TestPrefix),
                Simulate = emailAddress.StartsWithI(Global.TestPrefix),
                Reason = reason
            };

            return await QueueEmailAsync(organisationRegistrationDeclined);
        }
        
        private static async Task<bool> QueueEmailAsync<TTemplate>(TTemplate emailTemplate) where TTemplate : AEmailTemplate
        {
            try
            {
                await Program.MvcApplication.SendEmailQueue.AddMessageAsync(new QueueWrapper(emailTemplate));
                return true;
            }
            catch (Exception ex)
            {
                Program.MvcApplication.Logger.LogError(ex, ex.Message);
            }

            return false;
        }

    }

}
