using System;
using System.Linq;
using System.Reflection;
using GenderPayGap.WebUI.Controllers;
using Microsoft.AspNetCore.Authorization;
using NUnit.Framework;

namespace GenderPayGap.WebUI.Tests.Classes.BaseClasses
{
    [TestFixture]
    [SetCulture("en-GB")]
    public class BaseControllerTests
    {
        [TestCase(typeof(RegisterController), "PINSent", null, typeof(AuthorizeAttribute))]
        public void BaseController_Verify_Some_Method_Is_Decorated_With_Some_Attribute(Type controllerType,
            string methodName,
            Type modelArgumentForTheMethod,
            Type customAttributeToLookFor)
        {
            MethodInfo methodInfo = modelArgumentForTheMethod != null
                ? controllerType.GetMethod(methodName, new[] {modelArgumentForTheMethod})
                : controllerType.GetMethod(methodName);

            Assert.NotNull(methodInfo, $"Expected '{controllerType.Name}' to contain method '{methodName}'");

            object[] attributes = methodInfo.GetCustomAttributes(customAttributeToLookFor, true);

            string methodArguments = modelArgumentForTheMethod != null
                ? $"{modelArgumentForTheMethod.Name} model"
                : string.Empty;

            Assert.IsTrue(
                attributes.Any(),
                $"Expected custom attribute '{customAttributeToLookFor.Name}' to be decorating method '{methodName}({methodArguments})'");
        }

    }
}
