using GenderPayGap.WebUI.BusinessLogic.Classes;
using GenderPayGap.WebUI.BusinessLogic.Models.Organisation;
using GenderPayGap.WebUI.BusinessLogic.Models.Submit;
using NUnit.Framework;

namespace GenderPayGap.BusinessLogic.Tests.Models.Submit
{
    [TestFixture]
    public class ReturnViewModelTests
    {

        [Test]
        public void HasDraftWithContent_When_Draft_Is_Null_Returns_False()
        {
            // Arrange
            var testReturnViewModel = new ReturnViewModel {ReportInfo = new ReportInfoModel {Draft = null}};

            // Act
            bool actualHasDraftWithContent = testReturnViewModel.HasDraftWithContent();

            // Assert
            Assert.False(
                actualHasDraftWithContent,
                "If there isn't a Draft object inside the report info, the method won't be able to check further so it is expected to return false");
        }

        [Test]
        public void HasDraftWithContent_When_ReportInfo_Is_Null_Returns_False()
        {
            // Arrange
            var testReturnViewModel = new ReturnViewModel {ReportInfo = null};

            // Act
            bool actualHasDraftWithContent = testReturnViewModel.HasDraftWithContent();

            // Assert
            Assert.False(
                actualHasDraftWithContent,
                "If there isn't a reportInfo the method won't be able to check further so it is expected to return false");
        }

        [Test]
        public void HasDraftWithContent_When_ReturnViewModelContent_Is_Not_Null_Returns_True()
        {
            // Arrange
            var returnViewModelContentUnderDraft = new ReturnViewModel();
            var testReturnViewModel = new ReturnViewModel {
                ReportInfo = new ReportInfoModel {
                    Draft = new Draft(default, default) {ReturnViewModelContent = returnViewModelContentUnderDraft}
                }
            };

            // Act
            bool actualHasDraftWithContent = testReturnViewModel.HasDraftWithContent();

            // Assert
            Assert.True(
                actualHasDraftWithContent,
                "This method checks for the existence of the return view model content, so if it's NOT null, then it is of course expected to return 'true' because it has a Draft and the content inside it has indeed been populated");
        }


        [Test]
        public void HasDraftWithContent_When_ReturnViewModelContent_Is_Null_Returns_False()
        {
            // Arrange
            var testReturnViewModel = new ReturnViewModel {
                ReportInfo = new ReportInfoModel {Draft = new Draft(default, default) {ReturnViewModelContent = null}}
            };

            // Act
            bool actualHasDraftWithContent = testReturnViewModel.HasDraftWithContent();

            // Assert
            Assert.False(
                actualHasDraftWithContent,
                "This method checks for the existence of the return view model content, so if it's null, then it is of course expected to return false because it has indeed draft but the content wasn't loaded (i.e. when stashing/unStashing recursive objects are ignored)");
        }

    }
}
