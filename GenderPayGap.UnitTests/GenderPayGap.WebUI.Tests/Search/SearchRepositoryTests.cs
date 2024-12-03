using GenderPayGap.Core;
using GenderPayGap.Database;
using GenderPayGap.WebUI.Search;
using GenderPayGap.WebUI.Tests.TestHelpers;

namespace GenderPayGap.WebUI.Tests.Search
{
    [TestFixture]
    public class SearchRepositoryTests
    {

        [Test]
        public void LoadSearchDataIntoCache_Includes_RetiredOrganisationsWithSubmittedReports()
        {
            // Arrange
            var retiredOrganisationWithSubmittedReports = new Organisation
            {
                OrganisationId = 1,
                OrganisationName = "Test organisation",
                SectorType = SectorTypes.Private,
                Status = OrganisationStatuses.Retired,
                Returns = new List<Return> {new Return {Status = ReturnStatuses.Submitted}}
            };
            Program.DependencyInjectionServiceProvider = UiTestHelper.BuildContainerIoC(retiredOrganisationWithSubmittedReports);

            // Act
            SearchRepository.LoadSearchDataIntoCache();

            // Assert
            AssertOrganisationIsIncludedInViewingService(retiredOrganisationWithSubmittedReports);
        }

        [Test]
        public void LoadSearchDataIntoCache_DoesNotInclude_RetiredOrganisationsWithNoReports()
        {
            // Arrange
            var retiredOrganisationWithNoSubmittedReports = new Organisation
            {
                OrganisationId = 1,
                OrganisationName = "Test organisation",
                SectorType = SectorTypes.Private,
                Status = OrganisationStatuses.Retired
            };
            Program.DependencyInjectionServiceProvider = UiTestHelper.BuildContainerIoC(retiredOrganisationWithNoSubmittedReports);

            // Act
            SearchRepository.LoadSearchDataIntoCache();

            // Assert
            AssertOrganisationIsNotIncludedInViewingService(retiredOrganisationWithNoSubmittedReports);
        }

        [Test]
        public void LoadSearchDataIntoCache_DoesNotInclude_RetiredOrganisationsWithDraftReports()
        {
            // Arrange
            var retiredOrganisationWithDraftReports = new Organisation
            {
                OrganisationId = 1,
                OrganisationName = "Test organisation",
                SectorType = SectorTypes.Private,
                Status = OrganisationStatuses.Retired,
                Returns = new List<Return> {new Return {Status = ReturnStatuses.Draft}}
            };
            Program.DependencyInjectionServiceProvider = UiTestHelper.BuildContainerIoC(retiredOrganisationWithDraftReports);

            // Act
            SearchRepository.LoadSearchDataIntoCache();

            // Assert
            AssertOrganisationIsNotIncludedInViewingService(retiredOrganisationWithDraftReports);
        }

        [Test]
        public void LoadSearchDataIntoCache_DoesNotInclude_RetiredOrganisationsInScope()
        {
            // Arrange
            var retiredOrganisationInScope = new Organisation
            {
                OrganisationId = 1,
                OrganisationName = "Test organisation",
                SectorType = SectorTypes.Private,
                Status = OrganisationStatuses.Retired,
                OrganisationScopes = new List<OrganisationScope>
                {
                    new OrganisationScope {Status = ScopeRowStatuses.Active, ScopeStatus = ScopeStatuses.InScope}
                }
            };
            Program.DependencyInjectionServiceProvider = UiTestHelper.BuildContainerIoC(retiredOrganisationInScope);

            // Act
            SearchRepository.LoadSearchDataIntoCache();

            // Assert
            AssertOrganisationIsNotIncludedInViewingService(retiredOrganisationInScope);
        }

        [Test]
        public void LoadSearchDataIntoCache_DoesNotInclude_RetiredOrganisationsPresumedInScope()
        {
            // Arrange
            var retiredOrganisationPresumedInScope = new Organisation
            {
                OrganisationId = 1,
                OrganisationName = "Test organisation",
                SectorType = SectorTypes.Private,
                Status = OrganisationStatuses.Retired,
                OrganisationScopes = new List<OrganisationScope>
                {
                    new OrganisationScope {Status = ScopeRowStatuses.Active, ScopeStatus = ScopeStatuses.PresumedInScope}
                }
            };
            Program.DependencyInjectionServiceProvider = UiTestHelper.BuildContainerIoC(retiredOrganisationPresumedInScope);

            // Act
            SearchRepository.LoadSearchDataIntoCache();

            // Assert
            AssertOrganisationIsNotIncludedInViewingService(retiredOrganisationPresumedInScope);
        }

