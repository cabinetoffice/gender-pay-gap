using GenderPayGap.Core.Classes;
using GenderPayGap.Extensions;
using Moq;
using NUnit.Framework;

namespace GenderPayGap.WebUI.Tests.Controllers
{
    [TestFixture]
    [SetCulture("en-GB")]
    public class ViewingControllerTests
    {

        public static string ConfigureObfuscator(long valueToObfuscate)
        {
            string result = new InternalObfuscator().Obfuscate(valueToObfuscate.ToString());

            Mock<IObfuscator> mockedObfuscatorToSetup = AutoFacExtensions.ResolveAsMock<IObfuscator>();

            mockedObfuscatorToSetup
                .Setup(x => x.Obfuscate(It.IsAny<int>()))
                .Returns(result);

            mockedObfuscatorToSetup
                .Setup(x => x.Obfuscate(It.IsAny<string>()))
                .Returns(result);

            mockedObfuscatorToSetup
                .Setup(x => x.DeObfuscate(result))
                .Returns(valueToObfuscate.ToInt32());

            return result;
        }

    }
}
