using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Reflection;
using System.Security.Claims;
using Autofac;
using Autofac.Features.AttributeFilters;
using AutoMapper;
using GenderPayGap.BusinessLogic;
using GenderPayGap.BusinessLogic.Account.Abstractions;
using GenderPayGap.BusinessLogic.Account.Repositories;
using GenderPayGap.BusinessLogic.Repositories;
using GenderPayGap.BusinessLogic.Services;
using GenderPayGap.Core;
using GenderPayGap.Core.Classes;
using GenderPayGap.Core.Classes.Queues;
using GenderPayGap.Core.Interfaces;
using GenderPayGap.Core.Models;
using GenderPayGap.Core.Models.Cookies;
using GenderPayGap.Database;
using GenderPayGap.Extensions;
using GenderPayGap.Extensions.AspNetCore;
using GenderPayGap.Tests;
using GenderPayGap.Tests.Common.Classes;
using GenderPayGap.Tests.Common.Mocks;
using GenderPayGap.Tests.Common.TestHelpers;
using GenderPayGap.WebUI.Areas.Account.Abstractions;
using GenderPayGap.WebUI.Areas.Account.ViewServices;
using GenderPayGap.WebUI.Classes;
using GenderPayGap.WebUI.Classes.Presentation;
using GenderPayGap.WebUI.Classes.Services;
using GenderPayGap.WebUI.Controllers.Account;
using GenderPayGap.WebUI.Controllers.Admin;
using GenderPayGap.WebUI.Models.AccountCreation;
using GenderPayGap.WebUI.Options;
using GenderPayGap.WebUI.Services;
using GenderPayGap.WebUI.Tests.Mocks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;

namespace GenderPayGap.WebUI.Tests.TestHelpers
{
    public static class NewUiTestHelper
    {
        
        public static IContainer DIContainer;
        
        private const string Url = "https://localhost/";
        public static Uri Uri => new Uri(Url, UriKind.Absolute);

