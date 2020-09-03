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
            var runId = JobHelpers.CreateRunId();
            var startTime = VirtualDateTime.Now;
            JobHelpers.LogFunctionStart(runId,  nameof(SendReminderEmails), startTime);
            
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

            SendReminderEmailsForSectorType(OrganisationSectors.Private, runId, startTime);
            SendReminderEmailsForSectorType(OrganisationSectors.Public, runId, startTime);

            JobHelpers.LogFunctionEnd(runId, nameof(SendReminderEmails), startTime);
        }

        private void SendReminderEmailsForSectorType(OrganisationSectors sector, string runId, DateTime startTime)
        {
            if (IsAfterEarliestReminder(sector))
            {
                DateTime latestReminderEmailDate = GetLatestReminderEmailDate(sector);

                IEnumerable<User> usersUncheckedSinceLatestReminderDate = dataRepository.GetAll<User>()
                    .Where(user => !user.ReminderEmails
                            .Where(re => re.OrganisationSector == sector)
                            .Where(re => re.DateChecked > latestReminderEmailDate)
                            .Any());

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

        private void CheckUserAndSendReminderEmailsForSectorType(User user, OrganisationSectors sector)
        {
            List<Organisation> inScopeActiveOrganisationsForUserAndSectorTypeThatStillNeedToReport = user.UserOrganisations
                .Where(uo => uo.PINConfirmedDate != null)
                .Select(uo => uo.Organisation)
                .Where(o => o.Status == OrganisationStatuses.Active)
                .Where(o => o.Sector == sector)
                .Where(o => o.OrganisationScopes.Any(
                    s => s.Status == ScopeRowStatuses.Active &&
                         s.SnapshotDate == sector.GetAccountingStartDate() &&
                         (s.ScopeStatus == ScopeStatuses.InScope || s.ScopeStatus == ScopeStatuses.PresumedInScope)))
                .Where(
                    o =>
                        !o.Returns.Any(r =>
                            r.Status == ReturnStatuses.Submitted &&
                            r.AccountingDate == sector.GetAccountingStartDate()))
                .ToList();

            SendReminderEmailsForSectorType(user, inScopeActiveOrganisationsForUserAndSectorTypeThatStillNeedToReport, sector);
        }

        private void SendReminderEmailsForSectorType(
            User user,
            List<Organisation> inScopeOrganisationsForUserAndSectorTypeThatStillNeedToReport,
            OrganisationSectors organisationSector)
        {
            try
            {
                bool anyOrganisationsToEmailAbout = inScopeOrganisationsForUserAndSectorTypeThatStillNeedToReport.Count > 0;
                if (anyOrganisationsToEmailAbout)
                {
                    SendReminderEmail(user, organisationSector, inScopeOrganisationsForUserAndSectorTypeThatStillNeedToReport);
                }
                SaveReminderEmailRecord(user, organisationSector, anyOrganisationsToEmailAbout);
            }
            catch (Exception ex)
            {
                CustomLogger.Error(
                    "Failed whilst sending or saving reminder email",
                    new
                    {
                        user.UserId,
                        SectorType = organisationSector,
                        OrganisationIds = inScopeOrganisationsForUserAndSectorTypeThatStillNeedToReport.Select(o => o.OrganisationId),
                        Exception = ex.Message
                    });
            }
        }

        private void SendReminderEmail(User user,
            OrganisationSectors organisationSector,
            List<Organisation> organisations)
        {
            emailSendingService.SendReminderEmail(
                emailAddress: user.EmailAddress,
                deadlineDate: organisationSector.GetAccountingStartDate().AddYears(1).AddDays(-1).ToString("d MMMM yyyy"),
                daysUntilDeadline: organisationSector.GetAccountingStartDate().AddYears(1).Subtract(VirtualDateTime.Now).Days,
                organisationNames: GetOrganisationNameString(organisations),
                organisationIsSingular: organisations.Count == 1,
                organisationIsPlural: organisations.Count > 1,
                sectorType: organisationSector.ToString().ToLower());
        }

        private static void SaveReminderEmailRecord(User user, OrganisationSectors organisationSector, bool wasAnEmailSent)
        {
            var reminderEmailRecord = new ReminderEmail
            {
                UserId = user.UserId,
                OrganisationSector = organisationSector,
                DateChecked = VirtualDateTime.Now,
                EmailSent = wasAnEmailSent
            };

            var dataRepository = Global.ContainerIoC.Resolve<IDataRepository>();
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

        private bool IsAfterEarliestReminder(OrganisationSectors organisationSector)
        {
            return VirtualDateTime.Now > GetEarliestReminderDate(organisationSector);
        }

        private static DateTime GetEarliestReminderDate(OrganisationSectors organisationSector)
        {
            List<int> reminderEmailDays = GetReminderEmailDays();
            int earliestReminderDay = reminderEmailDays[reminderEmailDays.Count - 1];

            DateTime deadlineDate = GetDeadlineDate(organisationSector);
            return deadlineDate.AddDays(-earliestReminderDay);
        }

        private static DateTime GetLatestReminderEmailDate(OrganisationSectors organisationSector)
        {
            return GetReminderDates(organisationSector)
                .Where(reminderDate => reminderDate < VirtualDateTime.Now)
                .OrderByDescending(reminderDate => reminderDate)
                .FirstOrDefault();
        }

        private static List<DateTime> GetReminderDates(OrganisationSectors organisationSector)
        {
            List<int> reminderDays = GetReminderEmailDays();
            DateTime deadlineDate = GetDeadlineDate(organisationSector);

            return reminderDays.Select(reminderDay => deadlineDate.AddDays(-reminderDay)).ToList();
        }

        private static DateTime GetDeadlineDate(OrganisationSectors organisationSector)
        {
            return organisationSector.GetAccountingStartDate().AddYears(1);
        }

        private static List<int> GetReminderEmailDays()
        {
            var reminderEmailDays = JsonConvert.DeserializeObject<List<int>>(Global.ReminderEmailDays);
            reminderEmailDays.Sort();
            return reminderEmailDays;
        }

    }
}
