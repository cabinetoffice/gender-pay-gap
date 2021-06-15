using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;
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
            var activeReportingYears = GetActiveReportingYears(sector);
            foreach (int year in activeReportingYears)
            {
                if (IsAfterEarliestReminderForReportingYear(sector, year))
                {
                    DateTime latestReminderEmailDate = GetLatestReminderEmailDateForReportingYear(sector, year);

                    IEnumerable<User> usersUncheckedSinceLatestReminderDate = dataRepository.GetAll<User>()
                        .Where(user => !user.HasBeenAnonymised)
                        .Where(
                            user => !user.ReminderEmails
                                .Where(
                                    re => re.SectorType == sector
                                          && re.ReminderDate.HasValue
                                          && re.ReminderDate.Value.Date == latestReminderEmailDate.Date)
                                .Any(
                                    reminderEmail => reminderEmail.Status == ReminderEmailStatus.Completed
                                                     || reminderEmail.Status == ReminderEmailStatus.InProgress
                                                     && reminderEmail.DateChecked > VirtualDateTime.Now.AddHours(-1)));

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
                            var reminderEmail = AddNewReminderEmailRecord(user, sector, latestReminderEmailDate);
                            CheckUserAndSendReminderEmailsForSectorTypeAndReportingYear(
                                user,
                                year,
                                reminderEmail);
                        }
                        catch (Exception ex)
                        {
                            CustomLogger.Error(
                                "Failed whilst saving reminder email",
                                new {user.UserId, Exception = ex.Message});
                            continue;
                        }
                    }
                }
            }
        }

        private void CheckUserAndSendReminderEmailsForSectorTypeAndReportingYear(User user, int year, ReminderEmail reminderEmail)
        {
            List<Organisation> inScopeActiveOrganisationsForUserAndSectorTypeThatStillNeedToReport = user.UserOrganisations
                .Where(uo => uo.HasBeenActivated())
                .Select(uo => uo.Organisation)
                .Where(o => o.Status == OrganisationStatuses.Active)
                .Where(o => o.SectorType == reminderEmail.SectorType)
                .Where(
                    o => o.OrganisationScopes.Any(
                        s => s.Status == ScopeRowStatuses.Active
                             && s.SnapshotDate == reminderEmail.SectorType.GetAccountingStartDate(year)
                             && (s.ScopeStatus == ScopeStatuses.InScope || s.ScopeStatus == ScopeStatuses.PresumedInScope)))
                .Where(
                    o =>
                        !o.Returns.Any(
                            r =>
                                r.Status == ReturnStatuses.Submitted
                                && r.AccountingDate == reminderEmail.SectorType.GetAccountingStartDate(year)))
                .ToList();

            SendReminderEmailsForSectorTypeAndReportingYear(
                user,
                inScopeActiveOrganisationsForUserAndSectorTypeThatStillNeedToReport,
                year,
                reminderEmail);
        }

        private void SendReminderEmailsForSectorTypeAndReportingYear(
            User user,
            List<Organisation> inScopeOrganisationsForUserAndSectorTypeThatStillNeedToReport,
            int year,
            ReminderEmail reminderEmail)
        {
            try
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

                reminderEmail.Status = ReminderEmailStatus.Completed;

                MarkReminderEmailAsCompleted(
                    reminderEmail,
                    anyOrganisationsToEmailAbout);
            }
            catch (Exception ex)
            {
                CustomLogger.Error(
                    "Failed whilst sending or saving reminder email",
                    new
                    {
                        user.UserId,
                        SectorType = reminderEmail.SectorType,
                        OrganisationIds = inScopeOrganisationsForUserAndSectorTypeThatStillNeedToReport.Select(o => o.OrganisationId),
                        Exception = ex.Message
                    });
            }
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

        private static ReminderEmail AddNewReminderEmailRecord(User user, SectorTypes sectorType, DateTime reminderDate)
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

            var dataRepository = Global.ContainerIoC.Resolve<IDataRepository>();
            dataRepository.Insert(reminderEmailRecord);
            dataRepository.SaveChanges();

            return reminderEmailRecord;
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

        private static List<int> GetActiveReportingYears(SectorTypes sectorType)
        {
            return ReportingYearsHelper.GetReportingYears()
                .Where(year => GetDeadlineDateForReportingYear(sectorType, year) > VirtualDateTime.Now)
                .ToList();
        }

    }
}