        public static T GetController<T>(long userId = 0,  Dictionary<string, StringValues> requestFormValues = null, params object[] dbObjects)
        where T: Controller
        {

            DIContainer = BuildContainerIoC(dbObjects);

            //Create Inversion of Control container
            MvcApplication.ContainerIoC = DIContainer;
            Program.MvcApplication = DIContainer.Resolve<IMvcApplication>();
            
            //Mock UserId as claim
            var claims = new List<Claim>();
            if (userId < 0)
            {
                User user = dbObjects.OfType<User>().FirstOrDefault();
                if (user == null)
                {
                    foreach (object item in dbObjects)
                    {
                        var enumerable = item as IEnumerable<object>;
                        if (enumerable != null)
                        {
                            user = enumerable.OfType<User>().FirstOrDefault();
                            if (user != null)
                            {
                                break;
                            }
                        }
                    }
                }

                userId = user.UserId;
            }


            if (userId != 0)
            {
                claims.Add(new Claim("sub", userId.ToString()));
            }

            var mockPrincipal = new Mock<ClaimsPrincipal>();
            mockPrincipal.Setup(m => m.Claims).Returns(claims);
            mockPrincipal.Setup(m => m.Identity.IsAuthenticated).Returns(userId > 0);
            if (userId > 0)
            {
                mockPrincipal.Setup(m => m.Identity.Name).Returns(userId.ToString());
            }
            
            
            
//            //Set the Remote IP Address
//            var features = new FeatureCollection();
//            features.Set<IHttpConnectionFeature>(new HttpConnectionFeature {RemoteIpAddress = IPAddress.Parse("127.0.0.1")});

            //Mock HttpRequest
            var requestMock = new Mock<HttpRequest>();

            var requestHeaders = new HeaderDictionary();
            //requestMock.Setup(x => x.GetUri()).Returns(Uri);
            var requestCookies = new RequestCookieCollection(
                new Dictionary<string, string> {
                    {
                        "cookie_settings",
                        JsonConvert.SerializeObject(
                            new CookieSettings {
                                GoogleAnalyticsGpg = true, GoogleAnalyticsGovUk = true, ApplicationInsights = true, RememberSettings = true
                            })
                    }
                });
            requestMock.SetupGet(x => x.Cookies).Returns(requestCookies);
            requestMock.SetupGet(x => x.Headers).Returns(requestHeaders);
            
            
            var requestForm = new FormCollection(requestFormValues);
            requestMock.Setup(req => req.Form).Returns(requestForm);


            //Done:Added the queryString Dictionary property for mock object
            var queryString = new QueryString("?code=abcdefg");
            requestMock.SetupGet(x => x.QueryString).Returns(queryString);

            var query = new QueryCollection();

            //Mock HttpResponse
            var responseMock = new Mock<HttpResponse>();
            var responseHeaders = new HeaderDictionary();
            var responseCookies = new Mock<IResponseCookies>();
            responseCookies.Setup(c => c.Append(It.IsAny<string>(), It.IsAny<string>())).Verifiable();
            responseCookies.Setup(c => c.Append(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CookieOptions>())).Verifiable();
            responseCookies.Setup(c => c.Delete(It.IsAny<string>())).Verifiable();
            responseCookies.Setup(c => c.Delete(It.IsAny<string>(), It.IsAny<CookieOptions>())).Verifiable();
            responseMock.SetupGet(x => x.Headers).Returns(responseHeaders);
            responseMock.SetupGet(x => x.Cookies).Returns(responseCookies.Object);
            
            
            //Mock session
            var uri = new UriBuilder(Uri);

            //Mock HttpContext
            var httpContextMock = new Mock<HttpContext>();
            //contextMock.Setup(m => m.Cache).Returns(HttpRuntime.Cache);
            httpContextMock.Setup(ctx => ctx.User).Returns(mockPrincipal.Object);
            httpContextMock.SetupGet(ctx => ctx.Request.Headers).Returns(requestHeaders);
            httpContextMock.SetupGet(ctx => ctx.Request).Returns(requestMock.Object);
            httpContextMock.SetupGet(ctx => ctx.Request.Cookies).Returns(requestCookies);
            httpContextMock.SetupGet(ctx => ctx.Request.QueryString).Returns(queryString);
            httpContextMock.SetupGet(ctx => ctx.Request.Query).Returns(query);
            httpContextMock.SetupGet(ctx => ctx.Request.HttpContext).Returns(httpContextMock.Object);
            
            httpContextMock.SetupGet(ctx => ctx.Request.Form).Returns(requestForm);
            
            
            //contextMock.SetupGet(ctx => ctx.Request.GetUri()).Returns(Uri);
            //contextMock.Setup(ctx => ctx.GetUserHostAddress()).Returns("127.0.0.1");
            httpContextMock.SetupGet(ctx => ctx.Request.Scheme).Returns(uri.Scheme);
            httpContextMock.SetupGet(ctx => ctx.Request.Host).Returns(new HostString(uri.Host, uri.Port));
            httpContextMock.SetupGet(ctx => ctx.Request.Path).Returns(uri.Path);
            httpContextMock.SetupGet(ctx => ctx.Request.QueryString).Returns(new QueryString(uri.Query));

            httpContextMock.SetupGet(ctx => ctx.Response).Returns(responseMock.Object);
            httpContextMock.SetupGet(ctx => ctx.Response.Cookies).Returns(responseCookies.Object);
            httpContextMock.SetupGet(ctx => ctx.Response.Headers).Returns(responseHeaders);
            httpContextMock.SetupGet(ctx => ctx.Response.HttpContext).Returns(httpContextMock.Object);
            httpContextMock.SetupGet(ctx => ctx.Session).Returns(new MockHttpSession());
//            httpContextMock.SetupGet(ctx => ctx.Features).Returns(features);

            //Mock the httpcontext to the controllercontext

            var routeData = new RouteData();
            //var controllerContextMock = new Mock<ControllerContext>();
            var controllerContextMock = new ControllerContext {HttpContext = httpContextMock.Object, RouteData = routeData};
            
            Global.FileRepository = DIContainer.Resolve<IFileRepository>();
            Global.SearchRepository = DIContainer.Resolve<ISearchRepository<EmployerSearchModel>>();
//            Global.SicCodeSearchRepository = DIContainer.Resolve<ISearchRepository<SicCodeSearchModel>>();

            //Mock IHttpContextAccessor
            Mock<IHttpContextAccessor> mockHttpContextAccessor = DIContainer.Resolve<IHttpContextAccessor>().GetMockFromObject();
            mockHttpContextAccessor.SetupGet(a => a.HttpContext).Returns(httpContextMock.Object);

            //Configure the global HttpContext using the mock accessor
            System.Web.HttpContext.Configure(mockHttpContextAccessor.Object);

            //Create and return the controller
            var controller = DIContainer.Resolve<T>();
            
            controller.ControllerContext = controllerContextMock;
            
            //Mock session
            
//            controller.AddMockUriHelperNew(new UriBuilder(Uri).ToString());

            return controller;
            
        }
        
