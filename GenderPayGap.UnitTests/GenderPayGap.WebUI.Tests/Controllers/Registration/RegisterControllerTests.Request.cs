using System;
using System.Threading.Tasks;
using GenderPayGap.Core;
using GenderPayGap.Core.Classes;
using GenderPayGap.Database;
using GenderPayGap.Extensions;
using GenderPayGap.WebUI.Classes;
using GenderPayGap.WebUI.Controllers;
using GenderPayGap.WebUI.Models.Register;
using GenderPayGap.WebUI.Tests.TestHelpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using NUnit.Framework;

namespace GenderPayGap.WebUI.Tests.Controllers.Registration
{
    public partial class RegisterControllerTests
    {

        [Test]
        [Description("Check manual registration completes successfully on admin approval")]
        public async Task RegisterController_ReviewRequest_POST_ManualRegistration_ServiceActivated()
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
            var userOrg = new UserOrganisation {
                UserId = 1,
                OrganisationId = 1,
                AddressId = address.AddressId,
                Address = address,
                User = user,
                Organisation = org
            };

            var routeData = new RouteData();
            routeData.Values.Add("Action", "OrganisationType");
            routeData.Values.Add("Controller", "Register");

            var controller = UiTestHelper.GetController<RegisterController>(user.UserId, routeData, user, org, address, userOrg);

            var model = new OrganisationViewModel {ReviewCode = userOrg.GetReviewCode()};
            controller.StashModel(model);

            //ACT:
            var result = await controller.ReviewRequest(model, "approve") as RedirectToActionResult;

            //ASSERT:
            Assert.That(result != null, "Expected RedirectToActionResult");
            Assert.That(result.ActionName == "RequestAccepted", "Expected redirect to RequestAccepted");
            Assert.That(userOrg.PINConfirmedDate > DateTime.MinValue);
            Assert.That(userOrg.Organisation.Status == OrganisationStatuses.Active);
            Assert.That(userOrg.Organisation.GetLatestAddress().AddressId == address.AddressId);
            Assert.That(address.Status == AddressStatuses.Active);
            
        }

    }
}
