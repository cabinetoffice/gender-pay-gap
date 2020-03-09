using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using GenderPayGap.Core;
using GenderPayGap.Core.Classes;
using GenderPayGap.Core.Classes.Logger;
using GenderPayGap.Core.Interfaces;
using GenderPayGap.Database;
using GenderPayGap.Database.Models;
using GenderPayGap.Extensions;
using GenderPayGap.Extensions.AspNetCore;
using Microsoft.Azure.WebJobs;
using Newtonsoft.Json;

namespace GenderPayGap.WebJob
{
    public partial class Functions
    {

        // This trigger is set to run every hour, on the hour
        public void SendReminderEmails([TimerTrigger("25 * * * *" /* once per hour, at 25 minutes past the hour */)] TimerInfo timer)
        {
            var runId = CreateRunId();
            var startTime = VirtualDateTime.Now;
            LogFunctionStart(runId,  nameof(SendReminderEmails), startTime);
            
            List<int> reminderDays = GetReminderEmailDays();
            if (reminderDays.Count == 0)
            {
                var endTime = VirtualDateTime.Now;
                CustomLogger.Information($"Function finished: {nameof(SendReminderEmails)}. No ReminderEmailDays set.",
                    new
                {
                    runId,
                    environment = Config.EnvironmentName,
                    endTime,
                    TimeTakenInSeconds = (endTime - startTime).TotalSeconds
                });
                return;
            }

            SendReminderEmailsForSectorType(SectorTypes.Private, runId, startTime);
            SendReminderEmailsForSectorType(SectorTypes.Public, runId, startTime);

            LogFunctionEnd(runId, nameof(SendReminderEmails), startTime);
        }

        private void SendReminderEmailsForSectorType(SectorTypes sector, string runId, DateTime startTime)
        {
            if (IsAfterEarliestReminder(sector))
            {
                DateTime latestReminderEmailDate = GetLatestReminderEmailDate(sector);

                IEnumerable<User> usersUncheckedSinceLatestReminderDate = _DataRepository.GetAll<User>()
                    .Where(user => !user.ReminderEmails.Any(re => re.SectorType == sector && re.DateChecked > latestReminderEmailDate));

                foreach (User user in usersUncheckedSinceLatestReminderDate)
                {
                    if (VirtualDateTime.Now > startTime.AddMinutes(59))
                    {
                        var endTime = VirtualDateTime.Now;
                        CustomLogger.Information($"Function finished: {nameof(SendReminderEmails)}. Hit timeout break.",
                            new
                            {
                                runId,
                                environment = Config.EnvironmentName,
                                endTime,
                                TimeTakenInSeconds = (endTime - startTime).TotalSeconds
                            });
                        break;
                    }

                    CheckUserAndSendReminderEmailsForSectorType(user, sector);
                }
            }
        }

        private void CheckUserAndSendReminderEmailsForSectorType(User user, SectorTypes sector)
        {
            List<Organisation> inScopeOrganisationsForUserAndSectorTypeThatStillNeedToReport = user.UserOrganisations
                .Where(uo => uo.PINConfirmedDate != null)
                .Select(uo => uo.Organisation)
                .Where(o => o.SectorType == sector)
                .Where(
                    o =>
                        o.LatestScope.ScopeStatus == ScopeStatuses.InScope
                        || o.LatestScope.ScopeStatus == ScopeStatuses.PresumedInScope)
                .Where(
                    o =>
                        o.LatestReturn == null
                        || o.LatestReturn.AccountingDate != o.SectorType.GetAccountingStartDate()
                        || o.LatestReturn.Status != ReturnStatuses.Submitted)
                .ToList();

            SendReminderEmailsForSectorType(user, inScopeOrganisationsForUserAndSectorTypeThatStillNeedToReport, sector);
        }

