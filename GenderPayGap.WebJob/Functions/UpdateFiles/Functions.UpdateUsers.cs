using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using GenderPayGap.Core;
using GenderPayGap.Database;
using Microsoft.Azure.WebJobs;
using Microsoft.EntityFrameworkCore;

namespace GenderPayGap.WebJob
{
    public partial class Functions
    {

        public async Task UpdateUsers([TimerTrigger("30 2 * * *" /* 02:30 once per day */)]
            TimerInfo timer)
        {
            var runId = JobHelpers.CreateRunId();
            var startTime = DateTime.Now;
            JobHelpers.LogFunctionStart(runId,  nameof(UpdateUsers), startTime);
            
            try
            {
                string filePath = Path.Combine(Global.DownloadsPath, Filenames.Users);

                //Dont execute on startup if file already exists
                if (!StartedJobs.Contains(nameof(UpdateUsers)) && await Global.FileRepository.GetFileExistsAsync(filePath))
                {
                    return;
                }

                await UpdateUsersAsync(filePath);
                JobHelpers.LogFunctionEnd(runId, nameof(UpdateUsers), startTime);
            }
            catch (Exception ex)
            {
                JobHelpers.LogFunctionError(runId, nameof(UpdateUsers), startTime, ex );
                
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
