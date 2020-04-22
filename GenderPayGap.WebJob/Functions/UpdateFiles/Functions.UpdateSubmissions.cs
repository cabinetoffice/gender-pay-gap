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
            var runId = JobHelpers.CreateRunId();
            var startTime = DateTime.Now;
            JobHelpers.LogFunctionStart(runId,  nameof(UpdateSubmissions), startTime);
            
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

                JobHelpers.LogFunctionEnd(runId, nameof(UpdateSubmissions), startTime);
            }
            catch (Exception ex)
            {
                JobHelpers.LogFunctionError(runId, nameof(UpdateSubmissions), startTime, ex );
                
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
