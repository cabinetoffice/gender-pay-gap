using System;
using System.Collections.Generic;
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

        public async Task UpdateOrganisationsAsync([TimerTrigger("20 1 * * *" /* 01:20 once per day */)]
            TimerInfo timer,
            ILogger log)
        {
            var runId = JobHelpers.CreateRunId();
            var startTime = DateTime.Now;
            JobHelpers.LogFunctionStart(runId,  nameof(UpdateOrganisationsAsync), startTime);
            
            try
            {
                string filePath = Path.Combine(Global.DownloadsPath, Filenames.Organisations);

                //Dont execute on startup if file already exists
                if (!StartedJobs.Contains(nameof(UpdateOrganisationsAsync)))
                {
                    IEnumerable<string> files = await Global.FileRepository.GetFilesAsync(
                        Global.DownloadsPath,
                        $"{Path.GetFileNameWithoutExtension(Filenames.Organisations)}*{Path.GetExtension(Filenames.Organisations)}");
                    if (files.Any())
                    {
                        return;
                    }
                }

                await UpdateOrganisationsAsync(filePath);

                JobHelpers.LogFunctionEnd(runId, nameof(UpdateOrganisationsAsync), startTime);
            }
            catch (Exception ex)
            {
                JobHelpers.LogFunctionError(runId, nameof(UpdateOrganisationsAsync), startTime, ex );
                
                //Rethrow the error
                throw;
            }
            finally
            {
                StartedJobs.Add(nameof(UpdateOrganisationsAsync));
            }
        }

        public async Task UpdateOrganisationsAsync(string filePath)
        {
            if (RunningJobs.Contains(nameof(UpdateOrganisationsAsync)))
            {
                return;
            }

            RunningJobs.Add(nameof(UpdateOrganisationsAsync));
            try
            {
                await WriteRecordsPerYearAsync(
                    filePath,
                    _OrganisationBL.GetOrganisationsFileModelByYearAsync);
            }
            finally
            {
                RunningJobs.Remove(nameof(UpdateOrganisationsAsync));
            }
        }

    }
}
