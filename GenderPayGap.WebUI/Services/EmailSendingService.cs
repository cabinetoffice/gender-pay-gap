using GenderPayGap.Core;
using GenderPayGap.Core.Classes.Logger;
using GenderPayGap.Extensions.AspNetCore;
using GenderPayGap.WebUI.BackgroundJobs;
using GenderPayGap.WebUI.ExternalServices;

namespace GenderPayGap.WebUI.Services
{
    public class EmailSendingService
    {
        private readonly IGovNotifyAPI govNotifyApi;
        private readonly IBackgroundJobsApi backgroundJobsApi;

        public EmailSendingService(
            IGovNotifyAPI govNotifyApi,
            IBackgroundJobsApi backgroundJobsApi)
        {
            this.govNotifyApi = govNotifyApi;
            this.backgroundJobsApi = backgroundJobsApi;
        }


        public void SendAccountVerificationEmail(string emailAddress, string verificationUrl)
        {
            var personalisation = new Dictionary<string, dynamic>
            {
                {"VerificationUrl", verificationUrl},
            };

            var notifyEmail = new NotifyEmail
            {
                EmailAddress = emailAddress,
                TemplateId = EmailTemplates.AccountVerificationEmail,
                Personalisation = personalisation
            };

            AddEmailToQueue(notifyEmail);
        }

        public void SendSuccessfulSubmissionEmail(string emailAddress,
            string organisationName,
            string submittedOrUpdated,
            string reportingPeriod,
            string reportLink)
        {
            var personalisation = new Dictionary<string, dynamic>
            {
                {"OrganisationName", organisationName},
                {"SubmittedOrUpdated", submittedOrUpdated},
                {"ReportingPeriod", reportingPeriod},
                {"ReportLink", reportLink},
            };

            var notifyEmail = new NotifyEmail
            {
                EmailAddress = emailAddress,
                TemplateId = EmailTemplates.SendSuccessfulSubmissionEmail,
                Personalisation = personalisation
            };

            AddEmailToQueue(notifyEmail);
        }

        public void SendPinEmail(string emailAddress, string pin, string organisationName)
        {
            var personalisation = new Dictionary<string, dynamic>
            {
                {"PIN", pin},
                {"OrganisationName", organisationName},
            };

            var notifyEmail = new NotifyEmail
            {
                EmailAddress = emailAddress, TemplateId = EmailTemplates.SendPinEmail, Personalisation = personalisation
            };

            AddEmailToQueue(notifyEmail);
        }

        public void SendUserAddedToOrganisationEmail(string emailAddress, string organisationName, string username)
        {
            var personalisation = new Dictionary<string, dynamic>
            {
                {"OrganisationName", organisationName},
                {"Username", username},
            };

            var notifyEmail = new NotifyEmail
            {
                EmailAddress = emailAddress, TemplateId = EmailTemplates.UserAddedToOrganisationEmail, Personalisation = personalisation
            };

            AddEmailToQueue(notifyEmail);
        }

        public void SendRemovedUserFromOrganisationEmail(string emailAddress, string organisationName, string removedUserName)
        {
            var personalisation = new Dictionary<string, dynamic>
            {
                {"OrganisationName", organisationName},
                {"RemovedUser", removedUserName},
            };

            var notifyEmail = new NotifyEmail
            {
                EmailAddress = emailAddress,
                TemplateId = EmailTemplates.RemovedUserFromOrganisationEmail,
                Personalisation = personalisation
            };

            AddEmailToQueue(notifyEmail);
        }

        public void SendScopeChangeInEmail(string emailAddress, string organisationName)
        {
            var personalisation = new Dictionary<string, dynamic>
            {
                {"OrganisationName", organisationName},
            };

            var notifyEmail = new NotifyEmail
            {
                EmailAddress = emailAddress, TemplateId = EmailTemplates.ScopeChangeInEmail, Personalisation = personalisation
            };

            AddEmailToQueue(notifyEmail);
        }

