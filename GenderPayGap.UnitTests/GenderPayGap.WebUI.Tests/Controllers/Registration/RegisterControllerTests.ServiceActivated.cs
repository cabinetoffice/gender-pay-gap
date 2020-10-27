using System;
using System.Threading.Tasks;
using GenderPayGap.Core;
using GenderPayGap.Core.Classes;
using GenderPayGap.Database;
using GenderPayGap.Extensions;
using GenderPayGap.WebUI.Controllers;
using GenderPayGap.WebUI.Models.Register;
using GenderPayGap.WebUI.Tests.TestHelpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using NUnit.Framework;

namespace GenderPayGap.WebUI.Tests.Controllers.Registration
{
    [TestFixture]
    public partial class RegisterControllerTests
    {

        [Test]
        [Description("Check registration completes successfully when correct pin entered ")]
        public async Task RegisterController_ActivateService_POST_CorrectPIN_ServiceActivated()
        {
            //ARRANGE:
            //create a user who does exist in the db
            var user = new User {UserId = 1, EmailAddress = "test@hotmail.com", EmailVerifiedDate = VirtualDateTime.Now};
            var org = new Organisation {OrganisationId = 1, SectorType = SectorTypes.Private, Status = OrganisationStatuses.Pending};

            //TODO: Refactoring to user the same Helpers (ie AddScopeStatus.AddScopeStatus)
            org.OrganisationScopes.Add(
                new OrganisationScope {
                    Organisation = org,
                    ScopeStatus = ScopeStatuses.InScope,
                    SnapshotDate = org.SectorType.GetAccountingStartDate(VirtualDateTime.Now.Year),
                    Status = ScopeRowStatuses.Active
                });

            var address = new OrganisationAddress {AddressId = 1, OrganisationId = 1, Organisation = org, Status = AddressStatuses.Pending};
            var pin = "ASDFG";
            var userOrg = new UserOrganisation {
                UserId = 1,
                OrganisationId = 1,
                PINSentDate = VirtualDateTime.Now,
                PIN = pin,
                AddressId = address.AddressId,
                Address = address
            };

            var routeData = new RouteData();
            routeData.Values.Add("Action", nameof(RegisterController.ActivateService));
            routeData.Values.Add("Controller", "Register");

            var controller = UiTestHelper.GetController<RegisterController>(user.UserId, routeData, user, org, address, userOrg);
            controller.ReportingOrganisationId = org.OrganisationId;

            var model = new CompleteViewModel {PIN = pin};

            //ACT:
            var result = await controller.ActivateService(model) as RedirectToActionResult;

            //ASSERT:
            Assert.That(result != null, "Expected RedirectToActionResult");
            Assert.That(result.ActionName == "ServiceActivated", "Expected redirect to ServiceActivated");
            Assert.That(userOrg.PINConfirmedDate > DateTime.MinValue);
            Assert.That(userOrg.Organisation.Status == OrganisationStatuses.Active);
            Assert.That(userOrg.Organisation.GetLatestAddress().AddressId == address.AddressId);
            Assert.That(address.Status == AddressStatuses.Active);
            
        }
    }
}
