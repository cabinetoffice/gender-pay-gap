using System;
using System.Collections.Generic;
using GenderPayGap.Core;
using GenderPayGap.Core.Classes;
using GenderPayGap.Database;
using GenderPayGap.Extensions;
using GenderPayGap.WebUI.Models.Admin;
using GenderPayGap.WebUI.Tests.Builders;
using GenderPayGap.WebUI.Tests.TestHelpers;
using Microsoft.AspNetCore.Mvc;
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
            var user = new UserBuilder().Build();

            var organisation = CreateDefaultOrganisation(SectorTypes.Private);

            var organisationScope2018 = CreateDefaultOrganisationScope(organisation, ScopeStatuses.PresumedInScope, 2018);

            var organisationScope2017 = CreateDefaultOrganisationScope(organisation, ScopeStatuses.PresumedInScope, 2017);
            
            var requestFormValues = new Dictionary<string, StringValues>();
            requestFormValues.Add("GovUk_Radio_NewScopeStatus", "OutOfScope");
            requestFormValues.Add("GovUk_Text_Reason", "A reason");

            object[] dbObjects = {user, organisation, organisationScope2017, organisationScope2018};

            var controller = NewUiTestHelper.GetController<WebUI.Controllers.AdminOrganisationScopeController>(requestFormValues: requestFormValues, dbObjects: dbObjects);

            // Act
            controller.ChangeScopePost(1, 2018, new AdminChangeScopeViewModel());
            
            // Assert
            // Old scopes from the same year should be retired
            Assert.AreEqual(organisationScope2018.Status, ScopeRowStatuses.Retired);
            
            // Scopes from a different year should not be retired
            Assert.AreEqual(organisationScope2017.Status, ScopeRowStatuses.Active);
        }

        private User CreateDefaultUser()
        {
            return new User
            {
                UserId = 1,
                EmailAddress = "test@example.com",
                Firstname = "Test",
                Lastname = "Example",
                Status = UserStatuses.Active
            };
        }

        private Organisation CreateDefaultOrganisation(SectorTypes sector)
        {
            return new Organisation
            {
                OrganisationId = 1, OrganisationName = "Test Organisation Ltd", CompanyNumber = "12345678", Created = DateTime.Now, SectorType = sector
            };
        }

        private OrganisationScope CreateDefaultOrganisationScope(Organisation organisation, ScopeStatuses scopeStatus, int reportingYear)
        {
            return new OrganisationScope
            {
                Organisation = organisation,
                OrganisationId = organisation.OrganisationId,
                ReadGuidance = true,
                ScopeStatus = scopeStatus,
                Reason = "Initial setup",
                Status = ScopeRowStatuses.Active,
                SnapshotDate = SectorTypes.Private.GetAccountingStartDate(reportingYear)
            };
        }

    }
}