        public void SendScopeChangeOutEmail(string emailAddress, string organisationName)
        {
            var personalisation = new Dictionary<string, dynamic>
            {
                {"OrganisationName", organisationName},
            };

            var notifyEmail = new NotifyEmail
            {
                EmailAddress = emailAddress, TemplateId = EmailTemplates.ScopeChangeOutEmail, Personalisation = personalisation
            };

            AddEmailToQueue(notifyEmail);
        }

        public void SendChangeEmailPendingVerificationEmail(string emailAddress, string url)
        {
            var personalisation = new Dictionary<string, dynamic>
            {
                {"url", url},
            };

            var notifyEmail = new NotifyEmail
            {
                EmailAddress = emailAddress,
                TemplateId = EmailTemplates.SendChangeEmailPendingVerificationEmail,
                Personalisation = personalisation
            };

            AddEmailToQueue(notifyEmail);
        }

        public void SendChangeEmailCompletedVerificationEmail(string emailAddress)
        {
            var notifyEmail = new NotifyEmail
            {
                EmailAddress = emailAddress,
                TemplateId = EmailTemplates.SendChangeEmailCompletedVerificationEmail,
            };

            AddEmailToQueue(notifyEmail);
        }

        public void SendChangeEmailCompletedNotificationEmail(string emailAddress)
        {
            var notifyEmail = new NotifyEmail
            {
                EmailAddress = emailAddress,
                TemplateId = EmailTemplates.SendChangeEmailCompletedNotificationEmail,
            };

            AddEmailToQueue(notifyEmail);
        }

        public void SendChangePasswordCompletedEmail(string emailAddress)
        {
            var notifyEmail = new NotifyEmail
            {
                EmailAddress = emailAddress,
                TemplateId = EmailTemplates.SendChangePasswordCompletedEmail,
            };

            AddEmailToQueue(notifyEmail);
        }

        public void SendResetPasswordVerificationEmail(string emailAddress, string url)
        {
            var personalisation = new Dictionary<string, dynamic>
            {
                {"url", url},
            };

            var notifyEmail = new NotifyEmail
            {
                EmailAddress = emailAddress,
                TemplateId = EmailTemplates.SendResetPasswordVerificationEmail,
                Personalisation = personalisation
            };

            AddEmailToQueue(notifyEmail);
        }

        public void SendResetPasswordCompletedEmail(string emailAddress)
        {
            var notifyEmail = new NotifyEmail
            {
                EmailAddress = emailAddress,
                TemplateId = EmailTemplates.SendResetPasswordCompletedEmail,
            };

            AddEmailToQueue(notifyEmail);
        }

        public void SendCloseAccountCompletedEmail(string emailAddress)
        {
            var notifyEmail = new NotifyEmail
            {
                EmailAddress = emailAddress,
                TemplateId = EmailTemplates.SendCloseAccountCompletedEmail,
            };

            AddEmailToQueue(notifyEmail);
        }

        public void SendOrganisationRegistrationApprovedEmail(string emailAddress, string url)
        {
            var personalisation = new Dictionary<string, dynamic>
            {
                {"url", url},
            };

            var notifyEmail = new NotifyEmail
            {
                EmailAddress = emailAddress,
                TemplateId = EmailTemplates.SendOrganisationRegistrationApprovedEmail,
                Personalisation = personalisation
            };

            AddEmailToQueue(notifyEmail);
        }

        public void SendOrganisationRegistrationDeclinedEmail(string emailAddress, string reason)
        {
            var personalisation = new Dictionary<string, dynamic>
            {
                {"reason", reason},
            };

            var notifyEmail = new NotifyEmail
            {
                EmailAddress = emailAddress,
                TemplateId = EmailTemplates.SendOrganisationRegistrationDeclinedEmail,
                Personalisation = personalisation
            };

            AddEmailToQueue(notifyEmail);
        }

        public void SendGeoOrphanOrganisationEmail(string organisationName)
        {
            foreach (string emailAddress in Global.GeoDistributionList)
            {
                // This personalisation Dictionary should be created separately for each recipient (i.e. inside the foreach loop)
                // This ensures each recipient gets a separate instance of the Dictionary
                // This allows each Dictionary to be modified independently without affecting each other
                // Passing in the same Dictionary caused bug GPG-570
                var personalisation = new Dictionary<string, dynamic>
                {
                    {"OrganisationName", organisationName}
                };

                var notifyEmail = new NotifyEmail
                {
                    EmailAddress = emailAddress,
                    TemplateId = EmailTemplates.SendGeoOrphanOrganisationEmail,
                    Personalisation = personalisation
                };

                AddEmailToQueue(notifyEmail);
            }
        }

