using Autofac;
using GenderPayGap.BusinessLogic;
using GenderPayGap.BusinessLogic.Services;
using GenderPayGap.Core;
using GenderPayGap.Core.API;
using GenderPayGap.Core.Classes;
using GenderPayGap.Core.Interfaces;
using GenderPayGap.Core.Models;
using GenderPayGap.Extensions.AspNetCore;
using GenderPayGap.Tests.Common.Classes;
using GenderPayGap.Tests.Common.Mocks;
using GenderPayGap.Tests.Common.TestHelpers;
using GenderPayGap.WebJob.Services;
using Microsoft.Azure.Search;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace GenderPayGap.WebJob.Tests.TestHelpers
{
    internal static class WebJobTestHelper
    {

        private static IContainer BuildContainerIoC(params object[] dbObjects)
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<WebJob.Functions>().InstancePerDependency();

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

            builder.Register(c => Mock.Of<ICompaniesHouseAPI>()).As<ICompaniesHouseAPI>().SingleInstance();

            builder.Register(c => Mock.Of<ISearchServiceClient>()).As<ISearchServiceClient>().SingleInstance();

            //Create the mock repositories
            builder.Register(c => new MockFileRepository()).As<IFileRepository>().SingleInstance();
            builder.Register(c => new MockSearchRepository()).As<ISearchRepository<EmployerSearchModel>>().SingleInstance();
            builder.RegisterType(typeof(SicCodeSearchRepository)).As<ISearchRepository<SicCodeSearchModel>>().SingleInstance();

            // BL Services
            builder.RegisterInstance(Config.Configuration).SingleInstance();
            builder.RegisterType<CommonBusinessLogic>().As<ICommonBusinessLogic>().InstancePerLifetimeScope();
            builder.RegisterType<ScopeBusinessLogic>().As<IScopeBusinessLogic>().InstancePerLifetimeScope();
            builder.RegisterType<SubmissionBusinessLogic>().As<ISubmissionBusinessLogic>().InstancePerLifetimeScope();
            builder.RegisterType<SecurityCodeBusinessLogic>().As<ISecurityCodeBusinessLogic>().InstancePerLifetimeScope();
            builder.RegisterType<OrganisationBusinessLogic>().As<IOrganisationBusinessLogic>().InstancePerLifetimeScope();
            builder.RegisterType<SearchBusinessLogic>().As<ISearchBusinessLogic>().InstancePerLifetimeScope();
            builder.RegisterType<UpdateFromCompaniesHouseService>().As<UpdateFromCompaniesHouseService>().InstancePerLifetimeScope();

            //Register some singletons
            builder.RegisterType<InternalObfuscator>().As<IObfuscator>().SingleInstance();
            builder.RegisterType<EncryptionHandler>().As<IEncryptionHandler>().SingleInstance();

            builder.Register(c => Mock.Of<IGovNotifyAPI>()).As<IGovNotifyAPI>().SingleInstance();
            builder.RegisterType<EmailSendingService>().As<EmailSendingService>().InstancePerLifetimeScope();

            builder.RegisterInstance(new NullLoggerFactory()).As<ILoggerFactory>().SingleInstance();

            builder.RegisterGeneric(typeof(Logger<>))
                .As(typeof(ILogger<>))
                .SingleInstance();

            return builder.Build();
        }

        public static WebJob.Functions SetUp(params object[] dbObjects)
        {
            IContainer containerIoc = BuildContainerIoC(dbObjects);
            var functions = containerIoc.Resolve<WebJob.Functions>();

            //Create the mock repositories
            Global.FileRepository = containerIoc.Resolve<IFileRepository>();
            Global.SearchRepository = containerIoc.Resolve<ISearchRepository<EmployerSearchModel>>();
            Global.SicCodeSearchRepository = containerIoc.Resolve<ISearchRepository<SicCodeSearchModel>>();

            return functions;
        }

    }
}
