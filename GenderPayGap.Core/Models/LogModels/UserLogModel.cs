using System;
using GenderPayGap.Extensions;

namespace GenderPayGap.Core.Models
{

    [Serializable]
    public class UserLogModel
    {

        public UserLogModel() { }

        public UserLogModel(string userId,
            string emailAddress,
            UserAction action,
            string targetName,
            string targetOldValue,
            string targetNewValue,
            string actionBy)
        {
            UserId = userId;
            EmailAddress = emailAddress;
            Action = action.ToString();
            TargetName = targetName;
            TargetOldValue = targetOldValue;
            TargetNewValue = targetNewValue;
            ActionBy = actionBy;
        }

        public string UserId { get; set; }

        public string EmailAddress { get; set; }

        public DateTime Date { get; set; } = VirtualDateTime.Now;

        public string Action { get; set; }

        public string ActionBy { get; set; }

        public string TargetName { get; set; }

        public string TargetOldValue { get; set; }

        public string TargetNewValue { get; set; }

    }

}