        public void SendGeoOrganisationRegistrationRequestEmail(string contactName, string reportingOrg, string reportingAddress, string url)
        {
            foreach (string emailAddress in Global.GeoDistributionList)
            {
                // This personalisation Dictionary should be created separately for each recipient (i.e. inside the foreach loop)
                // This ensures each recipient gets a separate instance of the Dictionary
                // This allows each Dictionary to be modified independently without affecting each other
                // Passing in the same Dictionary caused bug GPG-570
                var personalisation = new Dictionary<string, dynamic>
                {
                    {"name", contactName},
                    {"org2", reportingOrg},
                    {"address", reportingAddress},
                    {"url", url},
                };

                var notifyEmail = new NotifyEmail
                {
                    EmailAddress = emailAddress,
                    TemplateId = EmailTemplates.SendGeoOrganisationRegistrationRequestEmail,
                    Personalisation = personalisation
                };

                AddEmailToQueue(notifyEmail);
            }
        }

        public void SendGeoFirstTimeDataSubmissionEmail(string year, string organisationName, string postedDate, string url)
        {
            foreach (string emailAddress in Global.GeoDistributionList)
            {
                // This personalisation Dictionary should be created separately for each recipient (i.e. inside the foreach loop)
                // This ensures each recipient gets a separate instance of the Dictionary
                // This allows each Dictionary to be modified independently without affecting each other
                // Passing in the same Dictionary caused bug GPG-570
                var personalisation = new Dictionary<string, dynamic>
                {
                    {"year", year},
                    {"organisationName", organisationName},
                    {"postedDate", postedDate},
                    {"url", url},
                };

                var notifyEmail = new NotifyEmail
                {
                    EmailAddress = emailAddress,
                    TemplateId = EmailTemplates.SendGeoFirstTimeDataSubmissionEmail,
                    Personalisation = personalisation
                };

                AddEmailToQueue(notifyEmail);
            }
        }

        public void SendAccountRetirementNotificationEmail(string emailAddress, string userFullName, string daysRemaining)
        {
            var personalisation = new Dictionary<string, dynamic>
            {
                {"UserFullName", userFullName},
                {"EmailAddress", emailAddress}, // We do need to send the email address in the personalisation - because it appears in the body of the email
                {"DaysRemaining", daysRemaining},
            };

            var notifyEmail = new NotifyEmail
            {
                EmailAddress = emailAddress,
                TemplateId = EmailTemplates.SendAccountRetirementNotificationEmail,
                Personalisation = personalisation
            };

            AddEmailToQueue(notifyEmail);
        }

        private void AddEmailToQueue(NotifyEmail notifyEmail)
        {
            AddEnvironmentDetailsToEmail(notifyEmail);

            try
            {
                backgroundJobsApi.AddEmailToQueue(notifyEmail);

                CustomLogger.Information("Successfully queued Notify email", new {notifyEmail});
            }
            catch (Exception ex)
            {
                CustomLogger.Error("Failed to queue Notify email", new {Exception = ex});
            }
        }


        public void SendReminderEmail(
            string emailAddress,
            string deadlineDate,
            int daysUntilDeadline,
            string organisationNames,
            bool organisationIsSingular,
            bool organisationIsPlural,
            string sectorType)
        {
            var personalisation = new Dictionary<string, dynamic>
            {
                {"DeadlineDate", deadlineDate},
                {"DaysUntilDeadline", daysUntilDeadline},
                {"OrganisationNames", organisationNames},
                {"OrganisationIsSingular", organisationIsSingular},
                {"OrganisationIsPlural", organisationIsPlural},
                {"SectorType", sectorType},
            };

            var notifyEmail = new NotifyEmail
            {
                EmailAddress = emailAddress,
                TemplateId = EmailTemplates.ReminderEmail,
                Personalisation = personalisation
            };

            SendEmailDirectly(notifyEmail);
        }

