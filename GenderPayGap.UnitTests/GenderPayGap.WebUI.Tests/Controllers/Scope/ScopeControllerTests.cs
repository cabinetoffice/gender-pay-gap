using GenderPayGap.Core;
using GenderPayGap.Database;
using GenderPayGap.Extensions;
using GenderPayGap.WebUI.Controllers;
using GenderPayGap.WebUI.Models.ScopeNew;
using GenderPayGap.WebUI.Tests.Builders;
using GenderPayGap.WebUI.Tests.TestHelpers;
using NUnit.Framework;

namespace GenderPayGap.WebUI.Tests.Controllers.Scope
{
    [TestFixture]
    [SetCulture("en-GB")]
    public class ScopeControllerTests
    {

        [SetUp]
        public void BeforeEach()
        {
            UiTestHelper.SetDefaultEncryptionKeys();
        }

        [Test]
        [Description("POST: Existing scopes are retired when a new scope is added")]
        public void POST_Existing_Scopes_Are_Retired_When_New_Scope_Is_Added()
        {
            // Arrange
            Organisation organisation = new OrganisationBuilder().WithSectorType(SectorTypes.Private).Build();
            
            User user = new UserBuilder().WithOrganisation(organisation).Build();

            OrganisationScope organisationScope2018 = new OrganisationScopeBuilder()
                .WithOrganisation(organisation)
                .WithReportingYear(2018)
                .Build();
            
            OrganisationScope organisationScope2017 = new OrganisationScopeBuilder()
                .WithOrganisation(organisation)
                .WithReportingYear(2017)
                .Build();

            var controller = new ControllerBuilder<ScopeController>().WithLoggedInUser(user)
                .WithDatabaseObjects(user)
                .WithDatabaseObjects(organisation)
                .WithDatabaseObjects(organisationScope2017, organisationScope2018)
                .Build();

            // Act
            string encryptedOrganisationId = Encryption.EncryptId(organisation.OrganisationId);
            controller.ConfirmOutOfScopeAnswers(encryptedOrganisationId, 2018, new ScopeViewModel());
            
            // Assert
            // Old scopes from the same year should be retired
            Assert.AreEqual(organisationScope2018.Status, ScopeRowStatuses.Retired);
            
            // Scopes from a different year should not be retired
            Assert.AreEqual(organisationScope2017.Status, ScopeRowStatuses.Active);
        }

    }
}
