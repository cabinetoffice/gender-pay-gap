using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GenderPayGap.Core.Interfaces;
using GenderPayGap.Core.Models;
using GenderPayGap.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace GenderPayGap.Core.Classes
{
    [ProviderAlias("QueuedLog")]
    public class LogEventLoggerProvider : ILoggerProvider
    {

        internal readonly string ApplicationName;
        private readonly IList<LoggerFilterRule> FilterRules;
        private readonly LogLevel MinLevel = LogLevel.Information;

        private readonly IQueue queue;
        public string Alias = typeof(LogEventLoggerProvider).GetAttributeValue((ProviderAliasAttribute attrib) => attrib.Alias);

        public LogEventLoggerProvider(IQueue queue, string applicationName, IOptions<LoggerFilterOptions> filterOptions)
        {
            if (string.IsNullOrWhiteSpace(applicationName))
            {
                throw new ArgumentNullException(nameof(applicationName));
            }

            ApplicationName = applicationName;
            this.queue = queue ?? throw new ArgumentNullException(nameof(queue));

            if (filterOptions != null)
            {
                MinLevel = filterOptions.Value.MinLevel;
                FilterRules = filterOptions.Value.Rules.Where(r => r.ProviderName.EqualsI(Alias)).ToList();
                if (FilterRules == null || FilterRules.Count == 0)
                {
                    FilterRules = filterOptions.Value.Rules
                        .Where(r => string.IsNullOrWhiteSpace(r.ProviderName) || r.ProviderName.EqualsI("default"))
                        .ToList();
                }
            }
        }

        public ILogger CreateLogger(string category)
        {
            LoggerFilterRule matchedCategory = FilterRules.FirstOrDefault(x => category.StartsWithI(x.CategoryName));
            if (matchedCategory == null)
            {
                matchedCategory =
                    FilterRules.FirstOrDefault(x => string.IsNullOrWhiteSpace(x.CategoryName) || x.CategoryName.EqualsI("default"));
            }

            LogLevel minLevel = MinLevel;
            Func<string, string, LogLevel, bool> filter = null;
            if (matchedCategory != null)
            {
                if (matchedCategory.LogLevel.HasValue)
                {
                    minLevel = matchedCategory.LogLevel.Value;
                }

                filter = matchedCategory.Filter;
            }

            return new LogEventLogger(this, category, minLevel, filter);
        }

        public void Dispose() { }

        public async Task WriteAsync(LogLevel logLevel, LogEntryModel logEntry)
        {
            var wrapper = new LogEventWrapperModel {ApplicationName = ApplicationName, LogLevel = logLevel, LogEntry = logEntry};

            await queue.AddMessageAsync(wrapper);
        }

    }
}
