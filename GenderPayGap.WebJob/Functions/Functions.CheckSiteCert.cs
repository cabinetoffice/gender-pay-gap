using System;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using GenderPayGap.Core;
using GenderPayGap.Core.Classes.Logger;
using GenderPayGap.Extensions;
using GenderPayGap.Extensions.AspNetCore;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace GenderPayGap.WebJob
{
    public partial class Functions
    {

        public async Task CheckSiteCertAsync([TimerTrigger("20 3 * * *" /* 03:20 once per day */)]
            TimerInfo timer,
            ILogger log)
        {
            string runId = CreateRunId();
            DateTime startTime = VirtualDateTime.Now;
            LogFunctionStart(runId, nameof(CheckSiteCertAsync), startTime);
            try
            {
                if (Global.CertExpiresWarningDays > 0)
                {
                    //Get the cert thumbprint
                    string certThumprint = Config.Configuration["WEBSITE_LOAD_CERTIFICATES"].SplitI(";").FirstOrDefault();
                    if (string.IsNullOrWhiteSpace(certThumprint))
                    {
                        certThumprint = Config.Configuration["CertThumprint"].SplitI(";").FirstOrDefault();
                    }

                    if (!string.IsNullOrWhiteSpace(certThumprint))
                    {
                        //Load the cert from the thumprint
                        X509Certificate2 cert = HttpsCertificate.LoadCertificateFromThumbprint(certThumprint);

                        var expires = cert.GetExpirationDateString().ToDateTime();
                        DateTime time = VirtualDateTime.Now;
                        if (expires < VirtualDateTime.UtcNow)
                        {
                            CustomLogger.Error(
                                $"The website certificate for '{Global.ExternalHost}' expired on {expires.ToFriendlyDate()} and needs replacing immediately.",
                                new {environment = Config.EnvironmentName, time});

                            emailSendingService.SendGeoSiteCertificateExpiredEmail(
                                Config.GetAppSetting("GEODistributionList"),
                                Global.ExternalHost,
                                expires.ToFriendlyDate());
                        }
                        else
                        {
                            TimeSpan remainingTime = expires - VirtualDateTime.Now;

                            if (expires < VirtualDateTime.UtcNow.AddDays(Global.CertExpiresWarningDays))
                            {
                                CustomLogger.Error(
                                    $"The website certificate for '{Global.ExternalHost}' is due expire on {expires.ToFriendlyDate()} and will need replacing within {remainingTime.ToFriendly(maxParts: 2)}.",
                                    new {environment = Config.EnvironmentName, time});

                                emailSendingService.SendGeoSiteCertificateSoonToExpireEmail(
                                    Config.GetAppSetting("GEODistributionList"),
                                    Global.ExternalHost,
                                    expires.ToFriendlyDate(),
                                    remainingTime.ToFriendly(maxParts: 2));
                            }
                        }
                    }
                }

                LogFunctionEnd(runId, nameof(CheckSiteCertAsync), startTime);
            }
            catch (Exception ex)
            {
                LogFunctionError(runId, nameof(CheckSiteCertAsync), startTime, ex);
                //Rethrow the error
                throw;
            }
        }

    }
}
