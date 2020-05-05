﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using GenderPayGap.Core.Classes.Logger;
using GenderPayGap.Extensions;
using GenderPayGap.Extensions.AspNetCore;
using Microsoft.Azure.WebJobs;
using static GenderPayGap.Extensions.Web;

namespace GenderPayGap.WebJob
{
    public partial class Functions
    {

        public async Task TakeSnapshotAsync([TimerTrigger("20 5 * * *" /* 05:20 once per day */)]
            TimerInfo timer)
        {
            var runId = JobHelpers.CreateRunId();
            var startTime = VirtualDateTime.Now;
            JobHelpers.LogFunctionStart(runId,  nameof(TakeSnapshotAsync), startTime);
            
            try
            {
                string azureStorageConnectionString = Config.GetConnectionString("AzureStorage");
                if (azureStorageConnectionString.Equals("UseDevelopmentStorage=true"))
                {
                    return;
                }

                Dictionary<string, string> connectionString = azureStorageConnectionString.ConnectionStringToDictionary();

                string azureStorageAccount = connectionString["AccountName"];
                string azureStorageKey = connectionString["AccountKey"];
                string azureStorageShareName = Config.GetAppSetting("AzureStorageShareName");

                //Take the snapshot
                await TakeSnapshotAsync(azureStorageAccount, azureStorageKey, azureStorageShareName);

                //Get the list of snapshots
                string response = await ListSnapshotsAsync(azureStorageAccount, azureStorageKey, azureStorageShareName);
                var count = 0;
                if (!string.IsNullOrWhiteSpace(response))
                {
                    XElement xml = XElement.Parse(response);
                    List<string> snapshots =
                        xml.Descendants().Where(e => e.Name.LocalName.EqualsI("Snapshot")).Select(e => e.Value).ToList();
                    //var snapshots = snapshots.Where(e => e.EqualsI("Snapshot")).Select(e=>e.Value).ToList();
                    DateTime deadline = VirtualDateTime.Now.AddDays(0 - Config.GetAppSetting("MaxSnapshotDays").ToInt32(35));
                    foreach (string snapshot in snapshots)
                    {
                        DateTime date = DateTime.Parse(snapshot);
                        if (date > deadline)
                        {
                            continue;
                        }

                        await DeleteSnapshotAsync(azureStorageAccount, azureStorageKey, azureStorageShareName, snapshot);
                        count++;
                    }
                }
                
                DateTime endTime = VirtualDateTime.Now;
                CustomLogger.Information(
                    $"Function finished: {nameof(TakeSnapshotAsync)}. Successfully deleted {count} stale snapshots",
                    new {
                        runId,
                        Environment = Config.EnvironmentName, 
                        endTime, 
                        TimeTakenInSeconds = (endTime - startTime).TotalSeconds
                        
                    });
            }
            catch (Exception ex)
            {
                JobHelpers.LogFunctionError(runId, nameof(TakeSnapshotAsync), startTime, ex );

                //Rethrow the error
                throw;
            }
        }

        public async Task TakeSnapshotAsync()
        {
            try
            {
                string azureStorageConnectionString = Config.GetConnectionString("AzureStorage");
                if (azureStorageConnectionString.Equals("UseDevelopmentStorage=true"))
                {
                    return;
                }

                Dictionary<string, string> connectionString = azureStorageConnectionString.ConnectionStringToDictionary();

                string azureStorageAccount = connectionString["AccountName"];
                string azureStorageKey = connectionString["AccountKey"];
                string azureStorageShareName = Config.GetAppSetting("AzureStorageShareName");

                //Take the snapshot
                await TakeSnapshotAsync(azureStorageAccount, azureStorageKey, azureStorageShareName);

                CustomLogger.Debug($"Executed {nameof(TakeSnapshotAsync)} successfully");
            }
            catch (Exception ex)
            {
                CustomLogger.Error($"Failed webjob:{nameof(TakeSnapshotAsync)}:{ex.Message}", ex);
                throw;
            }
        }

