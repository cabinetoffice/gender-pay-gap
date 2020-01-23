using System.Collections.Generic;
using System.Linq;
using GenderPayGap.Tests.Common.TestHelpers;
using NUnit.Framework;

namespace GenderPayGap.Database.Tests
{
    [TestFixture]
    public class OrganisationTests
    {

        [SetUp]
        public void BeforeEach()
        {
            _testOrganisation = OrganisationHelper.GetPublicOrganisation();

            Return mocked2017Return = ReturnHelper.CreateTestReturn(_testOrganisation);
            OrganisationHelper.LinkOrganisationAndReturn(_testOrganisation, mocked2017Return);

            Return mocked2018Return = ReturnHelper.CreateTestReturn(_testOrganisation, 2018);
            OrganisationHelper.LinkOrganisationAndReturn(_testOrganisation, mocked2018Return);
        }

        private Organisation _testOrganisation;

        [TestCase(2, 2)]
        [TestCase(1, 1)]
        public void OrganisationDB_GetRecentReportingYears_Returns_The_Correct_Years(int countOfYears, int expected)
        {
            // Arrange - Act
            IEnumerable<int> returnedDates = _testOrganisation.GetRecentReportingYears(countOfYears);
            int actualCountOfReturnedDates = returnedDates.Count();

            // Assert
            Assert.AreEqual(expected, actualCountOfReturnedDates);
        }

        [TestCase(2, 2)]
        [TestCase(1, 1)]
        public void OrganisationDB_GetRecentReports_Returns_The_Correct_Years(int countOfYears, int expected)
        {
            // Arrange - Act
            IEnumerable<Return> returnsAvailableInPreviousYears = _testOrganisation.GetRecentReports(countOfYears);
            int actualCount = returnsAvailableInPreviousYears.Count();

            // Assert
            Assert.AreEqual(expected, actualCount);
        }

        [Test]
        public void OrganisationDB_GetRecentReports_Returns_New_Return_If_No_Data_Available()
        {
            // todo: the logic under test here should be changed, since it's not correct that a 'new return' - filled with zeroes!! - is created if the system doesn't find one a return on the database for a particular year.

            // Arrange
            _testOrganisation.Returns = new List<Return>(); // remove returns

            // Act
            IEnumerable<Return> returnsAvailableForTheLastTwoYears = _testOrganisation.GetRecentReports(2);
            int actualCount = returnsAvailableForTheLastTwoYears.Count();
            Return returnForCurrentYear = returnsAvailableForTheLastTwoYears.ElementAt(0);
            Return returnForPreviousYear = returnsAvailableForTheLastTwoYears.ElementAt(1);

            // Assert
            Assert.AreEqual(2, actualCount);
            Assert.AreEqual(0, returnForCurrentYear.ReturnId);
            Assert.AreEqual(0, returnForPreviousYear.ReturnId);
        }

    }
}
