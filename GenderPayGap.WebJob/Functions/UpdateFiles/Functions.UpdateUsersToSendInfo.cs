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

        public async Task UpdateUsersToSendInfo([TimerTrigger("50 2 * * *" /* 02:50 once per day */)]
            TimerInfo timer,
            ILogger log)
        {
            try
            {
                string filePath = Path.Combine(Global.DownloadsPath, Filenames.SendInfo);

                //Dont execute on startup if file already exists
                if (!StartedJobs.Contains(nameof(UpdateUsersToSendInfo)) && await Global.FileRepository.GetFileExistsAsync(filePath))
                {
                    return;
                }

                await UpdateUsersToSendInfoAsync(filePath);
                log.LogDebug($"Executed {nameof(UpdateUsersToSendInfo)}:successfully");
            }
            catch (Exception ex)
            {
                string message = $"Failed {nameof(UpdateUsersToSendInfo)}:{ex.Message}";

                //Send Email to GEO reporting errors
                await _Messenger.SendGeoMessageAsync("GPG - WEBJOBS ERROR", message);
                //Rethrow the error
                throw;
            }
            finally
            {
                StartedJobs.Add(nameof(UpdateUsersToSendInfo));
            }
        }

        public async Task UpdateUsersToSendInfoAsync(string filePath)
        {
            if (RunningJobs.Contains(nameof(UpdateUsersToSendInfo)))
            {
                return;
            }

            RunningJobs.Add(nameof(UpdateUsersToSendInfo));
            try
            {
                List<User> users = await _DataRepository.GetAll<User>()
                    .Where(
                        user => user.Status == UserStatuses.Active
                                && user.UserSettings.Any(us => us.Key == UserSettingKeys.SendUpdates && us.Value.ToLower() == "true"))
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
                RunningJobs.Remove(nameof(UpdateUsersToSendInfo));
            }
        }

    }
}
