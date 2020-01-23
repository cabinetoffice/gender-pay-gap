using System;
using Autofac;
using GenderPayGap.Core.Classes;
using GenderPayGap.Core.Classes.Queues;
using GenderPayGap.Core.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

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

        public static void RegisterLogRecord(this ContainerBuilder builder, string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
            {
                throw new ArgumentNullException(nameof(fileName));
            }

            string applicationName = AppDomain.CurrentDomain.FriendlyName;

            builder.Register(ctx => new LogRecordLogger(ctx.Resolve<LogRecordQueue>(), applicationName, fileName))
                .Keyed<ILogRecordLogger>(fileName)
                .SingleInstance();
        }

        /// <summary>
        ///     Adds the LogEvent queue as logging provider to the application
        /// </summary>
        /// <param name="factory"></param>
        public static ILoggerFactory UseLogEventQueueLogger(this ILoggerFactory factory, IServiceProvider serviceProvider)
        {
            // Resolve filter options
            var filterOptions = serviceProvider.GetService<IOptions<LoggerFilterOptions>>();

            // Resolve the keyed queue from autofac
            var lifetimeScope = serviceProvider.GetService<ILifetimeScope>();
            var logEventQueue = lifetimeScope.Resolve<LogEventQueue>();

            // Create the logging provider
            factory.AddProvider(new LogEventLoggerProvider(logEventQueue, AppDomain.CurrentDomain.FriendlyName, filterOptions));

            return factory;
        }

        /// <summary>
        ///     Adds an azure queue logger to a LoggingBuilder
        /// </summary>
        /// <param name="builder"></param>
        public static ILoggingBuilder AddAzureQueueLogger(this ILoggingBuilder builder)
        {
            // Build the current service provider
            ServiceProvider serviceProvider = builder.Services.BuildServiceProvider();

            // Resolve filter options
            var filterOptions = serviceProvider.GetService<IOptions<LoggerFilterOptions>>();

            // Resolve the keyed queue from autofac
            var lifetimeScope = serviceProvider.GetService<IContainer>();
            var logEventQueue = lifetimeScope.Resolve<LogEventQueue>();

            // Register the logging provider
            return builder.AddProvider(new LogEventLoggerProvider(logEventQueue, AppDomain.CurrentDomain.FriendlyName, filterOptions));
        }

    }
}
