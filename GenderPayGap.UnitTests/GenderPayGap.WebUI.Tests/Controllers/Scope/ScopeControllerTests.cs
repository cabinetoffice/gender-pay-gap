using System.Collections.Generic;
using GenderPayGap.Core;
using GenderPayGap.Database;
using GenderPayGap.Extensions;
using GenderPayGap.WebUI.Controllers;
using GenderPayGap.WebUI.Models.ScopeNew;
using GenderPayGap.WebUI.Tests.Builders;
using Microsoft.Extensions.Primitives;
using NUnit.Framework;

namespace GenderPayGap.WebUI.Tests.Controllers.Scope
{
    [TestFixture]
    [SetCulture("en-GB")]
    public class ScopeControllerTests
    {

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

            var requestFormValues = new Dictionary<string, StringValues>();
            requestFormValues.Add("GovUk_Radio_NewScopeStatus", "OutOfScope");
            requestFormValues.Add("GovUk_Text_Reason", "A reason");

            var controller = new ControllerBuilder<ScopeController>().WithLoggedInUser(user)
                .WithRequestFormValues(requestFormValues)
                .WithDatabaseObjects(user, organisation, organisationScope2017, organisationScope2018)
                .Build();

            // Act
            string encryptedOrganisationId = Encryption.EncryptQuerystring(organisation.OrganisationId.ToString());
            controller.ConfirmOutOfScopeAnswers(encryptedOrganisationId, 2018, new OutOfScopeViewModel());
            
            // Assert
            // Old scopes from the same year should be retired
            Assert.AreEqual(organisationScope2018.Status, ScopeRowStatuses.Retired);
            
            // Scopes from a different year should not be retired
            Assert.AreEqual(organisationScope2017.Status, ScopeRowStatuses.Active);
        }

    }
}
