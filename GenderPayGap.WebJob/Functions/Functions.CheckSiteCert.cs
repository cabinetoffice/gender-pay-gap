using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using GenderPayGap.Core;
using GenderPayGap.Core.Classes.Logger;
using GenderPayGap.Extensions;
using GenderPayGap.Extensions.AspNetCore;
using Microsoft.Azure.WebJobs;

namespace GenderPayGap.WebJob
{
    public partial class Functions
    {

        public async Task CheckSiteCertAsync([TimerTrigger("20 3 * * *" /* 03:20 once per day */)]
            TimerInfo timer)
        {
            string runId = JobHelpers.CreateRunId();
            DateTime startTime = VirtualDateTime.Now;
            JobHelpers.LogFunctionStart(runId, nameof(CheckSiteCertAsync), startTime);
            try
            {
                if (WebJobGlobal.CertExpiresWarningDays > 0)
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
                                $"The website certificate for '{WebJobGlobal.ExternalHost}' expired on {expires.ToFriendlyDate()} and needs replacing immediately.",
                                new {environment = Config.EnvironmentName, time});

                            emailSendingService.SendGeoSiteCertificateExpiredEmail(
                                WebJobGlobal.ExternalHost,
                                expires.ToFriendlyDate());
                        }
                        else if (expires < VirtualDateTime.UtcNow.AddDays(WebJobGlobal.CertExpiresWarningDays))
                        {
                            TimeSpan remainingTime = expires - VirtualDateTime.Now;

                            CustomLogger.Error(
                                $"The website certificate for '{WebJobGlobal.ExternalHost}' is due expire on {expires.ToFriendlyDate()} and will need replacing within {remainingTime.ToFriendly(maxParts: 2)}.",
                                new {environment = Config.EnvironmentName, time});

                            emailSendingService.SendGeoSiteCertificateSoonToExpireEmail(
                                WebJobGlobal.ExternalHost,
                                expires.ToFriendlyDate(),
                                remainingTime.ToFriendly(maxParts: 2));
                        }
                    }
                }

                JobHelpers.LogFunctionEnd(runId, nameof(CheckSiteCertAsync), startTime);
            }
            catch (Exception ex)
            {
                JobHelpers.LogFunctionError(runId, nameof(CheckSiteCertAsync), startTime, ex);
                //Rethrow the error
                throw;
            }
        }

    }
}