        public void SendEmailFromQueue(NotifyEmail notifyEmail)
        {
            SendEmail(notifyEmail);
        }

        private void SendEmailDirectly(NotifyEmail notifyEmail)
        {
            AddEnvironmentDetailsToEmail(notifyEmail);
            SendEmail(notifyEmail);
        }

        private void SendEmail(NotifyEmail notifyEmail)
        {
            if (!Config.IsProduction())
            {
                notifyEmail.EmailAddress = Global.TestEnvironmentEmailRecipient;
            }

            govNotifyApi.SendEmail(notifyEmail);
        }


        private static void AddEnvironmentDetailsToEmail(NotifyEmail notifyEmail)
        {
            if (notifyEmail.Personalisation == null)
            {
                notifyEmail.Personalisation = new Dictionary<string, dynamic>();
            }

            notifyEmail.Personalisation.Add("Environment", GetEnvironmentNameAndRecipientEmailAddressForTestEnvironments(notifyEmail));
        }

        private static string GetEnvironmentNameAndRecipientEmailAddressForTestEnvironments(NotifyEmail notifyEmail)
        {
            return Config.IsProduction()
                ? ""
                : $"[{Config.EnvironmentName} To:{notifyEmail.EmailAddress}] ";
        }

    }

    public static class EmailTemplates
    {

        public const string AccountVerificationEmail = "ab3b8a9b-4327-41d5-9dc1-9694ff088345";
        public const string ScopeChangeOutEmail = "a5e14ca4-9fe7-484d-a239-fc57f0324c19";
        public const string ScopeChangeInEmail = "a54efa64-33d6-4150-9484-669ff8a6c764";
        public const string RemovedUserFromOrganisationEmail = "65ecaa57-e794-4075-9c00-f13b3cb33446";
        public const string UserAddedToOrganisationEmail = "8513d426-1881-49db-92c2-11dd1fd7a30f";
        public const string SendPinEmail = "c320cf3e-d5a1-434e-95c6-84933063be8a";
        public const string SendSuccessfulSubmissionEmail = "9f690ae4-2913-4e98-b9c9-427080f210de";
        public const string SendChangeEmailPendingVerificationEmail = "1ca62b42-d9aa-4f63-823b-971d8831cbc1";
        public const string SendChangeEmailCompletedVerificationEmail = "9d772c11-101a-4eb4-85cf-7b1f575770eb";
        public const string SendChangeEmailCompletedNotificationEmail = "be9cbf1a-1af1-47d8-bcd1-f1821ab61b14";
        public const string SendChangePasswordCompletedEmail = "190bb5a5-ff34-4b15-b164-20f8442289bf";
        public const string SendResetPasswordVerificationEmail = "b9110c6c-831b-4f62-b5cc-3cd903172eeb";
        public const string SendResetPasswordCompletedEmail = "81a83348-a653-4e0c-80e2-b741ecb27cd2";
        public const string SendCloseAccountCompletedEmail = "75caab84-b95a-4991-87fe-c29af3f9e096";
        public const string SendOrganisationRegistrationApprovedEmail = "a349aa87-787d-4fa8-9ce4-f8e5a1b8209e";
        public const string SendOrganisationRegistrationDeclinedEmail = "43d16081-b789-4426-9b00-13f3d9f6dbea";
        public const string SendGeoOrphanOrganisationEmail = "34ca9b32-09d2-4604-80e6-4afdd019d7d2";
        public const string SendGeoOrganisationRegistrationRequestEmail = "3683b65f-9f50-44b8-ae4b-4ae1e84f1a1f";
        public const string SendGeoFirstTimeDataSubmissionEmail = "fecf5ef0-9ecf-494d-891d-8e00847bff31";
        public const string ReminderEmail = "db15432c-9eda-4df4-ac67-290c7232c546";
        public const string SendAccountRetirementNotificationEmail = "1ce9f1be-1fa2-421c-a967-675341e418b8";

    }

}
