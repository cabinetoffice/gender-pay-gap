using System;
using System.Collections.Generic;
using System.Linq;
using GenderPayGap.Core;
using GenderPayGap.Core.Classes;
using GenderPayGap.Core.Interfaces;
using GenderPayGap.Database;
using GenderPayGap.Extensions;
using GenderPayGap.WebUI.Helpers;
using GenderPayGap.WebUI.Models.Admin;
using GenderPayGap.WebUI.Models.AdminReferenceData;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GenderPayGap.WebUI.Controllers.Admin
{
    [Authorize(Roles = LoginRoles.GpgAdmin)]
    [Route("admin")]
    public class AdminReAddRetiredSubmissionsController : Controller
    {

        private readonly IDataRepository dataRepository;

        public AdminReAddRetiredSubmissionsController(IDataRepository dataRepository)
        {
            this.dataRepository = dataRepository;
        }

        [HttpGet("re-add-retired-submissions")]
        public IActionResult ReAddRetiredSubmissionsGet()
        {
            var viewModel = new AdminReAddRetiredSubmissionsViewModel();
            return View("ReAddRetiredSubmissions", viewModel);
        }

        [HttpPost("re-add-retired-submissions")]
        [ValidateAntiForgeryToken]
        public IActionResult ReAddRetiredSubmissionsPost(AdminReAddRetiredSubmissionsViewModel viewModel)
        {
            var expectedHeadings = new List<string>
            {
                "ReturnId",
                "OrganisationId",
                "AccountingDate",
                "DiffMeanHourlyPayPercent",
                "DiffMedianHourlyPercent",
                "DiffMeanBonusPercent",
                "DiffMedianBonusPercent",
                "MaleMedianBonusPayPercent",
                "FemaleMedianBonusPayPercent",
                "MaleLowerPayBand",
                "FemaleLowerPayBand",
                "MaleMiddlePayBand",
                "FemaleMiddlePayBand",
                "MaleUpperPayBand",
                "FemaleUpperPayBand",
                "MaleUpperQuartilePayBand",
                "FemaleUpperQuartilePayBand",
                "CompanyLinkToGPGInfo",
                "Created",
                "Modified",
                "JobTitle",
                "FirstName",
                "LastName",
                "MinEmployees",
                "MaxEmployees",
                "IsLateSubmission",
                "EHRCResponse"
            };

            if (!ReferenceDataHelper.TryParseCsvFileWithHeadings(
                viewModel.File,
                expectedHeadings,
                out List<Return> returns,
                out string errorMessage))
            {
                return Json("Error: " + errorMessage);
            }

            User user = ControllerHelper.GetGpgUserFromAspNetUser(User, dataRepository);

            var status = ReturnStatuses.Retired;
            DateTime statusDate = VirtualDateTime.Now;
            string statusDetails = $"Re-added retired submission ({statusDate.ToString("yyyy-MM-dd HH:mm:ss")})";
            string modifications = "ReAddedRetiredSubmissions";

            List<long> organisationIds = returns
                .Select(ret => ret.OrganisationId)
                .ToList();

            List<Organisation> organisations =
                dataRepository.GetAll<Organisation>()
                    .Where(o => organisationIds.Contains(o.OrganisationId))
                    .ToList();

            foreach (Return ret in returns)
            {
                if (dataRepository.Get<Return>(ret.ReturnId) != null)
                {
                    return Json($"Error: Return ID {ret.ReturnId} already exists");
                }

                Organisation organisation = organisations.FirstOrDefault(o => o.OrganisationId == ret.OrganisationId);
                if (organisation == null)
                {
                    return Json($"Error: Organisation ID {ret.OrganisationId} does not exist");
                }
                ret.Organisation = organisation;

                // Fix accounting date (might be wrong sector)
                int reportingYear = ret.AccountingDate.Year;
                ret.AccountingDate = ret.Organisation.SectorType.GetAccountingStartDate(reportingYear);

                ret.Status = status;
                ret.StatusDate = statusDate;
                ret.StatusDetails = statusDetails;
                ret.Modifications = modifications;

                var returnStatus = new ReturnStatus
                {
                    Return = ret,
                    ReturnId = ret.ReturnId,
                    ByUser = user,
                    ByUserId = user.UserId,
                    Status = status,
                    StatusDate = statusDate,
                    StatusDetails = statusDetails
                };
                ret.ReturnStatuses.Add(returnStatus);

                dataRepository.Insert(ret);
                dataRepository.Insert(returnStatus);
            }

            dataRepository.SaveChanges();

            return Json("Succeeded!");
        }

    }
}
