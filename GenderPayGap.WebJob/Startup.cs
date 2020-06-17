using System;
using System.Net.Http;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using GenderPayGap.BusinessLogic;
using GenderPayGap.BusinessLogic.Services;
using GenderPayGap.Core;
using GenderPayGap.Core.API;
using GenderPayGap.Core.Classes;
using GenderPayGap.Core.Interfaces;
using GenderPayGap.Database;
using GenderPayGap.Extensions;
using GenderPayGap.Extensions.AspNetCore;
using GenderPayGap.WebJob.Services;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.WindowsAzure.Storage.RetryPolicies;

namespace GenderPayGap.WebJob
{
    public class Startup
    {

        // ConfigureServices is where you register dependencies. This gets
        // called by the runtime before the ConfigureContainer method, below.
        public static void ConfigureServices(IServiceCollection services)
        {
            //Add a dedicated httpClient for Companies house API with exponential retry policy
            services.AddHttpClient<ICompaniesHouseAPI, CompaniesHouseAPI>(nameof(ICompaniesHouseAPI), CompaniesHouseAPI.SetupHttpClient)
                .SetHandlerLifetime(TimeSpan.FromMinutes(10))
                .AddPolicyHandler(CompaniesHouseAPI.GetRetryPolicy());

            //Create the Autofac inversion of control container
            var builder = new ContainerBuilder();

            //Populate autoAutofacfac container with any existing services registered already
            builder.Populate(services);

            //Configure the container
            BuildContainerIoC(builder);

            //Build the container
            Program.ContainerIOC = builder.Build();

            //Register Autofac as the service provider
            services.AddSingleton<IServiceProvider>(new AutofacServiceProvider(Program.ContainerIOC));

            //Register the webJobs IJobActivator
            services.AddSingleton(Program.ContainerIOC);
            services.AddSingleton<IJobActivator, AutofacJobActivator>();
        }

        public static void BuildContainerIoC(ContainerBuilder builder)
        {
            // Need to register webJob class in Autofac as well
            builder.RegisterType<Functions>().InstancePerDependency();

            builder.Register(c => new SqlRepository(new GpgDatabaseContext(WebJobGlobal.DatabaseConnectionString)))
                .As<IDataRepository>()
                .InstancePerDependency();
            builder.RegisterType<CompaniesHouseAPI>()
                .As<ICompaniesHouseAPI>()
                .SingleInstance()
                .WithParameter(
                    (p, ctx) => p.ParameterType == typeof(HttpClient),
                    (p, ctx) => ctx.Resolve<IHttpClientFactory>().CreateClient(nameof(ICompaniesHouseAPI)));

            // validate we have a storage connection
            if (string.IsNullOrWhiteSpace(WebJobGlobal.AzureStorageConnectionString))
            {
                throw new InvalidOperationException("No Azure Storage connection specified. Check the config.");
            }

            //Set the default encryption key
            Encryption.SetDefaultEncryptionKey(WebJobGlobal.DefaultEncryptionKey);

            builder.RegisterType<GovNotifyAPI>().As<IGovNotifyAPI>().SingleInstance();
            builder.RegisterType<EmailSendingService>().As<EmailSendingService>().SingleInstance();

            // BL Services
            builder.RegisterInstance(Config.Configuration).SingleInstance();
            builder.RegisterType<CommonBusinessLogic>().As<ICommonBusinessLogic>().SingleInstance();
            builder.RegisterType<ScopeBusinessLogic>().As<IScopeBusinessLogic>().InstancePerDependency();
            builder.RegisterType<SubmissionBusinessLogic>().As<ISubmissionBusinessLogic>().InstancePerDependency();
            builder.RegisterType<OrganisationBusinessLogic>().As<IOrganisationBusinessLogic>().InstancePerDependency();
            builder.RegisterType<UpdateFromCompaniesHouseService>().As<UpdateFromCompaniesHouseService>().InstancePerDependency();

            //Register some singletons
            builder.RegisterType<InternalObfuscator>().As<IObfuscator>().SingleInstance();
            builder.RegisterType<EncryptionHandler>().As<IEncryptionHandler>().SingleInstance();
        }

    }
}
