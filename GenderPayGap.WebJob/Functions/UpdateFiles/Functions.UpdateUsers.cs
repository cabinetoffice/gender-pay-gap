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

        public async Task UpdateUsers([TimerTrigger("30 2 * * *" /* 02:30 once per day */)]
            TimerInfo timer,
            ILogger log)
        {
            try
            {
                string filePath = Path.Combine(Global.DownloadsPath, Filenames.Users);

                //Dont execute on startup if file already exists
                if (!StartedJobs.Contains(nameof(UpdateUsers)) && await Global.FileRepository.GetFileExistsAsync(filePath))
                {
                    return;
                }

                await UpdateUsersAsync(filePath);
                log.LogDebug($"Executed {nameof(UpdateUsers)}:successfully");
            }
            catch (Exception ex)
            {
                string message = $"Failed {nameof(UpdateUsers)}:{ex.Message}";

                //Send Email to GEO reporting errors
                await _Messenger.SendGeoMessageAsync("GPG - WEBJOBS ERROR", message);
                //Rethrow the error
                throw;
            }
            finally
            {
                StartedJobs.Add(nameof(UpdateUsers));
            }
        }

        public async Task UpdateUsersAsync(string filePath)
        {
            if (RunningJobs.Contains(nameof(UpdateUsers)))
            {
                return;
            }

            RunningJobs.Add(nameof(UpdateUsers));
            try
            {
                List<User> users = await _DataRepository.GetAll<User>().ToListAsync();
                var records = users.Where(u => !u.IsAdministrator())
                    .OrderBy(u => u.Lastname)
                    .Select(
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
                            u.ContactOrganisation,
                            u.EmailVerifySendDate,
                            u.EmailVerifiedDate,
                            VerifyUrl = u.GetVerifyUrl(),
                            PasswordResetUrl = u.GetPasswordResetUrl(),
                            u.Status,
                            u.StatusDate,
                            u.StatusDetails,
                            u.Created
                        })
                    .ToList();
                await Core.Classes.Extensions.SaveCSVAsync(Global.FileRepository, records, filePath);
            }
            finally
            {
                RunningJobs.Remove(nameof(UpdateUsers));
            }
        }

    }
}