        [Test]
        public void LoadSearchDataIntoCache_DoesNotInclude_RetiredOrganisationsPreviouslyInScope()
        {
            // Arrange
            var retiredOrganisationPreviouslyInScope = new Organisation
            {
                OrganisationId = 1,
                OrganisationName = "Test organisation",
                SectorType = SectorTypes.Private,
                Status = OrganisationStatuses.Retired,
                OrganisationScopes = new List<OrganisationScope>
                {
                    new OrganisationScope {Status = ScopeRowStatuses.Retired, ScopeStatus = ScopeStatuses.InScope}
                }
            };
            Program.DependencyInjectionServiceProvider = UiTestHelper.BuildContainerIoC(retiredOrganisationPreviouslyInScope);

            // Act
            SearchRepository.LoadSearchDataIntoCache();

            // Assert
            AssertOrganisationIsNotIncludedInViewingService(retiredOrganisationPreviouslyInScope);
        }

        [Test]
        public void LoadSearchDataIntoCache_DoesNotInclude_RetiredOrganisationsOutOfScope()
        {
            // Arrange
            var retiredOrganisationOutOfScope = new Organisation
            {
                OrganisationId = 1,
                OrganisationName = "Test organisation",
                SectorType = SectorTypes.Private,
                Status = OrganisationStatuses.Retired,
                OrganisationScopes = new List<OrganisationScope>
                {
                    new OrganisationScope {Status = ScopeRowStatuses.Active, ScopeStatus = ScopeStatuses.OutOfScope}
                }
            };
            Program.DependencyInjectionServiceProvider = UiTestHelper.BuildContainerIoC(retiredOrganisationOutOfScope);

            // Act
            SearchRepository.LoadSearchDataIntoCache();

            // Assert
            AssertOrganisationIsNotIncludedInViewingService(retiredOrganisationOutOfScope);
        }

        [Test]
        public void LoadSearchDataIntoCache_Includes_ActiveOrganisationsWithSubmittedReports()
        {
            // Arrange
            var activeOrganisationWithSubmittedReports = new Organisation
            {
                OrganisationId = 1,
                OrganisationName = "Test organisation",
                SectorType = SectorTypes.Private,
                Status = OrganisationStatuses.Active,
                Returns = new List<Return> {new Return {Status = ReturnStatuses.Submitted}}
            };
            Program.DependencyInjectionServiceProvider = UiTestHelper.BuildContainerIoC(activeOrganisationWithSubmittedReports);

            // Act
            SearchRepository.LoadSearchDataIntoCache();

            // Assert
            AssertOrganisationIsIncludedInViewingService(activeOrganisationWithSubmittedReports);
        }

        [Test]
        public void LoadSearchDataIntoCache_Includes_ActiveOrganisationsWithDraftReports()
        {
            // Arrange
            var activeOrganisationWithDraftReports = new Organisation
            {
                OrganisationId = 1,
                OrganisationName = "Test organisation",
                SectorType = SectorTypes.Private,
                Status = OrganisationStatuses.Active,
                Returns = new List<Return> {new Return {Status = ReturnStatuses.Draft}},
                OrganisationScopes = new List<OrganisationScope>
                {
                    new OrganisationScope {Status = ScopeRowStatuses.Active, ScopeStatus = ScopeStatuses.PresumedInScope}
                }
            };
            Program.DependencyInjectionServiceProvider = UiTestHelper.BuildContainerIoC(activeOrganisationWithDraftReports);

            // Act
            SearchRepository.LoadSearchDataIntoCache();

            // Assert
            AssertOrganisationIsIncludedInViewingService(activeOrganisationWithDraftReports);
        }

        [Test]
        public void LoadSearchDataIntoCache_Includes_ActiveOrganisationsInScope()
        {
            // Arrange
            var activeOrganisationInScope = new Organisation
            {
                OrganisationId = 1,
                OrganisationName = "Test organisation",
                SectorType = SectorTypes.Private,
                Status = OrganisationStatuses.Active,
                OrganisationScopes = new List<OrganisationScope>
                {
                    new OrganisationScope {Status = ScopeRowStatuses.Active, ScopeStatus = ScopeStatuses.InScope}
                }
            };
            Program.DependencyInjectionServiceProvider = UiTestHelper.BuildContainerIoC(activeOrganisationInScope);

            // Act
            SearchRepository.LoadSearchDataIntoCache();

            // Assert
            AssertOrganisationIsIncludedInViewingService(activeOrganisationInScope);
        }

