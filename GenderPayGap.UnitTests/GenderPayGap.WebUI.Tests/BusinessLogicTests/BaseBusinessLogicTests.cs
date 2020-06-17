using Autofac;
using GenderPayGap.BusinessLogic.Services;
using GenderPayGap.Core.Classes;
using GenderPayGap.Core.Interfaces;
using Moq;

namespace GenderPayGap.BusinessLogic.Tests
{
    public class BaseBusinessLogicTests
    {

        private readonly ILifetimeScope _lifetimeScope;

        public BaseBusinessLogicTests()
        {
            var builder = new ContainerBuilder();

            builder.Register(c => Mock.Of<IOrganisationBusinessLogic>()).As<IOrganisationBusinessLogic>();
            builder.Register(c => Mock.Of<IDataRepository>()).As<IDataRepository>();
            builder.Register(c => Mock.Of<ISubmissionBusinessLogic>()).As<ISubmissionBusinessLogic>();
            builder.Register(c => Mock.Of<IScopeBusinessLogic>()).As<IScopeBusinessLogic>();
            builder.Register(c => Mock.Of<UpdateFromCompaniesHouseService>()).As<UpdateFromCompaniesHouseService>();
            builder.Register(c => Mock.Of<IEncryptionHandler>()).As<IEncryptionHandler>();
            builder.Register(c => Mock.Of<IObfuscator>()).As<IObfuscator>();

            IContainer container = builder.Build();

            _lifetimeScope = container.BeginLifetimeScope();
        }

        public T Get<T>()
        {
            return _lifetimeScope.Resolve<T>();
        }

    }
}
