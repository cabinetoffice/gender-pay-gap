using GenderPayGap.Core;
using GenderPayGap.Database;
using GenderPayGap.Database.Models;

namespace GenderPayGap.WebUI.Helpers
{
    public static class UserAnonymisationHelper
    {
        public static void AnonymiseAndRetireUser(User user, string statusMessage)
        {
            user.Firstname = $"User{user.UserId}";
            user.Lastname = $"User{user.UserId}";
            user.JobTitle = "Anonymised";
            user.EmailAddress = $"anonymised-{user.UserId}";
            user.ContactPhoneNumber = "Anonymised";
            user.Salt = "Anonymised";
            user.PasswordHash = "Anonymised";

            user.HasBeenAnonymised = true;

            user.SetStatus(UserStatuses.Retired, user, statusMessage);
        }

        public static void AnonymiseAuditLogsForUser(List<AuditLog> userAuditLogs)
        {
            var actionsToAnonymise = new List<AuditedAction>
            {
                AuditedAction.UserChangeEmailAddress,
                AuditedAction.UserChangeName,
                AuditedAction.UserChangeJobTitle,
                AuditedAction.UserChangePhoneNumber,
                AuditedAction.PurgeRegistration,
                AuditedAction.PurgeUser,
                AuditedAction.RegistrationLog
            };

            userAuditLogs  = userAuditLogs.Where(al => actionsToAnonymise.Contains(al.Action))
                .ToList();

            foreach (AuditLog auditLog in userAuditLogs)
            {
                auditLog.Details = new Dictionary<string, string> { { "Anonymised", "Anonymised" } };
            }
        }

    }
}
