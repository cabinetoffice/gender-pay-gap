using GenderPayGap.WebUI.Models.Submit;
using NUnit.Framework;

namespace GenderPayGap.WebUI.Tests.Models.Submit
{
    [TestFixture]
    public class SubmissionChangeSummaryTests
    {

        #region OrganisationSizeChanged

        [Test]
        public void SubmissionChangeSummary_ShouldProvideLateReason_OrganisationSizeChanged_Returns_False_When_Changed()
        {
            // Arrange
            var submissionChangeSummary = new SubmissionChangeSummary {OrganisationSizeChanged = true, IsPrevReportingStartYear = true};

            // Act
            bool actual = submissionChangeSummary.ShouldProvideLateReason;

            // Assert
            Assert.False(
                actual,
                "When the Organisation Size has changed for a report belonging to the previous year, the user does NOT need to provide a late reason");
        }

        #endregion

        #region WebsiteUrlChanged

        [Test]
        public void SubmissionChangeSummary_ShouldProvideLateReason_WebsiteUrlChanged_Returns_False_When_Changed()
        {
            // Arrange
            var submissionChangeSummary = new SubmissionChangeSummary {WebsiteUrlChanged = true, IsPrevReportingStartYear = true};

            // Act
            bool actual = submissionChangeSummary.ShouldProvideLateReason;

            // Assert
            Assert.False(
                actual,
                "When the Website Url has changed for a report belonging to the previous year, the user does NOT need to provide a late reason");
        }

        #endregion

        #region FiguresChanged

        [Test]
        public void SubmissionChangeSummary_ShouldProvideLateReason_Figures_Returns_True_When_Changed()
        {
            // Arrange
            var submissionChangeSummary = new SubmissionChangeSummary {FiguresChanged = true, IsPrevReportingStartYear = true};

            // Act
            bool actual = submissionChangeSummary.ShouldProvideLateReason;

            // Assert
            Assert.True(
                actual,
                "When the figures have changed for a report belonging to the previous year, the user 'ShouldProvideLateReason'");
        }

        [Test]
        public void SubmissionChangeSummary_ShouldProvideLateReason_Figures_Returns_False_When_Changed_On_Current_Year()
        {
            // Arrange
            var submissionChangeSummary = new SubmissionChangeSummary {FiguresChanged = true, IsPrevReportingStartYear = false};

            // Act
            bool actual = submissionChangeSummary.ShouldProvideLateReason;

            // Assert
            Assert.False(
                actual,
                "When the figures have changed for a report belonging to the current year, the user does not need to provide a late reason");
        }

        #endregion

        #region PersonResonsibleChanged

        [Test]
        public void SubmissionChangeSummary_ShouldProvideLateReason_PersonResponsibleChanged_Returns_False_When_Changed()
        {
            // Arrange
            var submissionChangeSummary = new SubmissionChangeSummary {PersonResonsibleChanged = true, IsPrevReportingStartYear = true};

            // Act
            bool actual = submissionChangeSummary.ShouldProvideLateReason;

            // Assert
            Assert.False(
                actual,
                "When the Person Responsible has changed for a report belonging to the previous year, the user does not need to provide a late reason");
        }

        [Test]
        public void SubmissionChangeSummary_ShouldProvideLateReason_PersonResponsibleChanged_Returns_False_When_Changed_On_Current_Year()
        {
            // Arrange
            var submissionChangeSummary = new SubmissionChangeSummary {PersonResonsibleChanged = true, IsPrevReportingStartYear = false};

            // Act
            bool actual = submissionChangeSummary.ShouldProvideLateReason;

            // Assert
            Assert.False(
                actual,
                "When the Person Responsible has changed for a report belonging to the current year, the user does not need to provide a late reason");
        }

        #endregion

    }
}
