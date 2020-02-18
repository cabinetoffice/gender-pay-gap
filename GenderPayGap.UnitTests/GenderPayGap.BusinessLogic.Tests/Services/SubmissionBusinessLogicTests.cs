using GenderPayGap.BusinessLogic.Models.Submit;
using GenderPayGap.Core;
using GenderPayGap.Core.Interfaces;
using GenderPayGap.Database;
using GenderPayGap.Tests.Common.TestHelpers;
using Moq;
using NUnit.Framework;

namespace GenderPayGap.BusinessLogic.Tests.Services
{
    [TestFixture]
    [SetCulture("en-GB")]
    public class SubmissionBusinessLogicTests
    {

        /*
         * These tests are specifically left here containing 'returnId' and 'size' because there was a discussion in regards to whether size was relevant to determine if an organisation is expected to provide data (report) or not for a given year.
         * In the end it was decided it didn't, the message 'you need to report' or 'you didn't need to report' will be based on organisationScope only.
         *         */

        [TestCase(0, 249, ScopeStatuses.InScope, true)]
        [TestCase(0, 499, ScopeStatuses.InScope, true)]
        [TestCase(0, 249, ScopeStatuses.OutOfScope, false)]
        [TestCase(0, 499, ScopeStatuses.OutOfScope, false)]
        [TestCase(0, 249, ScopeStatuses.PresumedInScope, true)]
        [TestCase(0, 499, ScopeStatuses.PresumedInScope, true)]
        [TestCase(0, 249, ScopeStatuses.PresumedOutOfScope, false)]
        [TestCase(0, 499, ScopeStatuses.PresumedOutOfScope, false)]
        [TestCase(1, 249, ScopeStatuses.InScope, true)]
        [TestCase(1, 499, ScopeStatuses.InScope, true)]
        [TestCase(1, 249, ScopeStatuses.OutOfScope, false)]
        [TestCase(1, 499, ScopeStatuses.OutOfScope, false)]
        [TestCase(1, 249, ScopeStatuses.PresumedInScope, true)]
        [TestCase(1, 499, ScopeStatuses.PresumedInScope, true)]
        [TestCase(1, 249, ScopeStatuses.PresumedOutOfScope, false)]
        [TestCase(1, 499, ScopeStatuses.PresumedOutOfScope, false)]
        public void SubmissionBusinessLogic_IsInScopeForThisReportYear_ReturnsTrueFalseCorrectly(int returnId,
            int maxEmployees,
            ScopeStatuses scope,
            bool expected)
        {
            // Arrange
            var submissionBusinessLogic = new SubmissionBusinessLogic(
                Mock.Of<ICommonBusinessLogic>(),
                Mock.Of<IDataRepository>());

            Organisation organisation = OrganisationHelper.GetOrganisationInAGivenScope(scope, "employerRefInScope", 2017);
            Return returnToConvert = ReturnHelper.CreateTestReturn(organisation);

            returnToConvert.ReturnId = returnId;
            returnToConvert.MaxEmployees = maxEmployees;

            // Act
            ReturnViewModel actualConvertedReturnViewModel =
                submissionBusinessLogic.ConvertSubmissionReportToReturnViewModel(returnToConvert);

            // Assert
            Assert.AreEqual(expected, actualConvertedReturnViewModel.IsInScopeForThisReportYear);
        }

    }
}
