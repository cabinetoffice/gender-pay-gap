using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using GenderPayGap.Core;
using GenderPayGap.Core.Classes;
using GenderPayGap.Core.Classes.Logger;
using GenderPayGap.Core.Helpers;
using GenderPayGap.Core.Interfaces;
using GenderPayGap.Database;
using GenderPayGap.Database.Models;
using GenderPayGap.Extensions;
using GenderPayGap.Extensions.AspNetCore;
using GenderPayGap.WebUI.Services;
using Newtonsoft.Json;

namespace GenderPayGap.WebUI.BackgroundJobs.ScheduledJobs
{
    public class SendReminderEmailsJob
    {

        private readonly IDataRepository dataRepository;
        private readonly EmailSendingService emailSendingService;

        public SendReminderEmailsJob(
            IDataRepository dataRepository,
            EmailSendingService emailSendingService)
        {
            this.dataRepository = dataRepository;
            this.emailSendingService = emailSendingService;
        }


        public void SendReminderEmails()
        {
            JobHelpers.RunAndLogJob(SendReminderEmailsAction, nameof(SendReminderEmails));
        }

        private void SendReminderEmailsAction(string runId)
        {
            DateTime startTime = VirtualDateTime.Now;

            List<int> reminderDays = GetReminderEmailDays();
            if (reminderDays.Count != 0)
            {
                SendReminderEmailsForSectorType(SectorTypes.Private, runId, startTime);
                SendReminderEmailsForSectorType(SectorTypes.Public, runId, startTime);
            }
            else
            {
                var endTime = VirtualDateTime.Now;
                CustomLogger.Information(
                    $"Function finished: {nameof(SendReminderEmails)}. No ReminderEmailDays set.",
                    new {runId, environment = Config.EnvironmentName, endTime, TimeTakenInSeconds = (endTime - startTime).TotalSeconds});
            }
        }

        private void SendReminderEmailsForSectorType(SectorTypes sector, string runId, DateTime startTime)
        {
            var activeReportingYears = ReportingYearsHelper.GetReportingYears(sector);
            foreach (int year in activeReportingYears)
            {
                if (IsAfterEarliestReminderForReportingYear(sector, year))
                {
                    SendReminderEmailForSectorTypeAndYear(sector, year, runId, startTime);
                }
            }
        }

        private void SendReminderEmailForSectorTypeAndYear(SectorTypes sector, int year, string runId, DateTime startTime)
        {
            DateTime latestReminderEmailDate = GetLatestReminderEmailDateForReportingYear(sector, year);

            IEnumerable<User> usersUncheckedSinceLatestReminderDate = dataRepository.GetAll<User>()
                .Where(user => !user.HasBeenAnonymised)
                .Where(UserHasNotBeenEmailedYet(sector, latestReminderEmailDate));

            foreach (User user in usersUncheckedSinceLatestReminderDate)
            {
                if (VirtualDateTime.Now > startTime.AddMinutes(45))
                {
                    var endTime = VirtualDateTime.Now;
                    CustomLogger.Information(
                        $"Function finished: {nameof(SendReminderEmails)}. Hit timeout break.",
                        new
                        {
                            runId,
                            environment = Config.EnvironmentName,
                            endTime,
                            TimeTakenInSeconds = (endTime - startTime).TotalSeconds
                        });
                    break;
                }

                try
                {
                    SendReminderEmailRecordIfNotInProgress(user, sector, latestReminderEmailDate, year);
                }
                catch (Exception ex)
                {
                    CustomLogger.Information(
                        "Failed whilst saving or sending reminder email",
                        new {user.UserId, SectorType = sector, Exception = ex.Message});
                }
            }
        }

        private Expression<Func<User, bool>> UserHasNotBeenEmailedYet(SectorTypes sector, DateTime reminderEmailDate)
        {
            return user => !user.ReminderEmails
                .Any(
                    reminderEmail => reminderEmail.SectorType == sector
                                     && reminderEmail.ReminderDate.HasValue
                                     && reminderEmail.ReminderDate.Value.Date == reminderEmailDate.Date
                                     && reminderEmail.Status == ReminderEmailStatus.Completed);
        }

