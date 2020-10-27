using System.Collections.Generic;
using System.Linq;
using GenderPayGap.Core.Classes;
using GenderPayGap.Core.Interfaces;
using GenderPayGap.Database;
using GenderPayGap.Tests.Common.Classes;
using GenderPayGap.Tests.Common.TestHelpers;
using GenderPayGap.WebUI.BusinessLogic.Models.Compare;
using GenderPayGap.WebUI.BusinessLogic.Services;
using Moq;
using NUnit.Framework;

namespace GenderPayGap.BusinessLogic.Tests.Services
{
    [TestFixture]
    [SetCulture("en-GB")]
    public class OrganisationBusinessLogicTests : BaseBusinessLogicTests
    {

        private List<Return> GetFourOrgsWithVariousReturns()
        {
            var result = new List<Return>();

            #region Two organisations with returns that have all their information filled correctly

            for (var i = 0; i < 2; i++)
            {
                Organisation organisationInfoCorrect = OrganisationHelper.GetMockedOrganisation();
                Return returnInfoCorrect = ReturnHelper.CreateTestReturn(organisationInfoCorrect);
                result.Add(returnInfoCorrect);
            }

            #endregion

            #region Organisation with return that doesn't contain bonus info

            var tempOrg = new Organisation();
            var tempReturn = new Return();
            do
            {
                tempOrg = OrganisationHelper.GetMockedOrganisation();
                tempReturn = ReturnHelper.CreateTestReturnWithNoBonus(tempOrg);
            } while (result.Any(x => x.OrganisationId == tempOrg.OrganisationId));

            result.Add(tempReturn);

            #endregion

            #region Organisation with return that has a bonus information completed as 0%

            do
            {
                tempOrg = OrganisationHelper.GetMockedOrganisation();
                tempReturn = ReturnHelper.CreateBonusTestReturn(
                    tempOrg,
                    0,
                    0,
                    0,
                    0);
            } while (result.Any(x => x.OrganisationId == tempOrg.OrganisationId));

            result.Add(tempReturn);

            #endregion

            #region Organisation with return that has a bonus information filled with negative numbers

            do
            {
                tempOrg = OrganisationHelper.GetMockedOrganisation();
                tempReturn = ReturnHelper.CreateBonusTestReturn(
                    tempOrg,
                    -15,
                    -34,
                    -56,
                    -78);
            } while (result.Any(x => x.OrganisationId == tempOrg.OrganisationId));

            result.Add(tempReturn);

            #endregion

            return result;
        }

        [Test]
        public void OrganisationBusinessLogic_GetCompareData_Leaves_Null_Values_At_The_Bottom_Of_The_List()
        {
            // Arrange
            List<Return> listOfReturns = GetFourOrgsWithVariousReturns();

            IEnumerable<Organisation> listOfOrgs = listOfReturns.Select(ret => ret.Organisation);

            Mock<IDataRepository> mockedDataRepository = MoqHelpers.CreateMockDataRepository();

            mockedDataRepository.SetupGetAll(listOfOrgs, listOfReturns);

            IDataRepository dataRepository = mockedDataRepository.Object;


            var submissionBusinessLogic = new SubmissionBusinessLogic(dataRepository);

            var mockedEncryptionHandler = Get<IEncryptionHandler>();
            var mockedObfuscator = Get<IObfuscator>();

            var organisationBusinessLogic = new OrganisationBusinessLogic(
                dataRepository,
                submissionBusinessLogic,
                mockedEncryptionHandler,
                mockedObfuscator);

            IEnumerable<string> listEncOrgIds = listOfReturns.Select(x => mockedObfuscator.Obfuscate(x.OrganisationId.ToString()));
            var year = 2017;
            var sortColumn = "DiffMedianBonusPercent";
            var sortAscending = true;

            // Act
            IEnumerable<CompareReportModel> data = organisationBusinessLogic.GetCompareData(
                listEncOrgIds,
                year,
                sortColumn,
                sortAscending);

            // Assert
        }

    }
}
