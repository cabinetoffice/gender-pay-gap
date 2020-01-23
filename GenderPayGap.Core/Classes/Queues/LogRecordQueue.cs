using GenderPayGap.Core.Interfaces;

namespace GenderPayGap.Core.Classes.Queues
{

    public class LogRecordQueue : AzureQueue
    {

        public LogRecordQueue(string connectionString, IFileRepository fileRepo)
            : base(connectionString, QueueNames.LogRecord, fileRepo) { }

    }

}
