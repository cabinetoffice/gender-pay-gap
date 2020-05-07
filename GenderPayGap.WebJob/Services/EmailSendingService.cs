using System.Collections.Generic;
using GenderPayGap.Core;
using GenderPayGap.Core.Classes;
using GenderPayGap.Core.Interfaces;
using GenderPayGap.Extensions.AspNetCore;

namespace GenderPayGap.WebJob.Services
{
    public class EmailSendingService
    {

        private readonly IGovNotifyAPI govNotifyApi;

        public EmailSendingService(IGovNotifyAPI govNotifyApi)
        {
            this.govNotifyApi = govNotifyApi;
        }

        public void SendGeoSiteCertificateSoonToExpireEmail(string host, string expiryDate, string remainingDays)
        {
            var personalisation = new Dictionary<string, dynamic>
            {
                {"host", host},
                {"expiryDate", expiryDate},
                {"remainingDays", remainingDays},
                {"Environment", GetEnvironmentNameForTestEnvironments()}
            };

            foreach (string emailAddress in Global.GeoDistributionList)
            {
                var notifyEmail = new NotifyEmail
                {
                    EmailAddress = emailAddress,
                    TemplateId = EmailTemplates.GeoSiteCertificateSoonToExpireEmail,
                    Personalisation = personalisation
                };

                SendEmail(notifyEmail);
            }
        }

        public void SendGeoSiteCertificateExpiredEmail(string host, string expiryDate)
        {
            var personalisation = new Dictionary<string, dynamic>
            {
                {"host", host},
                {"expiryDate", expiryDate},
                {"Environment", GetEnvironmentNameForTestEnvironments()}
            };

            foreach (string emailAddress in Global.GeoDistributionList)
            {
                var notifyEmail = new NotifyEmail
                {
                    EmailAddress = emailAddress,
                    TemplateId = EmailTemplates.GeoSiteCertificateExpiredEmail,
                    Personalisation = personalisation
                };

                SendEmail(notifyEmail);
            }
        }

        private void SendEmail(NotifyEmail notifyEmail)
        {
            govNotifyApi.SendEmail(notifyEmail);
        }

        private static string GetEnvironmentNameForTestEnvironments()
        {
            return Config.IsProduction() ? "" : $"[{Config.EnvironmentName}] ";
        }

    }

    public static class EmailTemplates
    {

        public const string GeoSiteCertificateSoonToExpireEmail = "f05abb4f-55b3-472c-8c18-b568b6f2b4c8";
        public const string GeoSiteCertificateExpiredEmail = "a928f9e7-962c-447f-a19e-466cfbe61740";

    }

}
