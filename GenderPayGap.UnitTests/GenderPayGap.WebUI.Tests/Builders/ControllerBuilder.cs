using System;
using System.Collections.Generic;
using System.Reflection;
using System.Security.Claims;
using Autofac;
using GenderPayGap.Core.Classes;
using GenderPayGap.Core.Interfaces;
using GenderPayGap.Database;
using GenderPayGap.Extensions;
using GenderPayGap.Tests.Common.TestHelpers;
using GenderPayGap.WebUI.BackgroundJobs;
using GenderPayGap.WebUI.BusinessLogic.Abstractions;
using GenderPayGap.WebUI.Cookies;
using GenderPayGap.WebUI.ExternalServices;
using GenderPayGap.WebUI.Repositories;
using GenderPayGap.WebUI.Services;
using GenderPayGap.WebUI.Tests.TestHelpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;

namespace GenderPayGap.WebUI.Tests.Builders
{
    public class ControllerBuilder<T>
    where T : Controller
    {

        private User user;
        private bool mockUriHelper;

        public List<NotifyEmail> EmailsSent { get; } = new List<NotifyEmail>();
        public IDataRepository DataRepository { get; }


        public ControllerBuilder()
        {
            DataRepository = CreateInMemoryTestDatabase();
        }

        public ControllerBuilder<T> WithLoggedInUser(User user)
        {
            this.user = user;
            return this;
        }

        public ControllerBuilder<T> WithDatabaseObjects(params object[] databaseObjects)
        {
            foreach (object databaseObject in databaseObjects)
            {
                DataRepository.Insert(databaseObject);
            }
            DataRepository.SaveChanges();

            return this;
        }

        public ControllerBuilder<T> WithMockUriHelper()
        {
            this.mockUriHelper = true;
            return this;
        }

        public T Build()
        {
            IContainer DIContainer = BuildContainerIocForControllerOfType();
            Startup.ContainerIoC = DIContainer;

            var httpContextMock = new Mock<HttpContext>();

            CreateMockPrincipal(httpContextMock);
            CreateMockHttpRequestWithOptionalFormValues(httpContextMock);
            CreateMockHttpResponse(httpContextMock);

            //Mock the httpcontext to the controllercontext
            var routeData = new RouteData();
            var controllerContextMock = new ControllerContext { HttpContext = httpContextMock.Object, RouteData = routeData };

            //Create and return the controller
            var controller = DIContainer.Resolve<T>();
            controller.ControllerContext = controllerContextMock;

            if (mockUriHelper)
            {
                controller.AddMockUriHelperNew(new Uri("https://localhost:44371/mockURL").ToString());
            }
            return controller;
        }

        private void CreateMockPrincipal(Mock<HttpContext> httpContextMock)
        {
            bool weHaveALoggedInUser = (user != null);

            var claims = new List<Claim>();
            if (weHaveALoggedInUser)
            {
                claims.Add(new Claim("user_id", user.UserId.ToString()));
            }

            var mockPrincipal = new Mock<ClaimsPrincipal>();
            mockPrincipal.Setup(m => m.Claims).Returns(claims);
            mockPrincipal.Setup(m => m.Identity.IsAuthenticated).Returns(weHaveALoggedInUser);

            httpContextMock.Setup(ctx => ctx.User).Returns(mockPrincipal.Object);
        }

        private void CreateMockHttpRequestWithOptionalFormValues(
            Mock<HttpContext> httpContextMock)
        {
            var requestMock = new Mock<HttpRequest>();
            var requestHeaders = new HeaderDictionary();
            var requestCookies = new MockRequestCookieCollection(
                new Dictionary<string, string>
                {
                    {
                        "cookie_settings",
                        JsonConvert.SerializeObject(
                            new CookieSettings {GoogleAnalyticsGpg = true, GoogleAnalyticsGovUk = true})
                    }
                });
            requestMock.SetupGet(x => x.Headers).Returns(requestHeaders);
            requestMock.SetupGet(x => x.Cookies).Returns(requestCookies);

            httpContextMock.SetupGet(ctx => ctx.Request).Returns(requestMock.Object);
            httpContextMock.SetupGet(ctx => ctx.Request.Headers).Returns(requestHeaders);
            httpContextMock.SetupGet(ctx => ctx.Request.Cookies).Returns(requestCookies);

            httpContextMock.SetupGet(ctx => ctx.Request.HttpContext).Returns(httpContextMock.Object);
        }

        private static void CreateMockHttpResponse(Mock<HttpContext> httpContextMock)
        {
            var responseMock = new Mock<HttpResponse>();
            var responseHeaders = new HeaderDictionary();
            var responseCookies = new Mock<IResponseCookies>();

            responseCookies.Setup(c => c.Append(It.IsAny<string>(), It.IsAny<string>())).Verifiable();
            responseCookies.Setup(c => c.Append(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CookieOptions>())).Verifiable();
            responseCookies.Setup(c => c.Delete(It.IsAny<string>())).Verifiable();
            responseCookies.Setup(c => c.Delete(It.IsAny<string>(), It.IsAny<CookieOptions>())).Verifiable();
            responseMock.SetupGet(x => x.Headers).Returns(responseHeaders);
            responseMock.SetupGet(x => x.Cookies).Returns(responseCookies.Object);

            httpContextMock.SetupGet(ctx => ctx.Response).Returns(responseMock.Object);
            httpContextMock.SetupGet(ctx => ctx.Response.Headers).Returns(responseHeaders);
            httpContextMock.SetupGet(ctx => ctx.Response.Cookies).Returns(responseCookies.Object);

            httpContextMock.SetupGet(ctx => ctx.Response.HttpContext).Returns(httpContextMock.Object);
        }

        private IContainer BuildContainerIocForControllerOfType()
        {
            var builder = new ContainerBuilder();

            // Register the Controller itself
            builder.RegisterType<T>().InstancePerLifetimeScope();

            // Register dependencies
            // - an in-memory version of the database
            builder.Register(c => DataRepository).As<IDataRepository>().InstancePerLifetimeScope();
            // - the email sending API (IBackgroundJobsApi)
            SetupEmailSendingApi(builder);

            builder.RegisterType<EmailSendingService>().As<EmailSendingService>().InstancePerLifetimeScope();
            builder.Register(g => new MockGovNotify()).As<IGovNotifyAPI>().InstancePerLifetimeScope();

            builder.RegisterType<UserRepository>().As<IUserRepository>().InstancePerLifetimeScope();
            builder.RegisterType<RegistrationRepository>().As<RegistrationRepository>().InstancePerLifetimeScope();
            builder.RegisterType<AuditLogger>().As<AuditLogger>().InstancePerLifetimeScope();
            builder.RegisterType<PinInThePostService>().As<PinInThePostService>().InstancePerLifetimeScope();

            // Misc other things we seem to need
            builder.Register(c => Mock.Of<IHttpContextAccessor>()).As<IHttpContextAccessor>().InstancePerLifetimeScope();

            // Things we might want in future
            //builder.Register(c => new SystemFileRepository()).As<IFileRepository>().InstancePerLifetimeScope();
            //builder.RegisterType<OrganisationService>().As<OrganisationService>().InstancePerLifetimeScope();
            //builder.RegisterType<ReturnService>().As<ReturnService>().InstancePerLifetimeScope();
            //builder.RegisterType<DraftReturnService>().As<DraftReturnService>().InstancePerLifetimeScope();

            //builder.RegisterType<UpdateFromCompaniesHouseService>().As<UpdateFromCompaniesHouseService>().InstancePerLifetimeScope();
            //builder.RegisterType<AutoCompleteSearchService>().As<AutoCompleteSearchService>().InstancePerLifetimeScope();
            //builder.RegisterType<ViewingSearchService>().As<ViewingSearchService>().InstancePerLifetimeScope();

            IContainer container = builder.Build();
            return container;
        }

        private IDataRepository CreateInMemoryTestDatabase()
        {
            //Get the method name of the unit test or the parent
            string testName = TestContext.CurrentContext.Test.FullName;
            if (string.IsNullOrWhiteSpace(testName))
            {
                testName = UiTestHelper.GetCurrentTestName();
            }

            DbContextOptionsBuilder<GpgDatabaseContext> optionsBuilder =
                new DbContextOptionsBuilder<GpgDatabaseContext>().UseInMemoryDatabase(testName);

            optionsBuilder.ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning));

            // show more detailed EF errors i.e. ReturnId value instead of '{ReturnId}' in the logs etc...
            optionsBuilder.EnableSensitiveDataLogging();

            var dbContext = new GpgDatabaseContext(optionsBuilder.Options);
            var dataRepository = new SqlRepository(dbContext);
            return dataRepository;
        }

        private void SetupEmailSendingApi(ContainerBuilder builder)
        {
            var mockBackgroundJobsApi = new Mock<IBackgroundJobsApi>();
            mockBackgroundJobsApi
                .Setup(q => q.AddEmailToQueue(It.IsAny<NotifyEmail>()))
                .Callback<NotifyEmail>(notifyEmail => EmailsSent.Add(notifyEmail));
            builder.Register(c => mockBackgroundJobsApi.Object).As<IBackgroundJobsApi>().InstancePerLifetimeScope();
        }

    }
}
