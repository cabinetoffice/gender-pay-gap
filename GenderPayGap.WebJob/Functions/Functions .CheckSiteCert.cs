﻿using System;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using GenderPayGap.Core;
using GenderPayGap.Extensions;
using GenderPayGap.Extensions.AspNetCore;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace GenderPayGap.WebJob
{
    public partial class Functions
    {

        public async Task CheckSiteCertAsync([TimerTrigger("20 3 * * *" /* 03:20 once per day */)] TimerInfo timer, ILogger log)
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

                        DateTime expires = cert.GetExpirationDateString().ToDateTime();
                        if (expires < VirtualDateTime.UtcNow)
                        {
                            await _Messenger.SendGeoMessageAsync(
                                "GPG - WEBSITE CERTIFICATE EXPIRED",
                                $"The website certificate for '{Global.ExternalHost}' expired on {expires.ToFriendlyDate()} and needs replacing immediately.");
                        }
                        else
                        {
                            TimeSpan remainingTime = expires - VirtualDateTime.Now;

                            if (expires < VirtualDateTime.UtcNow.AddDays(Global.CertExpiresWarningDays))
                            {
                                await _Messenger.SendGeoMessageAsync(
                                    "GPG - WEBSITE CERTIFICATE EXPIRING",
                                    $"The website certificate for '{Global.ExternalHost}' is due expire on {expires.ToFriendlyDate()} and will need replacing within {remainingTime.ToFriendly(maxParts: 2)}.");
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