        private void SendReminderEmailsForReportingYear(User user, int year, ReminderEmail reminderEmail)
        {
            var snapshotDate = reminderEmail.SectorType.GetAccountingStartDate(year);

            List<Organisation> inScopeActiveOrganisationsForUserAndSectorTypeThatStillNeedToReport = user.UserOrganisations
                .Where(uo => uo.HasBeenActivated())
                .Select(uo => uo.Organisation)
                .Where(o => o.Status == OrganisationStatuses.Active)
                .Where(o => o.SectorType == reminderEmail.SectorType)
                .Where(OrganisationIsInScopeForSnapshotDate(snapshotDate))
                .Where(OrganisationHasNotReportedForSnapshotDate(snapshotDate))
                .ToList();

            CheckAndSendReminderEmailsForReportingYear(
                user,
                inScopeActiveOrganisationsForUserAndSectorTypeThatStillNeedToReport,
                year,
                reminderEmail);
        }

        private Func<Organisation, bool> OrganisationIsInScopeForSnapshotDate(DateTime snapshotDate)
        {
            return o => o.OrganisationScopes.Any(
                s => s.Status == ScopeRowStatuses.Active
                     && s.SnapshotDate == snapshotDate
                     && (s.ScopeStatus == ScopeStatuses.InScope || s.ScopeStatus == ScopeStatuses.PresumedInScope));
        }

        private Func<Organisation, bool> OrganisationHasNotReportedForSnapshotDate(DateTime snapshotDate)
        {
            return o => !o.Returns.Any(r => r.Status == ReturnStatuses.Submitted && r.AccountingDate == snapshotDate);
        }

        private void SendReminderEmailRecordIfNotInProgress(User user, SectorTypes sectorType, DateTime reminderDate, int year)
        {
            var reminderEmailRecord = GetReminderEmailRecord(user, sectorType, reminderDate);

            if (reminderEmailRecord == null)
            {
                reminderEmailRecord = CreateAndGetReminderEmailRecord(user, sectorType, reminderDate);
                SendReminderEmailsForReportingYear(
                    user,
                    year,
                    reminderEmailRecord);
            }
            else if (ReminderEmailSendingHasExpired(reminderEmailRecord))
            {
                reminderEmailRecord = GetRecheckedReminderEmail(reminderEmailRecord);
                SendReminderEmailsForReportingYear(
                    user,
                    year,
                    reminderEmailRecord);
            }
        }

        private ReminderEmail GetReminderEmailRecord(User user, SectorTypes sectorType, DateTime reminderDate)
        {
            return dataRepository
                .GetAll<ReminderEmail>()
                .FirstOrDefault(
                    re => re.UserId == user.UserId
                          && re.ReminderDate.HasValue
                          && re.ReminderDate.Value.Date == reminderDate.Date
                          && re.SectorType == sectorType);
        }

        private ReminderEmail CreateAndGetReminderEmailRecord(User user, SectorTypes sectorType, DateTime reminderDate)
        {
            var reminderEmailRecord = new ReminderEmail
            {
                UserId = user.UserId,
                SectorType = sectorType,
                DateChecked = VirtualDateTime.Now,
                ReminderDate = reminderDate,
                EmailSent = false,
                Status = ReminderEmailStatus.InProgress
            };
            dataRepository.Insert(reminderEmailRecord);
            dataRepository.SaveChanges();

            return reminderEmailRecord;
        }

        private ReminderEmail GetRecheckedReminderEmail(ReminderEmail reminderEmailRecord)
        {
            reminderEmailRecord.DateChecked = VirtualDateTime.Now;
            dataRepository.SaveChanges();

            return reminderEmailRecord;
        }

