using System;
using Microsoft.Extensions.Logging;

namespace GenderPayGap.Core.Models
{
    [Serializable]
    public class LogEventWrapperModel
    {

        public string ApplicationName { get; set; }
        public LogLevel LogLevel { get; set; }
        public LogEntryModel LogEntry { get; set; }

    }
}
