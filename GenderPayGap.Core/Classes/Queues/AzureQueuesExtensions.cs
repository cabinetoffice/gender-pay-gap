using System;
using Autofac;
using GenderPayGap.Core.Classes.Queues;
using GenderPayGap.Core.Interfaces;

namespace GenderPayGap.Infrastructure.AzureQueues.Extensions
{
    public static class AzureQueuesExtensions
    {

        public static void RegisterAzureQueue(this ContainerBuilder builder,
            string azureConnectionString,
            string queueName,
            bool supportLargeMessages = true)
        {
            if (string.IsNullOrWhiteSpace(azureConnectionString))
            {
                throw new ArgumentNullException(nameof(azureConnectionString));
            }

            if (string.IsNullOrWhiteSpace(queueName))
            {
                throw new ArgumentNullException(nameof(queueName));
            }

            builder.Register(
                    ctx =>
                        supportLargeMessages
                            ? new AzureQueue(azureConnectionString, queueName, ctx.Resolve<IFileRepository>())
                            : new AzureQueue(azureConnectionString, queueName))
                .Keyed<IQueue>(queueName)
                .SingleInstance();
        }

    }
}
