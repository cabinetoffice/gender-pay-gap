using System.Linq;
using GenderPayGap.Core.Interfaces;
using GenderPayGap.Database;
using GenderPayGap.Tests.Common.Classes;
using MockQueryable.Moq;
using Moq;
using NUnit.Framework;

namespace GenderPayGap.Tests
{
    [TestFixture]
    [SetCulture("en-GB")]
    public class SearchTests : AssertionHelper
    {

        private Mock<IDataRepository> mockDataRepo;

        #region Test Data

        private readonly OrganisationScope[] testScopeData = {
            new OrganisationScope {OrganisationScopeId = 123}, new OrganisationScope {OrganisationScopeId = 321}
        };

        #endregion

        [SetUp]
        public void BeforeEach()
        {
            mockDataRepo = MoqHelpers.CreateMockAsyncDataRepository();
        }

        [Test]
        [Description("")]
        public void ScopingViewModel_UnpackScope_FastTrack_IsNullOrWhiteSpace()
        {
            //ARRANGE
            mockDataRepo.Setup(r => r.GetAll<OrganisationScope>())
                .Returns(testScopeData.AsQueryable().BuildMock().Object);

            //ACT

            //ASSERT
        }

    }
}