        public static IContainer BuildContainerIoC(params object[] dbObjects)
        {
            var builder = new ContainerBuilder();

            //Create an in-memory version of the database
            if (dbObjects != null && dbObjects.Length > 0)
            {
                builder.RegisterInMemoryTestDatabase(dbObjects);
            }
            else
            {
                Mock<IDataRepository> mockDataRepo = MoqHelpers.CreateMockAsyncDataRepository();
                builder.Register(c => mockDataRepo.Object).As<IDataRepository>().InstancePerLifetimeScope();
            }
            
            //Create the mock repositories
            // BL Repository
            builder.Register(c => new SystemFileRepository()).As<IFileRepository>().InstancePerLifetimeScope();
            builder.Register(c => new MockSearchRepository()).As<ISearchRepository<EmployerSearchModel>>().InstancePerLifetimeScope();
            
            builder.Register(c => Mock.Of<ISearchRepository<SicCodeSearchModel>>()).As<ISearchRepository<SicCodeSearchModel>>();
            builder.RegisterType<UserRepository>().As<IUserRepository>().SingleInstance();
            builder.RegisterType<RegistrationRepository>().As<IRegistrationRepository>().SingleInstance();

//             BL Services
            builder.RegisterInstance(Config.Configuration);
            builder.RegisterType<CommonBusinessLogic>().As<ICommonBusinessLogic>().InstancePerLifetimeScope();
            builder.RegisterType<SearchBusinessLogic>().As<ISearchBusinessLogic>().InstancePerLifetimeScope();
            builder.RegisterType<UpdateFromCompaniesHouseService>().As<UpdateFromCompaniesHouseService>().InstancePerLifetimeScope();
            builder.RegisterType<SecurityCodeBusinessLogic>().As<ISecurityCodeBusinessLogic>().InstancePerLifetimeScope();
            builder.RegisterType<DraftFileBusinessLogic>().As<IDraftFileBusinessLogic>().InstancePerLifetimeScope();
            builder.RegisterType<DownloadableFileBusinessLogic>().As<IDownloadableFileBusinessLogic>().InstancePerLifetimeScope();

            builder.Register(
                    c => c.ResolveAsMock<ScopeBusinessLogic>(
                            false,
                            typeof(ICommonBusinessLogic),
                            typeof(IDataRepository),
                            typeof(ISearchBusinessLogic))
                        .Object)
                .As<IScopeBusinessLogic>()
                .InstancePerLifetimeScope();

            builder.Register(
                    c => c.ResolveAsMock<SubmissionBusinessLogic>(false, typeof(ICommonBusinessLogic), typeof(IDataRepository)).Object)
                .As<ISubmissionBusinessLogic>()
                .InstancePerLifetimeScope();
            builder.RegisterType<OrganisationBusinessLogic>().As<IOrganisationBusinessLogic>().InstancePerLifetimeScope();

            
            builder.RegisterType<MockPrivateEmployerRepository>()
                .As<IPagedRepository<EmployerRecord>>()
                .Keyed<IPagedRepository<EmployerRecord>>("Private")
                .SingleInstance();
            builder.RegisterType<MockPublicEmployerRepository>()
                .As<IPagedRepository<EmployerRecord>>()
                .Keyed<IPagedRepository<EmployerRecord>>("Public")
                .SingleInstance();
            builder.Register(g => new MockGovNotify()).As<IGovNotifyAPI>().SingleInstance();
            builder.Register(g => new MockClassQueue(QueueNames.SendNotifyEmail)).Keyed<IQueue>(QueueNames.SendNotifyEmail).SingleInstance();
            builder.Register(g => new MockClassQueue(QueueNames.ExecuteWebJob)).Keyed<IQueue>(QueueNames.ExecuteWebJob).SingleInstance();

            builder.Register(c => Mock.Of<LogEventQueue>()).SingleInstance();
            builder.Register(c => Mock.Of<LogRecordQueue>()).SingleInstance();

            builder.RegisterType<PinInThePostService>().As<PinInThePostService>().SingleInstance();

            // UI Presentation
            builder.Register(
                    c =>
                        c.ResolveAsMock<ScopePresentation>(
                                false,
                                typeof(IScopeBusinessLogic),
                                typeof(IDataRepository),
                                typeof(IOrganisationBusinessLogic))
                            .Object)
                .As<IScopePresentation>()
                .InstancePerLifetimeScope();

            builder.Register(
                    c => new ViewingService(
                        c.Resolve<IDataRepository>(),
                        c.Resolve<ISearchRepository<EmployerSearchModel>>(),
                        c.Resolve<ISearchRepository<SicCodeSearchModel>>(),
                        c.Resolve<ICommonBusinessLogic>()))
                .As<IViewingService>()
                .InstancePerLifetimeScope();

            builder.Register(
                    c => new SubmissionService(
                        c.Resolve<IDataRepository>(),
                        c.Resolve<IScopeBusinessLogic>(),
                        c.Resolve<IFileRepository>(),
                        c.Resolve<IDraftFileBusinessLogic>(),
                        c.Resolve<IOptionsSnapshot<SubmissionOptions>>()))
                .As<ISubmissionService>()
                .InstancePerLifetimeScope();

            builder.RegisterType<CompareViewService>().As<ICompareViewService>().InstancePerLifetimeScope();
            builder.RegisterType<SearchViewService>().As<ISearchViewService>().InstancePerLifetimeScope();
            builder.RegisterType<ChangeEmailViewService>().As<IChangeEmailViewService>().InstancePerLifetimeScope();
            builder.RegisterType<ChangeDetailsViewService>().As<IChangeDetailsViewService>().InstancePerLifetimeScope();
            builder.RegisterType<ChangePasswordViewService>().As<IChangePasswordViewService>().InstancePerLifetimeScope();
            builder.RegisterType<CloseAccountViewService>().As<ICloseAccountViewService>().InstancePerLifetimeScope();

            builder.RegisterInstance(new NullLoggerFactory()).As<ILoggerFactory>().SingleInstance();
            builder.RegisterGeneric(typeof(Logger<>)).As(typeof(ILogger<>)).SingleInstance();

            builder.RegisterType<AuditLogger>().As<AuditLogger>().SingleInstance();

            builder.Register(c => new Mock<IAdminService>().Object).As<IAdminService>().InstancePerLifetimeScope();
            builder.Register(c => Mock.Of<IObfuscator>()).As<IObfuscator>().SingleInstance();
            builder.Register(c => Mock.Of<IEncryptionHandler>()).As<IEncryptionHandler>().SingleInstance();

            //Register WebTracker
            builder.Register(c => Mock.Of<IWebTracker>()).As<IWebTracker>().InstancePerLifetimeScope();
            //Register the global instance of Program.MvcApplication (equavalent in old Global.asax.cs)
            //Specify WithAttributeFiltering for the consumer - required to resolve with Keyed attributes
            builder.RegisterType<MvcApplication>().As<IMvcApplication>().SingleInstance().WithAttributeFiltering();

            //Register all BaseControllers - this is required to ensure KeyFilter is resolved in constructors
            builder.RegisterAssemblyTypes(typeof(BaseController).Assembly)
                .Where(t => t.IsAssignableTo<BaseController>())
                .InstancePerLifetimeScope()
                .WithAttributeFiltering();
            
            //Register all Controllers - this is required to ensure KeyFilter is resolved in constructors
            builder.RegisterAssemblyTypes(typeof(AccountCreationController).Assembly)
                .Where(t => t.IsAssignableTo<AccountCreationController>())
                .InstancePerLifetimeScope()
                .WithAttributeFiltering();
            
            //Register all controllers - this is required to ensure KeyFilter is resolved in constructors
            builder.RegisterType<AdminUnconfirmedPinsController>().InstancePerLifetimeScope();

            builder.Register(c => new MockCache()).As<IDistributedCache>().SingleInstance();
            builder.RegisterType<HttpCache>().As<IHttpCache>().SingleInstance();
            builder.RegisterType<HttpSession>().As<IHttpSession>().InstancePerLifetimeScope();
            builder.Register(c => Mock.Of<IHttpContextAccessor>()).As<IHttpContextAccessor>().InstancePerLifetimeScope();

            builder.RegisterType<ActionContextAccessor>().As<IActionContextAccessor>().SingleInstance();
            builder.Register(c => Mock.Of<IUrlHelper>()).SingleInstance();

            //builder.Register(c => new Mock()).As<IFileRepository>();

            var environmentMock = new Mock<IHostingEnvironment>();
            environmentMock.SetupGet(m => m.WebRootPath).Returns(Environment.CurrentDirectory);
            builder.RegisterInstance(environmentMock.Object).As<IHostingEnvironment>().SingleInstance();

            SetupHelpers.SetupMockLogRecordGlobals(builder);

            IContainer container = builder.Build();

            Mapper.Reset();
            Mapper.Initialize(
                config => {
                    // allows auto mapper to inject our dependencies
                    config.ConstructServicesUsing(container.Resolve);
                    // register all out mapper profiles (classes/mappers/*)
                    config.AddMaps(typeof(MvcApplication).Assembly);
                });

            return container;
        }
        
       
                /// <summary>
        ///     Registers an InMemory SQLRespository and populates with entities
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="dbObjects"></param>
        public static void RegisterInMemoryTestDatabase(this ContainerBuilder builder, params object[] dbObjects)
        {
            GpgDatabaseContext dbContext = CreateInMemoryTestDatabase(dbObjects);
            builder.Register(c => new SqlRepository(dbContext)).As<IDataRepository>().InstancePerLifetimeScope();
        }

