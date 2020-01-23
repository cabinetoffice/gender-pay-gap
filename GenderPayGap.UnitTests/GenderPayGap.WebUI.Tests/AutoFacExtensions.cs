using Autofac;
using GenderPayGap.WebUI.Tests.TestHelpers;
using Moq;

namespace GenderPayGap.WebUI.Tests
{
    public static class AutoFacExtensions
    {
        /// <summary>
        /// Retrieve an instance from the DependencyResolver and returns a mocked instance
        /// </summary>
        /// <typeparam name="TInstance"></typeparam>
        /// <returns></returns>
        public static Mock<TInstance> ResolveAsMock<TInstance>(bool callbase = false) where TInstance : class
        {
            // get the instance form the Inversion of Control locator
            TInstance instance = UiTestHelper.DIContainer.Resolve<TInstance>();

            // generate and return the mocked instance
            var mockedInstance = Mock.Get(instance);
            mockedInstance.CallBase = callbase;
            return mockedInstance;
        }
    }
}