        private void CheckAndSendReminderEmailsForReportingYear(
            User user,
            List<Organisation> inScopeOrganisationsForUserAndSectorTypeThatStillNeedToReport,
            int year,
            ReminderEmail reminderEmail)
        {
            bool anyOrganisationsToEmailAbout = inScopeOrganisationsForUserAndSectorTypeThatStillNeedToReport.Count > 0;
            if (anyOrganisationsToEmailAbout)
            {
                SendReminderEmailForReportingYear(
                    user,
                    reminderEmail.SectorType,
                    inScopeOrganisationsForUserAndSectorTypeThatStillNeedToReport,
                    year);
            }

            MarkReminderEmailAsCompleted(
                reminderEmail,
                anyOrganisationsToEmailAbout);
        }

        private void SendReminderEmailForReportingYear(User user,
            SectorTypes sectorType,
            List<Organisation> organisations,
            int year)
        {
            var deadlineDate = GetDeadlineDateForReportingYear(sectorType, year);
            emailSendingService.SendReminderEmail(
                emailAddress: user.EmailAddress,
                deadlineDate: deadlineDate.AddDays(-1).ToString("d MMMM yyyy"),
                daysUntilDeadline: deadlineDate.Subtract(VirtualDateTime.Now).Days,
                organisationNames: GetOrganisationNameString(organisations),
                organisationIsSingular: organisations.Count == 1,
                organisationIsPlural: organisations.Count > 1,
                sectorType: sectorType.ToString().ToLower());
        }


        private bool ReminderEmailSendingHasExpired(ReminderEmail reminderEmailRecord)
        {
            return reminderEmailRecord.Status == ReminderEmailStatus.InProgress
                   && reminderEmailRecord.DateChecked < VirtualDateTime.Now.AddMinutes(-15);
        }

        private void MarkReminderEmailAsCompleted(ReminderEmail reminderEmail, bool wasAnEmailSent)
        {
            reminderEmail.Status = ReminderEmailStatus.Completed;
            reminderEmail.DateChecked = VirtualDateTime.Now;
            reminderEmail.EmailSent = wasAnEmailSent;

            dataRepository.SaveChanges();
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

            return $"{organisations[0].OrganisationName} and {organisations.Count - 1} other employers";
        }

        private bool IsAfterEarliestReminderForReportingYear(SectorTypes sectorType, int year)
        {
            return VirtualDateTime.Now > GetEarliestReminderDateForReportingYear(sectorType, year);
        }

        private static DateTime GetEarliestReminderDateForReportingYear(SectorTypes sectorType, int year)
        {
            List<int> reminderEmailDays = GetReminderEmailDays();
            int earliestReminderDay = reminderEmailDays[reminderEmailDays.Count - 1];

            DateTime deadlineDate = GetDeadlineDateForReportingYear(sectorType, year);
            return deadlineDate.AddDays(-earliestReminderDay);
        }

        private static DateTime GetLatestReminderEmailDateForReportingYear(SectorTypes sectorType, int year)
        {
            return GetReminderDatesForReportingYear(sectorType, year)
                .Where(reminderDate => reminderDate < VirtualDateTime.Now)
                .OrderByDescending(reminderDate => reminderDate)
                .FirstOrDefault();
        }

        private static List<DateTime> GetReminderDatesForReportingYear(SectorTypes sectorType, int year)
        {
            List<int> reminderDays = GetReminderEmailDays();
            DateTime deadlineDate = GetDeadlineDateForReportingYear(sectorType, year);

            return reminderDays.Select(reminderDay => deadlineDate.AddDays(-reminderDay)).ToList();
        }

        private static DateTime GetDeadlineDateForReportingYear(SectorTypes sectorType, int year)
        {
            return ReportingYearsHelper.GetDeadlineForAccountingDate(sectorType.GetAccountingStartDate(year)).AddDays(1);
        }

        private static List<int> GetReminderEmailDays()
        {
            var reminderEmailDays = JsonConvert.DeserializeObject<List<int>>(Global.ReminderEmailDays);
            reminderEmailDays.Sort();
            return reminderEmailDays;
        }

    }
}
