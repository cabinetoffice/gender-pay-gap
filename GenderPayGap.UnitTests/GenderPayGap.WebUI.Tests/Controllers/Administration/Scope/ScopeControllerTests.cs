using System;
using System.Collections.Generic;
using GenderPayGap.Core;
using GenderPayGap.Core.Classes;
using GenderPayGap.Database;
using GenderPayGap.Extensions;
using GenderPayGap.WebUI.Models.Admin;
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
            var user = new User
            {
                UserId = 1,
                EmailAddress = "test@example.com",
                Firstname = "Test",
                Lastname = "Example",
                EmailVerifySendDate = VirtualDateTime.Now,
                EmailVerifyHash = Guid.NewGuid().ToString("N"),
                Status = UserStatuses.Active
            };

            var organisation = new Organisation
            {
                OrganisationId = 1, OrganisationName = "Test Organisation Ltd", CompanyNumber = "12345678", Created = DateTime.Now
            };

            var organisationScope2018 = new OrganisationScope
            {
                Organisation = organisation,
                OrganisationId = organisation.OrganisationId,
                ReadGuidance = true,
                ScopeStatus = ScopeStatuses.PresumedInScope,
                Reason = "Initial setup",
                Status = ScopeRowStatuses.Active,
                SnapshotDate = SectorTypes.Private.GetAccountingStartDate(2018)
            };
            
            var organisationScope2017 = new OrganisationScope
            {
                Organisation = organisation,
                OrganisationId = organisation.OrganisationId,
                ReadGuidance = true,
                ScopeStatus = ScopeStatuses.PresumedInScope,
                Reason = "Initial setup",
                Status = ScopeRowStatuses.Active,
                SnapshotDate = SectorTypes.Private.GetAccountingStartDate(2017)
            };
            
            var requestFormValues = new Dictionary<string, StringValues>();
            requestFormValues.Add("GovUk_Radio_NewScopeStatus_OutOfScope", "true");
            requestFormValues.Add("GovUk_Text_Reason", "A reason");

            var testViewModel = new AdminChangeScopeViewModel
            {
                Reason = "A reason",
                CurrentScopeStatus = ScopeStatuses.InScope,
                NewScopeStatus = NewScopeStatus.OutOfScope,
                OrganisationId = 12345,
                OrganisationName = "Something Ltd",
                ReportingYear = 2018
            };

            object[] dbObjects = {user, organisation, organisationScope2017, organisationScope2018};

            var controller = NewUiTestHelper.GetController<WebUI.Controllers.AdminOrganisationScopeController>(dbObjects: dbObjects);

            // Act
            controller.ChangeScopePost(testViewModel.OrganisationId, testViewModel.ReportingYear, testViewModel);
            
            // Assert
            // Old scopes from the same year should be retired
            Assert.AreEqual(organisationScope2018.Status, ScopeRowStatuses.Retired);
            
            // Scopes from a different year should not be retired
            Assert.AreEqual(organisationScope2017.Status, ScopeRowStatuses.Active);
        }

    }
}
