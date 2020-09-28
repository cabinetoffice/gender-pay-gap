using System;
using System.Collections.Generic;
using GenderPayGap.Core;
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
            var testViewModel = new AdminChangeScopeViewModel
            {
                Reason = "A reason",
                CurrentScopeStatus = ScopeStatuses.InScope,
                NewScopeStatus = NewScopeStatus.OutOfScope,
                OrganisationId = 12345,
                OrganisationName = "Something Ltd",
                ReportingYear = 2018
            };

            var controller = NewUiTestHelper.GetController<WebUI.Controllers.AdminOrganisationScopeController>();

            // Act
            var response = (RedirectToActionResult) controller.ChangeScopePost(testViewModel.OrganisationId, testViewModel.ReportingYear, testViewModel);
            
            // Assert
            // TODO: Check that old scopes for this reporting year are retired
        }

    }
}
