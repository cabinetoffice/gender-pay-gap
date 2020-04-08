using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using GenderPayGap.Core.Abstractions;
using GenderPayGap.Core.Interfaces;
using GenderPayGap.Extensions;
using GenderPayGap.Extensions.AspNetCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Notify.Client;
using Notify.Interfaces;
using Notify.Models;
using Notify.Models.Responses;

namespace GenderPayGap.Core.Classes
{

    public class GovNotifyEmailProvider : AEmailProvider
    {

        public GovNotifyEmailProvider(
            IHttpClientFactory httpClientFactory,
            IOptions<GovNotifyOptions> govNotifyOptions,
            ILogger<GovNotifyEmailProvider> logger)
            : base(logger)
        {
            Options = govNotifyOptions
                      ?? throw new ArgumentNullException("You must provide the gov notify email options", nameof(govNotifyOptions));

            if (Enabled)
            {
                // ensure we have api keys
                if (string.IsNullOrWhiteSpace(Options.Value.ClientReference))
                {
                    throw new NullReferenceException(
                        $"{nameof(Options.Value.ClientReference)}: You must supply a client reference for GovNotify");
                }

                if (string.IsNullOrWhiteSpace(Options.Value.ApiKey))
                {
                    throw new NullReferenceException($"{nameof(Options.Value.ApiKey)}: You must supply a production api key for GovNotify");
                }

                if (string.IsNullOrWhiteSpace(Options.Value.ApiTestKey))
                {
                    throw new NullReferenceException($"{nameof(Options.Value.ApiTestKey)}: You must supply a test api key for GovNotify");
                }
            }

            // create the clients
            HttpClient httpClient = httpClientFactory.CreateClient(nameof(GovNotifyEmailProvider));
            var notifyHttpWrapper = new HttpClientWrapper(httpClient);
            ProductionClient = new NotificationClient(notifyHttpWrapper, Options.Value.ApiKey);
            TestClient = new NotificationClient(notifyHttpWrapper, Options.Value.ApiTestKey);
        }

        public override bool Enabled => Options.Value.Enabled != false;

        public override async Task<SendEmailResult> SendEmailAsync<TTemplate>(string emailAddress,
            string templateId,
            TTemplate parameters,
            bool test)
        {
            // convert the model's public properties to a dictionary
            Dictionary<string, object> mergeParameters = parameters.GetPropertiesDictionary();

            // prefix subject with environment name
            mergeParameters["Environment"] = Config.IsProduction() ? "" : $"[{Config.EnvironmentName}] ";

            // determine which client to use
            IAsyncNotificationClient client = test ? TestClient : ProductionClient;

            // send email
            EmailNotificationResponse response = await client.SendEmailAsync(
                emailAddress,
                templateId,
                mergeParameters,
                Options.Value.ClientReference);

            // get result
            Notification notification = await client.GetNotificationByIdAsync(response.id);

            return new SendEmailResult {
                Status = notification.status,
                Server = "Gov Notify",
                ServerUsername = Options.Value.ClientReference,
                EmailAddress = emailAddress,
                EmailSubject = notification.subject
            };
        }

        #region Dependencies

        public IAsyncNotificationClient ProductionClient { get; }

        public IAsyncNotificationClient TestClient { get; }

        public IOptions<GovNotifyOptions> Options { get; }

        #endregion

    }

}