        private void SendReminderEmailsForSectorType(
            User user,
            List<Organisation> inScopeOrganisationsForUserAndSectorTypeThatStillNeedToReport,
            SectorTypes sectorType)
        {
            try
            {
                bool anyOrganisationsToEmailAbout = inScopeOrganisationsForUserAndSectorTypeThatStillNeedToReport.Count > 0;
                if (anyOrganisationsToEmailAbout)
                {
                    SendReminderEmail(user, sectorType, inScopeOrganisationsForUserAndSectorTypeThatStillNeedToReport);
                }
                SaveReminderEmailRecord(user, sectorType, anyOrganisationsToEmailAbout);
            }
            catch (Exception ex)
            {
                CustomLogger.Error(
                    "Failed whilst sending or saving reminder email",
                    new
                    {
                        user.UserId,
                        SectorType = sectorType,
                        OrganisationIds = inScopeOrganisationsForUserAndSectorTypeThatStillNeedToReport.Select(o => o.OrganisationId),
                        Exception = ex.Message
                    });
            }
        }

        private void SendReminderEmail(User user,
            SectorTypes sectorType,
            List<Organisation> organisations)
        {
            var personalisation = new Dictionary<string, dynamic>
            {
                {"DeadlineDate", sectorType.GetAccountingStartDate().AddYears(1).AddDays(-1).ToString("d MMMM yyyy")},
                {"DaysUntilDeadline", sectorType.GetAccountingStartDate().AddYears(1).Subtract(VirtualDateTime.Now).Days},
                {"OrganisationNames", GetOrganisationNameString(organisations)},
                {"OrganisationIsSingular", organisations.Count == 1},
                {"OrganisationIsPlural", organisations.Count > 1},
                {"SectorType", sectorType.ToString().ToLower()},
                {"Environment", Config.IsProduction() ? "" : $"[{Config.EnvironmentName}] "}
            };

            var notifyEmail = new NotifyEmail
            {
                EmailAddress = user.EmailAddress, TemplateId = "db15432c-9eda-4df4-ac67-290c7232c546", Personalisation = personalisation
            };
            
            govNotifyApi.SendEmail(notifyEmail);
        }
        
        private static void SaveReminderEmailRecord(User user, SectorTypes sectorType, bool wasAnEmailSent)
        {
            var reminderEmailRecord = new ReminderEmail
            {
                UserId = user.UserId,
                SectorType = sectorType,
                DateChecked = VirtualDateTime.Now,
                EmailSent = wasAnEmailSent
            };

            var dataRepository = Program.ContainerIOC.Resolve<IDataRepository>();
            dataRepository.Insert(reminderEmailRecord);
            dataRepository.SaveChangesAsync().Wait();
        }

        private string GetOrganisationNameString(List<Organisation> organisations)
        {
            if (organisations.Count == 1)
            {
                return organisations[0].OrganisationName;
            }

            if (organisations.Count == 2)
            {
                return $"{organisations[0].OrganisationName} and {organisations[1].OrganisationName}";
            }

            return $"{organisations[0].OrganisationName} and {organisations.Count - 1} other organisations";
        }

        private bool IsAfterEarliestReminder(SectorTypes sectorType)
        {
            return VirtualDateTime.Now > GetEarliestReminderDate(sectorType);
        }

        private static DateTime GetEarliestReminderDate(SectorTypes sectorType)
        {
            List<int> reminderEmailDays = GetReminderEmailDays();
            int earliestReminderDay = reminderEmailDays[reminderEmailDays.Count - 1];

            DateTime deadlineDate = GetDeadlineDate(sectorType);
            return deadlineDate.AddDays(-earliestReminderDay);
        }

        private static DateTime GetLatestReminderEmailDate(SectorTypes sectorType)
        {
            return GetReminderDates(sectorType)
                .Where(reminderDate => reminderDate < VirtualDateTime.Now)
                .OrderByDescending(reminderDate => reminderDate)
                .FirstOrDefault();
        }

        private static List<DateTime> GetReminderDates(SectorTypes sectorType)
        {
            List<int> reminderDays = GetReminderEmailDays();
            DateTime deadlineDate = GetDeadlineDate(sectorType);

            return reminderDays.Select(reminderDay => deadlineDate.AddDays(-reminderDay)).ToList();
        }

        private static DateTime GetDeadlineDate(SectorTypes sectorType)
        {
            return sectorType.GetAccountingStartDate().AddYears(1);
        }

        private static List<int> GetReminderEmailDays()
        {
            var reminderEmailDays = JsonConvert.DeserializeObject<List<int>>(Config.GetAppSetting("ReminderEmailDays"));
            reminderEmailDays.Sort();
            return reminderEmailDays;
        }

    }
}
