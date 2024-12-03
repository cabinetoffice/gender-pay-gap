using System.Reflection;
using Microsoft.AspNetCore.Mvc;

namespace GenderPayGap.WebUI.Tests.CodeQualityTests
{
    [TestFixture]
    public class EnsureAllPostRequestsRequireAntiForgeryTokenTests
    {

        [Test]
        public void EnsureAllPostRequestsRequireAntiForgeryToken()
        {
            Assembly assembly = typeof(Program).Assembly;

            var methodsMissingAttribute = new List<MethodInfo>();
            foreach (Type type in assembly.GetTypes())
            {
                if (typeof(Controller).IsAssignableFrom(type))
                {
                    foreach (MethodInfo method in type.GetMethods())
                    {
                        if (method.GetCustomAttributes<HttpPostAttribute>().Any())
                        {
                            if (!method.GetCustomAttributes<ValidateAntiForgeryTokenAttribute>().Any())
                            {
                                methodsMissingAttribute.Add(method);
                            }
                        }
                        
                    }
                }
            }

            if (methodsMissingAttribute.Any())
            {
                Assert.Fail(
                    $"All POST methods must use a ValidateAntiForgeryToken\n"
                    + $"The following methods are missing the ValidateAntiForgeryToken attribute\n"
                    + $"- {string.Join("\n- ", methodsMissingAttribute.Select(c => $"Controller [{c.DeclaringType.Name}] Method [{c.Name}]"))}\n");
            }

        }

    }
}
