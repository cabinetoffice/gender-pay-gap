using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GenderPayGap.Core.Classes;
using GenderPayGap.Core.Classes.Logger;
using GenderPayGap.Extensions.AspNetCore;

namespace GenderPayGap.WebUI.Services
{
    public class EmailSendingService
    {
        
        public static async void SendAccountVerificationEmail(string emailAddress, string url)
        {
            var personalisation = new Dictionary<string, dynamic> {
                {"url", url}, {"Environment", Config.IsProduction() ? "" : $"[{Config.EnvironmentName}] "}
            };

            var notifyEmail = new NotifyEmail {
                EmailAddress = emailAddress, TemplateId = EmailTemplates.AccountVerificationEmail, Personalisation = personalisation
            };

            await AddEmailToQueue(notifyEmail);
        }

        public static async void SendSuccessfulSubmissionEmail(string emailAddress,
            string organisationName,
            string submittedOrUpdated,
            string reportingPeriod,
            string reportLink)
        {
            var personalisation = new Dictionary<string, dynamic> {
                {"OrganisationName", organisationName},
                {"SubmittedOrUpdated", submittedOrUpdated},
                {"ReportingPeriod", reportingPeriod},
                {"ReportLink", reportLink},
                {"Environment", Config.IsProduction() ? "" : $"[{Config.EnvironmentName}] "}
            };

            var notifyEmail = new NotifyEmail {
                EmailAddress = emailAddress, TemplateId = EmailTemplates.SendSuccessfulSubmissionEmail, Personalisation = personalisation
            };

            await AddEmailToQueue(notifyEmail);
        }

        public static async void SendPinEmail(string emailAddress, string pin, string organisationName)
        {
            var personalisation = new Dictionary<string, dynamic> {
                {"PIN", pin},
                {"OrganisationName", organisationName},
                {"Environment", Config.IsProduction() ? "" : $"[{Config.EnvironmentName}] "}
            };

            var notifyEmail = new NotifyEmail {
                EmailAddress = emailAddress, TemplateId = EmailTemplates.SendPinEmail, Personalisation = personalisation
            };

            await AddEmailToQueue(notifyEmail);
        }

        public static async void SendUserAddedToOrganisationEmail(string emailAddress, string organisationName, string username)
        {
            var personalisation = new Dictionary<string, dynamic> {
                {"OrganisationName", organisationName},
                {"Username", username},
                {"Environment", Config.IsProduction() ? "" : $"[{Config.EnvironmentName}] "}
            };

            var notifyEmail = new NotifyEmail {
                EmailAddress = emailAddress, TemplateId = EmailTemplates.UserAddedToOrganisationEmail, Personalisation = personalisation
            };

            await AddEmailToQueue(notifyEmail);
        }

        public static async void SendRemovedUserFromOrganisationEmail(string emailAddress, string organisationName, string removedUserName)
        {
            var personalisation = new Dictionary<string, dynamic> {
                {"OrganisationName", organisationName},
                {"RemovedUser", removedUserName},
                {"Environment", Config.IsProduction() ? "" : $"[{Config.EnvironmentName}] "}
            };

            var notifyEmail = new NotifyEmail {
                EmailAddress = emailAddress, TemplateId = EmailTemplates.RemovedUserFromOrganisationEmail, Personalisation = personalisation
            };

            await AddEmailToQueue(notifyEmail);
        }

        public static async void SendScopeChangeInEmail(string emailAddress, string organisationName)
        {
            var personalisation = new Dictionary<string, dynamic> {
                {"OrganisationName", organisationName}, {"Environment", Config.IsProduction() ? "" : $"[{Config.EnvironmentName}] "}
            };

            var notifyEmail = new NotifyEmail {
                EmailAddress = emailAddress, TemplateId = EmailTemplates.ScopeChangeInEmail, Personalisation = personalisation
            };

            await AddEmailToQueue(notifyEmail);
        }

        public static async void SendScopeChangeOutEmail(string emailAddress, string organisationName)
        {
            var personalisation = new Dictionary<string, dynamic> {
                {"OrganisationName", organisationName}, {"Environment", Config.IsProduction() ? "" : $"[{Config.EnvironmentName}] "}
            };

            var notifyEmail = new NotifyEmail {
                EmailAddress = emailAddress, TemplateId = EmailTemplates.ScopeChangeOutEmail, Personalisation = personalisation
            };

            await AddEmailToQueue(notifyEmail);
        }

        private static async Task<bool> AddEmailToQueue(NotifyEmail notifyEmail)
        {
            try
            {
                await Program.MvcApplication.SendNotifyEmailQueue.AddMessageAsync(notifyEmail);

                CustomLogger.Information("Successfully added message to SendNotifyEmail Queue", new {notifyEmail});
                return true;
            }
            catch (Exception ex)
            {
                CustomLogger.Error("Failed to add message to SendNotifyEmail Queue", new {Exception = ex});
            }

            return false;
        }

    }

    public static class EmailTemplates
    {

        public const string AccountVerificationEmail = "ed3672eb-4a88-4db4-ae80-2884e5e7c68e";
        public const string ScopeChangeOutEmail = "a5e14ca4-9fe7-484d-a239-fc57f0324c19";
        public const string ScopeChangeInEmail = "a54efa64-33d6-4150-9484-669ff8a6c764";
        public const string RemovedUserFromOrganisationEmail = "65ecaa57-e794-4075-9c00-f13b3cb33446";
        public const string UserAddedToOrganisationEmail = "8513d426-1881-49db-92c2-11dd1fd7a30f";
        public const string SendPinEmail = "c320cf3e-d5a1-434e-95c6-84933063be8a";
        public const string SendSuccessfulSubmissionEmail = "9f690ae4-2913-4e98-b9c9-427080f210de";

    }

}
