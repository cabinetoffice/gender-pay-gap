using System;
using System.Collections.Generic;
using System.Linq;
using GenderPayGap.Core;
using GenderPayGap.Core.Interfaces;
using GenderPayGap.Database;
using GenderPayGap.Extensions;
using GenderPayGap.WebUI.BusinessLogic.Services;
using MockQueryable.Moq;
using Moq;
using NUnit.Framework;

namespace GenderPayGap.Tests
{

    // TODO: Move to BL test project :)

    [TestFixture]
    [SetCulture("en-GB")]
    public class ScopeBusinessLogicTests : AssertionHelper
    {

        private Mock<IDataRepository> mockDataRepo;

        private IList<Organisation> testOrgData;
        private IList<OrganisationScope> testOrgScopeData;
        public IScopeBusinessLogic testScopeBL;

        #region Test Data

        private void GenerateTestData()
        {
            testOrgData = new List<Organisation>(
                new[] {
                    new Organisation {OrganisationId = 1},
                    new Organisation {OrganisationId = 2},
                    new Organisation {OrganisationId = 3},
                    new Organisation {OrganisationId = 4},
                    new Organisation {OrganisationId = 5}
                });

            testOrgScopeData = new List<OrganisationScope>(
                new[] {
                    new OrganisationScope {
                        OrganisationScopeId = 15,
                        OrganisationId = 1,
                        Status = ScopeRowStatuses.Active,
                        ScopeStatusDate = VirtualDateTime.Now.AddDays(-51),
                        SnapshotDate = new DateTime(2017, 4, 5)
                    },
                    new OrganisationScope {
                        OrganisationScopeId = 25,
                        OrganisationId = 2,
                        ScopeStatusDate = VirtualDateTime.Now.AddDays(-5),
                        SnapshotDate = new DateTime(2018, 4, 5)
                    },
                    new OrganisationScope {
                        OrganisationScopeId = 35,
                        OrganisationId = 3,
                        ScopeStatusDate = VirtualDateTime.Now.AddDays(-2),
                        SnapshotDate = new DateTime(2017, 4, 5)
                    },
                    new OrganisationScope {
                        OrganisationScopeId = 45,
                        OrganisationId = 4,
                        ScopeStatusDate = VirtualDateTime.Now.AddDays(-100),
                        SnapshotDate = new DateTime(2017, 4, 5),
                        ScopeStatus = ScopeStatuses.OutOfScope
                    },
                    new OrganisationScope {
                        OrganisationScopeId = 55,
                        OrganisationId = 4,
                        ScopeStatusDate = VirtualDateTime.Now.AddDays(-44),
                        SnapshotDate = new DateTime(2017, 4, 5),
                        ScopeStatus = ScopeStatuses.InScope
                    },
                    new OrganisationScope {
                        OrganisationScopeId = 65,
                        OrganisationId = 4,
                        Status = ScopeRowStatuses.Active,
                        ScopeStatusDate = VirtualDateTime.Now.AddDays(-2),
                        SnapshotDate = new DateTime(2017, 4, 5),
                        ScopeStatus = ScopeStatuses.OutOfScope
                    }
                });

            testOrgData.ForEach(o => o.OrganisationScopes = testOrgScopeData.Where(os => o.OrganisationId == os.OrganisationId).ToList());
        }

        #endregion

        [SetUp]
        public void BeforeEach()
        {
            mockDataRepo = new Mock<IDataRepository>();
            testScopeBL = new ScopeBusinessLogic(mockDataRepo.Object);
            GenerateTestData();
        }

        #region GetLatestScopeForSnapshotYear()

        [Test]
        [Description("GetLatestScopeForSnapshotYear: When SnapshotYear doesn't Match Then Return Null")]
        public void GetLatestScopeForSnapshotYear_When_SnapshotYear_doesnt_Match_Then_Return_Null()
        {
            // Arrange
            mockDataRepo.Setup(r => r.GetAll<OrganisationScope>()).Returns(testOrgScopeData.AsQueryable().BuildMock().Object);
            mockDataRepo.Setup(r => r.GetAll<Organisation>()).Returns(testOrgData.AsQueryable().BuildMock().Object);

            // Act
            OrganisationScope result = testScopeBL.GetLatestScopeBySnapshotYear(4, 2016);

            // Assert
            Assert.That(result == null, "Expected to return null when using an existing organisation that has no scope");
        }

        [Test]
        [Description("GetLatestScopeForSnapshotYear: When SnapshotYear Matches Then Return LatestScopeForSnapshotYear")]
        public void GetLatestScopeForSnapshotYear_When_SnapshotYear_Matches_Then_Return_LatestScopeForSnapshotYear()
        {
            // Arrange
            mockDataRepo.Setup(r => r.GetAll<OrganisationScope>()).Returns(testOrgScopeData.AsQueryable().BuildMock().Object);
            mockDataRepo.Setup(r => r.GetAll<Organisation>()).Returns(testOrgData.AsQueryable().BuildMock().Object);

            // Act
            OrganisationScope result = testScopeBL.GetLatestScopeBySnapshotYear(4, 2017);

            // Assert
            Assert.That(result != null, "Expected to return a valid OrganisationScope Model");
            Assert.That(result.OrganisationId == 4, "Expected the model to have the same OrganisationId");
            Assert.That(result.OrganisationScopeId == 65, "Expected the model to have the same OrganisationScopeId");
        }

        #endregion

    }

}
