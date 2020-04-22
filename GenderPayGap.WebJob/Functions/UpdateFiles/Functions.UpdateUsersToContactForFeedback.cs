using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using GenderPayGap.Core;
using GenderPayGap.Database;
using Microsoft.Azure.WebJobs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GenderPayGap.WebJob
{
    public partial class Functions
    {

        public async Task UpdateUsersToContactForFeedback([TimerTrigger("40 2 * * *" /* 02:40 once per day */)]
            TimerInfo timer,
            ILogger log)
        {
            var runId = JobHelpers.CreateRunId();
            var startTime = DateTime.Now;
            JobHelpers.LogFunctionStart(runId,  nameof(UpdateUsersToContactForFeedback), startTime);
            
            try
            {
                string filePath = Path.Combine(Global.DownloadsPath, Filenames.AllowFeedback);

                //Dont execute on startup if file already exists
                if (!StartedJobs.Contains(nameof(UpdateUsersToContactForFeedback))
                    && await Global.FileRepository.GetFileExistsAsync(filePath))
                {
                    return;
                }

                await UpdateUsersToContactForFeedbackAsync(filePath);
                JobHelpers.LogFunctionEnd(runId, nameof(UpdateUsersToContactForFeedback), startTime);
                
            }
            catch (Exception ex)
            {
                JobHelpers.LogFunctionError(runId, nameof(UpdateUsersToContactForFeedback), startTime, ex );
                //Rethrow the error
                throw;
            }
            finally
            {
                StartedJobs.Add(nameof(UpdateUsersToContactForFeedback));
            }
        }

        public async Task UpdateUsersToContactForFeedbackAsync(string filePath)
        {
            if (RunningJobs.Contains(nameof(UpdateUsersToContactForFeedback)))
            {
                return;
            }

            RunningJobs.Add(nameof(UpdateUsersToContactForFeedback));
            try
            {
                List<User> users = await _DataRepository.GetAll<User>()
                    .Where(
                        user => user.Status == UserStatuses.Active
                                && user.UserSettings.Any(us => us.Key == UserSettingKeys.AllowContact && us.Value.ToLower() == "true"))
                    .ToListAsync();
                var records = users.Select(
                        u => new {
                            u.Firstname,
                            u.Lastname,
                            u.JobTitle,
                            u.EmailAddress,
                            u.ContactFirstName,
                            u.ContactLastName,
                            u.ContactJobTitle,
                            u.ContactEmailAddress,
                            u.ContactPhoneNumber,
                            u.ContactOrganisation
                        })
                    .ToList();
                await Core.Classes.Extensions.SaveCSVAsync(Global.FileRepository, records, filePath);
            }
            finally
            {
                RunningJobs.Remove(nameof(UpdateUsersToContactForFeedback));
            }
        }

    }
}
