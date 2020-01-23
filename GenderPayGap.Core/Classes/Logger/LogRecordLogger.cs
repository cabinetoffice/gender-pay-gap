using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GenderPayGap.Core.Interfaces;
using GenderPayGap.Core.Models;

namespace GenderPayGap.Core.Classes
{
    public interface ILogRecordLogger
    {

        Task WriteAsync(IEnumerable<object> records);
        Task WriteAsync(object record);

    }

    public class LogRecordLogger : ILogRecordLogger
    {

        private readonly IQueue queue;
        private readonly string fileName;

        public LogRecordLogger(IQueue queue, string applicationName, string fileName)
        {
            if (string.IsNullOrWhiteSpace(applicationName))
            {
                throw new ArgumentNullException(nameof(applicationName));
            }

            if (string.IsNullOrWhiteSpace(fileName))
            {
                throw new ArgumentNullException(nameof(fileName));
            }

            this.queue = queue ?? throw new ArgumentNullException(nameof(queue));
            ApplicationName = applicationName;
            this.fileName = fileName;
        }

        public string ApplicationName { get; }

        public async Task WriteAsync(IEnumerable<object> records)
        {
            foreach (object record in records)
            {
                await WriteAsync(record);
            }
        }

        public async Task WriteAsync(object record)
        {
            var wrapper = new LogRecordWrapperModel {ApplicationName = ApplicationName, FileName = fileName, Record = record};

            await queue.AddMessageAsync(wrapper);
        }

    }
}