        [Test]
        public void LoadSearchDataIntoCache_Includes_ActiveOrganisationsPresumedInScope()
        {
            // Arrange
            var activeOrganisationPresumedInScope = new Organisation
            {
                OrganisationId = 1,
                OrganisationName = "Test organisation",
                SectorType = SectorTypes.Private,
                Status = OrganisationStatuses.Active,
                OrganisationScopes = new List<OrganisationScope>
                {
                    new OrganisationScope {Status = ScopeRowStatuses.Active, ScopeStatus = ScopeStatuses.PresumedInScope}
                }
            };
            Program.DependencyInjectionServiceProvider = UiTestHelper.BuildContainerIoC(activeOrganisationPresumedInScope);

            // Act
            SearchRepository.LoadSearchDataIntoCache();

            // Assert
            AssertOrganisationIsIncludedInViewingService(activeOrganisationPresumedInScope);
        }

        [Test]
        public void LoadSearchDataIntoCache_DoesNotInclude_ActiveOrganisationsOutOfScope()
        {
            // Arrange
            var activeOrganisationOutOfScope = new Organisation
            {
                OrganisationId = 1,
                OrganisationName = "Test organisation",
                SectorType = SectorTypes.Private,
                Status = OrganisationStatuses.Active,
                OrganisationScopes = new List<OrganisationScope>
                {
                    new OrganisationScope {Status = ScopeRowStatuses.Active, ScopeStatus = ScopeStatuses.OutOfScope}
                }
            };
            Program.DependencyInjectionServiceProvider = UiTestHelper.BuildContainerIoC(activeOrganisationOutOfScope);

            // Act
            SearchRepository.LoadSearchDataIntoCache();

            // Assert
            AssertOrganisationIsNotIncludedInViewingService(activeOrganisationOutOfScope);
        }

        [Test]
        public void LoadSearchDataIntoCache_DoesNotIncludes_ActiveOrganisationsPreviouslyInScope()
        {
            // Arrange
            var activeOrganisationPreviouslyInScope = new Organisation
            {
                OrganisationId = 1,
                OrganisationName = "Test organisation",
                SectorType = SectorTypes.Private,
                Status = OrganisationStatuses.Active,
                OrganisationScopes = new List<OrganisationScope>
                {
                    new OrganisationScope {Status = ScopeRowStatuses.Retired, ScopeStatus = ScopeStatuses.InScope}
                }
            };
            Program.DependencyInjectionServiceProvider = UiTestHelper.BuildContainerIoC(activeOrganisationPreviouslyInScope);

            // Act
            SearchRepository.LoadSearchDataIntoCache();

            // Assert
            AssertOrganisationIsNotIncludedInViewingService(activeOrganisationPreviouslyInScope);
        }

        [Test]
        public void LoadSearchDataIntoCache_DoesNotInclude_DeletedOrganisations()
        {
            // Arrange
            var deletedOrganisation = new Organisation
            {
                OrganisationId = 1,
                OrganisationName = "Test organisation",
                SectorType = SectorTypes.Private,
                Status = OrganisationStatuses.Deleted,
                Returns = new List<Return> {new Return {Status = ReturnStatuses.Submitted}}
            };
            Program.DependencyInjectionServiceProvider = UiTestHelper.BuildContainerIoC(deletedOrganisation);

            // Act
            SearchRepository.LoadSearchDataIntoCache();

            // Assert
            AssertOrganisationIsNotIncludedInViewingService(deletedOrganisation);
        }

        private void AssertOrganisationIsIncludedInViewingService(Organisation organisation)
        {
            Assert.IsTrue(
                SearchRepository.CachedOrganisations.Exists(
                    o => o.OrganisationId == organisation.OrganisationId && o.IncludeInViewingService));
        }

        private void AssertOrganisationIsNotIncludedInViewingService(Organisation organisation)
        {
            Assert.IsTrue(
                SearchRepository.CachedOrganisations.Exists(
                    o => o.OrganisationId == organisation.OrganisationId && !o.IncludeInViewingService));
        }

    }
}