        public static GpgDatabaseContext CreateInMemoryTestDatabase(params object[] dbObjects)
        {
            //Get the method name of the unit test or the parent
            string testName = TestContext.CurrentContext.Test.FullName;
            if (string.IsNullOrWhiteSpace(testName))
            {
                testName = MethodBase.GetCurrentMethod().FindParentWithAttribute<TestAttribute>().Name;
            }

            DbContextOptionsBuilder<GpgDatabaseContext> optionsBuilder =
                new DbContextOptionsBuilder<GpgDatabaseContext>().UseInMemoryDatabase(testName);

            optionsBuilder.ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning));

            // show more detailed EF errors i.e. ReturnId value instead of '{ReturnId}' n the logs etc...
            optionsBuilder.EnableSensitiveDataLogging();

            var dbContext = new GpgDatabaseContext(optionsBuilder.Options);
            if (dbObjects != null && dbObjects.Length > 0)
            {
                foreach (object item in dbObjects)
                {
                    var enumerable = item as IEnumerable<object>;
                    if (enumerable == null)
                    {
                        dbContext.Add(item);
                    }
                    else
                    {
                        dbContext.AddRange(enumerable);
                    }
                }

                dbContext.SaveChanges();
            }

            return dbContext;
        }
        
                /// <summary>
        ///     Adds a new mock url to the action method of an existing or new Mock URl helper of the controller
        /// </summary>
        /// <param name="controller">The source controller to add the mock setup</param>
        /// <param name="url">The url to be returned by the Action method</param>
        /// <param name="actionName">The name of the action to mock. If null tries to get from Controller.RoutData.</param>
        /// <param name="controllerName">
        ///     The name of the action to mock. If null tries to get from Controller.RoutData, then uses
        ///     the Controller name.
        /// </param>
        public static void AddMockUriHelperNew(this Controller controller, string url, string actionName = null, string controllerName = null)
        {
            
            if (controllerName != null)
            {
                controllerName = controllerName.BeforeFirst("Controller");
            }

            var uri = new Uri(url);
            if (uri.Authority.EqualsI(Uri.Authority))
            {
                url = uri.PathAndQuery;
            }

            Mock<IUrlHelper> mockUrlHelper = controller.Url?.GetMockFromObject() ?? new Mock<IUrlHelper>();
            Expression<Func<IUrlHelper, string>> urlSetup = helper => helper.Action(
                It.Is<UrlActionContext>(
                    uac => (string.IsNullOrWhiteSpace(actionName) || uac.Action == actionName)
                           && (string.IsNullOrWhiteSpace(controllerName) || uac.Controller == controllerName || uac.Controller == null)));

            mockUrlHelper.Setup(urlSetup).Returns(url).Verifiable();

            controller.Url = mockUrlHelper.Object;
        }

    }
    
    
}
