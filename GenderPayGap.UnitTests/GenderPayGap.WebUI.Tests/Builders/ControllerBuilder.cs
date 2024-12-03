using System.Security.Claims;
using GenderPayGap.Core.Classes;
using GenderPayGap.Core.Interfaces;
using GenderPayGap.Database;
using GenderPayGap.WebUI.BackgroundJobs;
using GenderPayGap.WebUI.Cookies;
using GenderPayGap.WebUI.ExternalServices;
using GenderPayGap.WebUI.Repositories;
using GenderPayGap.WebUI.Services;
using GenderPayGap.WebUI.Tests.TestHelpers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Newtonsoft.Json;

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

        public ControllerBuilder<T> WithDatabaseObjects<TDbObj>(params TDbObj[] databaseObjects) where TDbObj: class
        {
            foreach (TDbObj databaseObject in databaseObjects)
            {
                DataRepository.Insert<TDbObj>(databaseObject);
            }

            return this;
        }

        public ControllerBuilder<T> WithMockUriHelper()
        {
            this.mockUriHelper = true;
            return this;
        }

        public T Build()
        {
            DataRepository.SaveChanges();
            
            IServiceProvider dependencyInjectionServiceProvider = BuildContainerIocForControllerOfType();
            Program.DependencyInjectionServiceProvider = dependencyInjectionServiceProvider;

            var httpContextMock = new Mock<HttpContext>();

            CreateMockPrincipal(httpContextMock);
            CreateMockHttpRequestWithOptionalFormValues(httpContextMock);
            CreateMockHttpResponse(httpContextMock);

            //Mock the httpcontext to the controllercontext
            var routeData = new RouteData();
            var controllerContextMock = new ControllerContext { HttpContext = httpContextMock.Object, RouteData = routeData };

            //Create and return the controller
            var controller = dependencyInjectionServiceProvider.GetService<T>();
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

        private IServiceProvider BuildContainerIocForControllerOfType()
        {
            WebApplicationBuilder builder = WebApplication.CreateBuilder();

            // Register the Controller itself
            builder.Services.AddScoped<T>();

            // Register dependencies
            // - an in-memory version of the database
            builder.Services.AddScoped<IDataRepository>(c => DataRepository);
            // - the email sending API (IBackgroundJobsApi)
            SetupEmailSendingApi(builder);

            builder.Services.AddScoped<EmailSendingService>();
            builder.Services.AddScoped<IGovNotifyAPI>(g => new MockGovNotify());

            builder.Services.AddScoped<UserRepository>();
            builder.Services.AddScoped<RegistrationRepository>();
            builder.Services.AddScoped<AuditLogger>();
            builder.Services.AddScoped<PinInThePostService>();

            // Misc other things we seem to need
            builder.Services.AddScoped<IHttpContextAccessor>(c => Mock.Of<IHttpContextAccessor>());

            // Things we might want in future
            //builder.Register(c => new SystemFileRepository()).As<IFileRepository>().InstancePerLifetimeScope();
            //builder.RegisterType<OrganisationService>().As<OrganisationService>().InstancePerLifetimeScope();
            //builder.RegisterType<ReturnService>().As<ReturnService>().InstancePerLifetimeScope();
            //builder.RegisterType<DraftReturnService>().As<DraftReturnService>().InstancePerLifetimeScope();

            //builder.RegisterType<UpdateFromCompaniesHouseService>().As<UpdateFromCompaniesHouseService>().InstancePerLifetimeScope();
            //builder.RegisterType<AutoCompleteSearchService>().As<AutoCompleteSearchService>().InstancePerLifetimeScope();
            //builder.RegisterType<ViewingSearchService>().As<ViewingSearchService>().InstancePerLifetimeScope();

            return builder.Services.BuildServiceProvider();
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

        private void SetupEmailSendingApi(WebApplicationBuilder builder)
        {
            var mockBackgroundJobsApi = new Mock<IBackgroundJobsApi>();
            mockBackgroundJobsApi
                .Setup(q => q.AddEmailToQueue(It.IsAny<NotifyEmail>()))
                .Callback<NotifyEmail>(notifyEmail => EmailsSent.Add(notifyEmail));
            builder.Services.AddScoped<IBackgroundJobsApi>(c => mockBackgroundJobsApi.Object);
        }

    }
}
