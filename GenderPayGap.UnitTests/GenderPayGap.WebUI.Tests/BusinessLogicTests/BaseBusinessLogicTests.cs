using Autofac;
using GenderPayGap.Core.Classes;
using GenderPayGap.Core.Interfaces;
using GenderPayGap.WebUI.BusinessLogic.Services;
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
            builder.Register(c => Mock.Of<IScopeBusinessLogic>()).As<IScopeBusinessLogic>();
            builder.Register(c => Mock.Of<UpdateFromCompaniesHouseService>()).As<UpdateFromCompaniesHouseService>();
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
