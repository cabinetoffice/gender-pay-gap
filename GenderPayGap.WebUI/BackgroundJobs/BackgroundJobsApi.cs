using GenderPayGap.WebUI.BackgroundJobs.QueueBasedJobs;
using GenderPayGap.WebUI.BackgroundJobs.ScheduledJobs;
using GenderPayGap.WebUI.ExternalServices;
using Hangfire;

namespace GenderPayGap.WebUI.BackgroundJobs
{
    public interface IBackgroundJobsApi {

        void AddEmailToQueue(NotifyEmail notifyEmail);

    }

    public class BackgroundJobsApi : IBackgroundJobsApi
    {

        public static void InitialiseScheduledJobs()
        {
            // Very frequent jobs
            RecurringJob.AddOrUpdate<FetchCompaniesHouseDataJob>(
                ScheduledJobIds.FetchCompaniesHouseDataJobId,
                j => j.FetchCompaniesHouseData(),
                "*/5 * * * *" /* every 5 minutes */);

            RecurringJob.AddOrUpdate<BackupDatabaseToJsonFileJob>(
                ScheduledJobIds.BackupDatabaseToJsonFileJobId,
                j => j.RunBackup(),
                "*/15 * * * *" /* every 15 minutes */);

            // Hourly jobs
            RecurringJob.AddOrUpdate<UpdatePublicFacingDownloadFilesJob>(
                ScheduledJobIds.UpdatePublicFacingDownloadFilesJobId,
                j => j.UpdateDownloadFiles(),
                "0 * * * *" /* once per hour, at 0 minutes past the hour */);

            RecurringJob.AddOrUpdate<SendReminderEmailsJob>(
                ScheduledJobIds.SendReminderEmailsJobId,
                j => j.SendReminderEmails(),
                "25 * * * *" /* once per hour, at 25 minutes past the hour */);

            // Daily jobs
            RecurringJob.AddOrUpdate<PurgeUsersJob>(
                ScheduledJobIds.PurgeUsersJobId,
                j => j.PurgeUsers(),
                "40 3 * * *" /* 03:40 once per day */);

            RecurringJob.AddOrUpdate<PurgeRegistrationsJob>(
                ScheduledJobIds.PurgeRegistrationsJobId,
                j => j.PurgeRegistrations(),
                "50 3 * * *" /* 03:50 once per day */);

            RecurringJob.AddOrUpdate<PurgeOrganisationsJob>(
                ScheduledJobIds.PurgeOrganisationsJobId,
                j => j.PurgeOrganisations(),
                "20 4 * * *" /* 04:20 once per day */);

            RecurringJob.AddOrUpdate<SetPresumedScopesJob>(
                ScheduledJobIds.SetPresumedScopesJobId,
                j => j.SetPresumedScopes(),
                "50 4 * * *" /* 04:50 once per day */);

            RecurringJob.AddOrUpdate<AnonymiseThreeYearOldFeedbackJob>(
                ScheduledJobIds.AnonymiseThreeYearOldFeedbackJobId,
                j => j.AnonymiseFeedback(),
                "20 5 * * *" /* 05:20 once per day */);


            RecurringJob.AddOrUpdate<NotifyUsersAndRetireInactiveAccountsJob>(
                ScheduledJobIds.NotifyUsersAndRetireInactiveAccountsJobId,
                j => j.NotifyUsersAndRetireInactiveAccounts(),
                "50 5 * * *" /* 5:50 once per day */);

            RecurringJob.AddOrUpdate<AnonymiseRetiredDuplicateUsersJob>(
                ScheduledJobIds.AnonymiseRetiredDuplicateUsersJobId,
                j => j.AnonymiseRetiredDuplicateUsers(),
                Cron.Never);
        }

        public void AddEmailToQueue(NotifyEmail notifyEmail)
        {
            BackgroundJob.Enqueue<SendNotifyEmailJob>(
                j => j.SendNotifyEmail(notifyEmail));
        }
    }

    internal class ScheduledJobIds
    {
        public const string FetchCompaniesHouseDataJobId = "FETCH_COMPANIES_HOUSE_DATA_JOB";
        public const string BackupDatabaseToJsonFileJobId = "BACKUP_DATABASE_TO_JSON_FILE_JOB";
        public const string UpdatePublicFacingDownloadFilesJobId = "UPDATE_PUBLIC_FACING_DOWNLOAD_FILES_JOB";
        public const string SendReminderEmailsJobId = "SET_REMINDER_EMAILS_JOB";
        public const string PurgeUsersJobId = "PURGE_USERS_JOB";
        public const string PurgeRegistrationsJobId = "PURG_REGISTRATIONS_JOB";
        public const string PurgeOrganisationsJobId = "PURGE_ORGANISATIONS_JOB";
        public const string SetPresumedScopesJobId = "SET_PRESUMED_SCOPES_JOB";
        public const string AnonymiseThreeYearOldFeedbackJobId = "ANONYMISE_THREE_YEAR_OLD_FEEDBACK_JOB";
        public const string NotifyUsersAndRetireInactiveAccountsJobId = "NOTIFY_USERS_AND_RETIRE_INACTIVE_ACCOUNT_JOB";
        public const string AnonymiseRetiredDuplicateUsersJobId = "ANONYMISE_RETIRED_DUPLICATE_USERS_JOB";

    }
}
