using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using GenderPayGap.Core.Classes;
using GenderPayGap.Core.Interfaces;
using GenderPayGap.Extensions.AspNetCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;
using Newtonsoft.Json;
using Notify.Authentication;
using NUnit.Framework;

namespace GenderPayGap.Core.Tests.Email.GovNotifyEmailProvider
{

    [TestFixture]
    [SetCulture("en-GB")]
    public class SendEmailAsyncTests
    {

        [SetUp]
        public void BeforeEach()
        {
            mockHttpClientFactory = new Mock<IHttpClientFactory>();
            mockHttpMessageHandler = new Mock<HttpMessageHandler>();

            mockEmailTemplateRepo = new Mock<IEmailTemplateRepository>();
            mockGovNotifyOptions = new Mock<IOptions<GovNotifyOptions>>();
            mockLogger = new Mock<ILogger<Core.Classes.GovNotifyEmailProvider>>();

            mockHttpClientFactory.Setup(_ => _.CreateClient(It.IsAny<string>()))
                .Returns(new HttpClient(mockHttpMessageHandler.Object));

            mockGovNotifyOptions.Setup(x => x.Value)
                .Returns(
                    new GovNotifyOptions {
                        ApiKey = $"prod-{ProductionServiceId}-{ProductionApiKey}",
                        ApiTestKey = $"test-{TestServiceId}-{TestApiKey}",
                        ClientReference = "GovNotifyClientReference",
                        Enabled = true
                    });

            // service under test
            testNotifyEmailProvider = new Core.Classes.GovNotifyEmailProvider(
                mockHttpClientFactory.Object,
                mockEmailTemplateRepo.Object,
                mockGovNotifyOptions.Object,
                mockLogger.Object);
        }

        [TearDown]
        public void AfterEach()
        {
            Config.EnvironmentName = "Local";
        }

        // notify keys broken down for testing
        private const string ProductionServiceId = "11111111-1111-1111-1111-111111111111";
        private const string ProductionApiKey = "22222222-2222-2222-2222-222222222222";

        private const string TestServiceId = "33333333-3333-3333-3333-333333333333";
        private const string TestApiKey = "44444444-4444-4444-4444-444444444444";

        private Mock<IEmailTemplateRepository> mockEmailTemplateRepo;
        private Mock<IOptions<GovNotifyOptions>> mockGovNotifyOptions;
        private Mock<ILogger<Core.Classes.GovNotifyEmailProvider>> mockLogger;

        private Mock<IHttpClientFactory> mockHttpClientFactory;
        private Mock<HttpMessageHandler> mockHttpMessageHandler;

        private Core.Classes.GovNotifyEmailProvider testNotifyEmailProvider;

        [TestCase(false, ProductionApiKey, ProductionServiceId)]
        [TestCase(true, TestApiKey, TestServiceId)]
        public async Task UsesExpectedApiCredentials(bool test, string apiKey, string expectedServiceId)
        {
            var sendAsyncCalled = false;

            mockHttpMessageHandler
                .Protected()
                .As<HttpMessageInvoker>()
                .Setup(x => x.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(
                    (HttpRequestMessage request, CancellationToken token) => {
                        sendAsyncCalled = true;

                        string actualToken = request.Headers.Authorization.Parameter.Replace("Bearer ", "");
                        IDictionary<string, object> decodedToken = Authenticator.DecodeToken(actualToken, apiKey);
                        object actualServiceId = decodedToken["iss"];
                        Assert.AreEqual(expectedServiceId, actualServiceId);

                        return new HttpResponseMessage {StatusCode = HttpStatusCode.OK, Content = new StringContent("{id: 123}")};
                    });

            SendEmailResult actual = await testNotifyEmailProvider.SendEmailAsync("test@test.com", "123", new {title = "TITLE"}, test);

            Assert.IsTrue(sendAsyncCalled);
        }

        [TestCase("Local", "[Local] ")]
        [TestCase("DEV", "[DEV] ")]
        [TestCase("TEST", "[TEST] ")]
        [TestCase("PROD", "")]
        public async Task SetsEnvironmentParameter(string testEnvironment, string expectedPersonalisation)
        {
            var sendAsyncCalled = false;

            // skips key vault config using whitespace
            Environment.SetEnvironmentVariable("Vault", " ");

            // set the environment to test
            Config.EnvironmentName = testEnvironment;

            mockHttpMessageHandler
                .Protected()
                .As<HttpMessageInvoker>()
                .Setup(x => x.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(
                    (HttpRequestMessage request, CancellationToken token) => {
                        if (request.Content != null)
                        {
                            sendAsyncCalled = true;
                            string content = request.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                            dynamic parsed = JsonConvert.DeserializeObject(content);
                            Assert.AreEqual(expectedPersonalisation, parsed.personalisation.Environment.Value);
                        }

                        return new HttpResponseMessage {StatusCode = HttpStatusCode.OK, Content = new StringContent("{id: 123}")};
                    });

            SendEmailResult actual = await testNotifyEmailProvider.SendEmailAsync("test@test.com", "123", new { }, true);

            Assert.IsTrue(sendAsyncCalled);
        }

        [TestCase]
        public async Task SendsExpectedEmailParameters()
        {
            var sendAsyncCalled = false;

            var expectedEmail = "test@test.com";
            var expectedTemplateId = "123";
            var expectedParameters = new {field1 = 123, field2 = "test"};

            mockHttpMessageHandler
                .Protected()
                .As<HttpMessageInvoker>()
                .Setup(x => x.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(
                    (HttpRequestMessage request, CancellationToken token) => {
                        if (request.Content != null)
                        {
                            sendAsyncCalled = true;
                            string content = request.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                            dynamic parsed = JsonConvert.DeserializeObject(content);

                            // assert email, template and reference
                            Assert.AreEqual(expectedEmail, parsed.email_address.Value);
                            Assert.AreEqual(expectedTemplateId, parsed.template_id.Value);
                            Assert.AreEqual(testNotifyEmailProvider.Options.Value.ClientReference, parsed.reference.Value);

                            // assert parameters
                            Assert.AreEqual(expectedParameters.field1, parsed.personalisation.field1.Value);
                            Assert.AreEqual(expectedParameters.field2, parsed.personalisation.field2.Value);
                        }

                        return new HttpResponseMessage {StatusCode = HttpStatusCode.OK, Content = new StringContent("{id: 123}")};
                    });

            SendEmailResult actual = await testNotifyEmailProvider.SendEmailAsync(
                expectedEmail,
                expectedTemplateId,
                expectedParameters,
                true);

            Assert.IsTrue(sendAsyncCalled);
        }

    }

}
