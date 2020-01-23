using System;
using System.Threading.Tasks;
using GenderPayGap.BusinessLogic.Models.Submit;
using GenderPayGap.BusinessLogic.Services;
using GenderPayGap.Core;
using GenderPayGap.Core.Classes;
using GenderPayGap.Database;
using GenderPayGap.Extensions;
using GenderPayGap.WebUI.Classes;
using GenderPayGap.WebUI.Classes.Services;
using GenderPayGap.WebUI.Controllers.Submission;
using GenderPayGap.WebUI.Tests.TestHelpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using NUnit.Framework;

namespace GenderPayGap.WebUI.Tests.Controllers
{

    public partial class SubmitControllerTests
    {

        [Test]
        [Description("DraftComplete should fail when male bonus is zero but provided mean or median bonus difference")]
        public async Task SubmitController_DraftComplete_POST_MaleBonusIsZero_ShowsErrors()
        {
            // Arrange
            var user = new User {UserId = 1, EmailAddress = "magnuski@hotmail.com", EmailVerifiedDate = VirtualDateTime.Now};
            var organisation = new Organisation {OrganisationId = 1, SectorType = SectorTypes.Public};
            var userOrganisation = new UserOrganisation {
                OrganisationId = organisation.OrganisationId,
                Organisation = organisation,
                UserId = 1,
                PINConfirmedDate = VirtualDateTime.Now,
                PIN = "1"
            };

            //set mock routeData
            var routeData = new RouteData();
            routeData.Values.Add("action", "EnterCalculations");
            routeData.Values.Add("controller", "submit");

            DateTime PrivateAccountingDate = Global.PrivateAccountingDate;

            var returnViewModel = new ReturnViewModel {
                AccountingDate = PrivateAccountingDate,
                MaleMedianBonusPayPercent = 0,
                FemaleMedianBonusPayPercent = 50,
                DiffMeanBonusPercent = -50,
                DiffMedianBonusPercent = -50,
                DiffMedianHourlyPercent = 50,
                DiffMeanHourlyPayPercent = 50,
                FemaleLowerPayBand = 50,
                FemaleMiddlePayBand = 50,
                FemaleUpperPayBand = 50,
                FemaleUpperQuartilePayBand = 50,
                MaleLowerPayBand = 50,
                MaleMiddlePayBand = 50,
                MaleUpperPayBand = 50,
                MaleUpperQuartilePayBand = 50,
                SectorType = SectorTypes.Private
            };

            var controller = UiTestHelper.GetController<SubmitController>(1, routeData, user, organisation, userOrganisation);
            controller.Bind(returnViewModel);

            var submissionServiceMock = new SubmissionService(
                null,
                null,
                null,
                new DraftFileBusinessLogic(new SystemFileRepository()),
                null);
            returnViewModel.ReportInfo.Draft = await submissionServiceMock.GetDraftFileAsync(
                organisation.OrganisationId,
                organisation.SectorType.GetAccountingStartDate().Year,
                user.UserId);

            // Act
            controller.StashModel(returnViewModel);
            controller.ReportingOrganisationId = 1;
            var result = await controller.SaveDraftAsync(returnViewModel) as ViewResult;

            // Assert
            Assert.NotNull(result, "Expected ViewResult");
            Assert.Null(result.ViewName, "Incorrect view returned");
            Assert.NotNull(result.Model as ReturnViewModel, "Expected ReturnViewModel");
            Assert.AreEqual(
                "Do not enter a bonus difference if 0% of your male employees received a bonus",
                result.ViewData.ModelState["DiffMedianBonusPercent"].Errors[0].ErrorMessage);
            Assert.AreEqual(
                "Do not enter a bonus difference if 0% of your male employees received a bonus",
                result.ViewData.ModelState["DiffMeanBonusPercent"].Errors[0].ErrorMessage);
        }

    }

}