        private static async Task<string> TakeSnapshotAsync(string storageAccount, string storageKey, string shareName)
        {
            var version = "2017-04-17";
            var comp = "snapshot";
            var restype = "share";
            DateTime dt = VirtualDateTime.UtcNow;
            string StringToSign = "PUT\n"
                                  + "\n" // content encoding
                                  + "\n" // content language
                                  + "\n" // content length
                                  + "\n" // content md5
                                  + "\n" // content type
                                  + "\n" // date
                                  + "\n" // if modified since
                                  + "\n" // if match
                                  + "\n" // if none match
                                  + "\n" // if unmodified since
                                  + "\n" // range
                                  + "x-ms-date:"
                                  + dt.ToString("R")
                                  + "\nx-ms-version:"
                                  + version
                                  + "\n" // headers
                                  + $"/{storageAccount}/{shareName}\ncomp:{comp}\nrestype:{restype}";

            string signature = SignAuthHeader(StringToSign, storageKey, storageAccount);
            string authorizationHeader = $"SharedKey {storageAccount}:{signature}";
            string url = $"https://{storageAccount}.file.core.windows.net/{shareName}?restype={restype}&comp={comp}";

            var headers = new Dictionary<string, string>();
            headers.Add("x-ms-date", dt.ToString("R"));
            headers.Add("x-ms-version", version);
            headers.Add("Authorization", authorizationHeader);
            string json = await CallApiAsync(HttpMethods.Put, url, headers: headers);
            return headers["x-ms-snapshot"];
        }


        private static async Task<string> ListSnapshotsAsync(string storageAccount, string storageKey, string shareName)
        {
            var version = "2017-04-17";
            var comp = "list";
            DateTime dt = VirtualDateTime.UtcNow;
            string StringToSign = "GET\n"
                                  + "\n" // content encoding
                                  + "\n" // content language
                                  + "\n" // content length
                                  + "\n" // content md5
                                  + "\n" // content type
                                  + "\n" // date
                                  + "\n" // if modified since
                                  + "\n" // if match
                                  + "\n" // if none match
                                  + "\n" // if unmodified since
                                  + "\n" // range
                                  + "x-ms-date:"
                                  + dt.ToString("R")
                                  + "\nx-ms-version:"
                                  + version
                                  + "\n" // headers
                                  + $"/{storageAccount}/\ncomp:{comp}\ninclude:snapshots\nprefix:{shareName}";

            string signature = SignAuthHeader(StringToSign, storageKey, storageAccount);
            string authorizationHeader = $"SharedKey {storageAccount}:{signature}";
            string url = $"https://{storageAccount}.file.core.windows.net/?comp={comp}&prefix={shareName}&include=snapshots";

            var headers = new Dictionary<string, string>();
            headers.Add("x-ms-date", dt.ToString("R"));
            headers.Add("x-ms-version", version);
            headers.Add("Authorization", authorizationHeader);
            string response = await CallApiAsync(HttpMethods.Get, url, headers: headers);
            return response;
        }

        private static async Task<string> DeleteSnapshotAsync(
            string storageAccount,
            string storageKey,
            string shareName,
            string snapshot)
        {
            var version = "2017-04-17";
            var restype = "share";
            DateTime dt = VirtualDateTime.UtcNow;
            string StringToSign = "DELETE\n"
                                  + "\n" // content encoding
                                  + "\n" // content language
                                  + "\n" // content length
                                  + "\n" // content md5
                                  + "\n" // content type
                                  + "\n" // date
                                  + "\n" // if modified since
                                  + "\n" // if match
                                  + "\n" // if none match
                                  + "\n" // if unmodified since
                                  + "\n" // range
                                  + "x-ms-date:"
                                  + dt.ToString("R")
                                  + "\nx-ms-version:"
                                  + version
                                  + "\n" // headers
                                  + $"/{storageAccount}/{shareName}\nrestype:{restype}\nsharesnapshot:{snapshot}";

            string signature = SignAuthHeader(StringToSign, storageKey, storageAccount);
            string authorizationHeader = $"SharedKey {storageAccount}:{signature}";
            string url = $"https://{storageAccount}.file.core.windows.net/{shareName}?sharesnapshot={snapshot}&restype={restype}";

            var headers = new Dictionary<string, string>();
            headers.Add("x-ms-date", dt.ToString("R"));
            headers.Add("x-ms-version", version);
            headers.Add("Authorization", authorizationHeader);
            string response = await CallApiAsync(HttpMethods.Delete, url, headers: headers);

            CustomLogger.Debug($"{nameof(DeleteSnapshotAsync)}: successfully deleted snapshot:{snapshot}");

            return headers["x-ms-request-id"];
        }


        private static string SignAuthHeader(string canonicalizedString, string key, string account)
        {
            byte[] unicodeKey = Convert.FromBase64String(key);
            using (var hmacSha256 = new HMACSHA256(unicodeKey))
            {
                byte[] dataToHmac = Encoding.UTF8.GetBytes(canonicalizedString);
                return Convert.ToBase64String(hmacSha256.ComputeHash(dataToHmac));
            }
        }

    }
}
