using System;
using System.Text;
using System.Threading.Tasks;
using GenderPayGap.Core.Interfaces;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using Newtonsoft.Json;

namespace GenderPayGap.Core.Classes.Queues
{

    public class AzureQueue : IQueue
    {

        public virtual async Task AddMessageAsync<TInstance>(TInstance instance)
        {
            if (instance == null || Equals(instance, default(TInstance)))
            {
                throw new ArgumentNullException(nameof(instance));
            }

            string json = JsonConvert.SerializeObject(instance);

            await AddMessageAsync(json);
        }

        public virtual async Task AddMessageAsync(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                throw new ArgumentNullException(nameof(message));
            }

            // Check if its a large message and that this queue supports it
            if (fileRepository == null && message.Length > CloudQueueMessage.MaxMessageSize)
            {
                throw new ArgumentException(
                    $"{nameof(AzureQueue)}: Queue message exceeds maximum message size for azure queues. The message size was {message.Length}. Azure's maximum message size is {CloudQueueMessage.MaxMessageSize}.",
                    nameof(message));
            }

            // Write the message to storage if it's too large
            if (fileRepository != null && message.Length > CloudQueueMessage.MaxMessageSize)
            {
                // Create a "message ID"
                string filePath = $"LargeQueueFiles\\{Name}\\{Guid.NewGuid()}.json";
                byte[] bytes = Encoding.UTF8.GetBytes(message);
                await fileRepository.WriteAsync(filePath, bytes);
                message = $"file:{filePath}";
            }

            // Create the azure message
            var queueMessage = new CloudQueueMessage(message);

            // Get the queue via lazy loading
            CloudQueue queue = await lazyQueue.Value;

            // Write the message to the azure queue
            await queue.AddMessageAsync(queueMessage);
        }

        private async Task<CloudQueue> ConnectToAzureQueueLazyAsync()
        {
            // Parse the connection string and return a reference to the storage account.
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(connectionString);

            // Create a CloudFileClient object for credentialed access to File storage.
            CloudQueueClient queueClient = storageAccount.CreateCloudQueueClient();

            CloudQueue queue = queueClient.GetQueueReference(Name);

            // Create the queue if it doesn't already exist
            await queue.CreateIfNotExistsAsync().ConfigureAwait(false);

            return queue;
        }

        #region Constructors

        public AzureQueue(string connectionString, string queueName)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new ArgumentNullException(nameof(connectionString));
            }

            if (string.IsNullOrWhiteSpace(queueName))
            {
                throw new ArgumentNullException(nameof(queueName));
            }

            this.connectionString = connectionString;
            Name = queueName;
            lazyQueue = new Lazy<Task<CloudQueue>>(async () => await ConnectToAzureQueueLazyAsync());
        }

        /// <summary>
        ///     Enables large message support when providing a FileRepository.
        /// </summary>
        public AzureQueue(string connectionString, string queueName, IFileRepository fileRepository) : this(connectionString, queueName)
        {
            this.fileRepository = fileRepository ?? throw new ArgumentNullException(nameof(fileRepository));
        }

        #endregion

        #region Dependencies

        private readonly Lazy<Task<CloudQueue>> lazyQueue;
        private readonly string connectionString;
        private readonly IFileRepository fileRepository;
        public string Name { get; }

        #endregion

    }

}
