using GenderPayGap.WebUI.BusinessLogic.Classes;
using GenderPayGap.WebUI.BusinessLogic.Models.Submit;
using NUnit.Framework;

namespace GenderPayGap.BusinessLogic.Tests.Classes
{
    [TestFixture]
    public class DraftTests
    {

        [Test]
        public void HasContent_When_ReturnViewModelContent_Is_Not_Null_Returns_True()
        {
            // Arrange
            var testDraft = new Draft(default, default) {ReturnViewModelContent = new ReturnViewModel()};

            // Act
            bool actualHasContent = testDraft.HasContent();

            // Assert
            Assert.True(actualHasContent, "If there is a ReturnViewModelContent inside the Draft, the method is expected to return true");
        }

        [Test]
        public void HasContent_When_ReturnViewModelContent_Is_Null_Returns_False()
        {
            // Arrange
            var testDraft = new Draft(default, default) {ReturnViewModelContent = null};

            // Act
            bool actualHasContent = testDraft.HasContent();

            // Assert
            Assert.False(actualHasContent, "If there isn't a ReturnViewModelContent the method is expected to return false");
        }

    }
}
