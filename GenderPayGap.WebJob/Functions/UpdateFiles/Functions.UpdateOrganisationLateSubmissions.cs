using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using GenderPayGap.BusinessLogic.Models;
using GenderPayGap.Core;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace GenderPayGap.WebJob
{
    public partial class Functions
    {

        public async Task UpdateOrganisationLateSubmissions([TimerTrigger("5 * * * *" /* once per hour, at 5 minutes past the hour */)]
            TimerInfo timer,
            ILogger log)
        {
            try
            {
                string filePath = Path.Combine(Global.DownloadsPath, Filenames.OrganisationLateSubmissions);

                //Dont execute on startup if file already exists
                if (!StartedJobs.Contains(nameof(UpdateOrganisationLateSubmissions))
                    && await Global.FileRepository.GetFileExistsAsync(filePath))
                {
                    return;
                }

                await UpdateOrganisationLateSubmissionsAsync(filePath, log);

                log.LogDebug($"Executed {nameof(UpdateOrganisationLateSubmissions)}:successfully");
            }
            catch (Exception ex)
            {
                string message = $"Failed {nameof(UpdateOrganisationLateSubmissions)}:{ex.Message}";

                //Send Email to GEO reporting errors
                await _Messenger.SendGeoMessageAsync("GPG - WEBJOBS ERROR", message);
                //Rethrow the error
                throw;
            }
            finally
            {
                StartedJobs.Add(nameof(UpdateOrganisationLateSubmissions));
            }
        }

        private async Task UpdateOrganisationLateSubmissionsAsync(string filePath, ILogger log)
        {
            string callingMethodName = nameof(UpdateOrganisationLateSubmissions);
            if (RunningJobs.Contains(callingMethodName))
            {
                return;
            }

            RunningJobs.Add(callingMethodName);
            try
            {
                IEnumerable<LateSubmissionsFileModel> records = _SubmissionBL.GetLateSubmissions();
                await Core.Classes.Extensions.SaveCSVAsync(Global.FileRepository, records, filePath);
            }
            finally
            {
                RunningJobs.Remove(callingMethodName);
            }
        }

    }
}
