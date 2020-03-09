using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using GenderPayGap.Core;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace GenderPayGap.WebJob
{
    public partial class Functions
    {

        public async Task UpdateSubmissions([TimerTrigger("15 * * * *" /* once per hour, at 15 minutes past the hour */)]
            TimerInfo timer,
            ILogger log)
        {
            try
            {
                string filePath = Path.Combine(Global.DownloadsPath, Filenames.OrganisationSubmissions);

                //Dont execute on startup if file already exists
                if (!StartedJobs.Contains(nameof(UpdateSubmissions))
                    && await Global.FileRepository.GetAnyFileExistsAsync(
                        Global.DownloadsPath,
                        $"{Path.GetFileNameWithoutExtension(Filenames.OrganisationSubmissions)}*{Path.GetExtension(Filenames.OrganisationSubmissions)}")
                )
                {
                    return;
                }

                await UpdateSubmissionsAsync(filePath);

                log.LogDebug($"Executed {nameof(UpdateSubmissions)}:successfully");
            }
            catch (Exception ex)
            {
                string message = $"Failed {nameof(UpdateSubmissions)}:{ex.Message}";

                //Send Email to GEO reporting errors
                await _Messenger.SendGeoMessageAsync("GPG - WEBJOBS ERROR", message);
                //Rethrow the error
                throw;
            }
            finally
            {
                StartedJobs.Add(nameof(UpdateSubmissions));
            }
        }

        public async Task UpdateSubmissionsAsync(string filePath)
        {
            if (RunningJobs.Contains(nameof(UpdateSubmissions)))
            {
                return;
            }

            RunningJobs.Add(nameof(UpdateSubmissions));
            try
            {
                await WriteRecordsPerYearAsync(
                        filePath,
                        year => Task.FromResult(_SubmissionBL.GetSubmissionsFileModelByYear(year).ToList()))
                    .ConfigureAwait(false);
            }
            finally
            {
                RunningJobs.Remove(nameof(UpdateSubmissions));
            }
        }

    }
}
