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

        public async Task UpdateScopes([TimerTrigger("10 * * * *" /* once per hour, at 10 minutes past the hour */)]
            TimerInfo timer,
            ILogger log)
        {
            try
            {
                string filePath = Path.Combine(Global.DownloadsPath, Filenames.OrganisationScopes);

                //Dont execute on startup if file already exists
                if (!StartedJobs.Contains(nameof(UpdateScopes)))
                {
                    IEnumerable<string> files = await Global.FileRepository.GetFilesAsync(
                        Global.DownloadsPath,
                        $"{Path.GetFileNameWithoutExtension(Filenames.OrganisationScopes)}*{Path.GetExtension(Filenames.OrganisationScopes)}");
                    if (files.Any())
                    {
                        return;
                    }
                }

                await UpdateScopesAsync(filePath);

                log.LogDebug($"Executed {nameof(UpdateScopes)}:successfully");
            }
            catch (Exception ex)
            {
                string message = $"Failed {nameof(UpdateScopes)}:{ex.Message}";

                //Send Email to GEO reporting errors
                await _Messenger.SendGeoMessageAsync("GPG - WEBJOBS ERROR", message);
                //Rethrow the error
                throw;
            }
            finally
            {
                StartedJobs.Add(nameof(UpdateScopes));
            }
        }

        public async Task UpdateScopesAsync(string filePath)
        {
            if (RunningJobs.Contains(nameof(UpdateScopes)))
            {
                return;
            }

            RunningJobs.Add(nameof(UpdateScopes));
            try
            {
                await WriteRecordsPerYearAsync(
                        filePath,
                        year => Task.FromResult(_ScopeBL.GetScopesFileModelByYear(year).ToList()))
                    .ConfigureAwait(false);
            }
            finally
            {
                RunningJobs.Remove(nameof(UpdateScopes));
            }
        }

    }
}
