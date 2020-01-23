using System;
using GenderPayGap.Extensions;

namespace GenderPayGap.Core.Models
{
    [Serializable]
    public class ManualChangeLogModel
    {

        public ManualChangeLogModel() { }

        public ManualChangeLogModel(string methodName,
            ManualActions action,
            string source,
            string referenceName,
            string referenceValue,
            string targetName,
            string targetOldValue,
            string targetNewValue,
            string comment = null,
            string details = null)
        {
            MethodName = methodName;
            Action = action;
            Source = source;
            ReferenceName = referenceName;
            ReferenceValue = referenceValue;
            TargetName = targetName;
            TargetOldValue = targetOldValue;
            TargetNewValue = targetNewValue;
            Details = details;
            Comment = comment;
        }

        public DateTime Date { get; set; } = VirtualDateTime.Now;
        public string MethodName { get; set; }
        public ManualActions Action { get; set; }
        public string ReferenceName { get; set; }
        public string ReferenceValue { get; set; }
        public string TargetName { get; set; }
        public string TargetOldValue { get; set; }
        public string TargetNewValue { get; set; }
        public string Details { get; set; }
        public string Comment { get; set; }
        public string Source { get; set; }

    }
}
