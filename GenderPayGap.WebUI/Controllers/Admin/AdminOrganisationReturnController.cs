using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using GenderPayGap.BusinessLogic.Services;
using GenderPayGap.Core;
using GenderPayGap.Core.Interfaces;
using GenderPayGap.Database;
using GenderPayGap.Extensions;
using GenderPayGap.WebUI.Classes;
using GenderPayGap.WebUI.Helpers;
using GenderPayGap.WebUI.Models.Admin;
using GenderPayGap.WebUI.Services;
using GovUkDesignSystem.Parsers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GenderPayGap.WebUI.Controllers.Admin
{
    [Authorize(Roles = "GPGadmin")]
    [Route("admin")]
    public class AdminOrganisationReturnController : Controller
    {

        private readonly AuditLogger auditLogger;

        private readonly IDataRepository dataRepository;

        public AdminOrganisationReturnController(IDataRepository dataRepository, AuditLogger auditLogger)
        {
            this.dataRepository = dataRepository;
            this.auditLogger = auditLogger;
        }

        [HttpGet("organisation/{id}/returns")]
        public IActionResult ViewReturns(long id)
        {
            Organisation organisation = dataRepository.Get<Organisation>(id);

            return View("ViewReturns", organisation);
        }

        [HttpGet("organisation/{id}/returns/{year}")]
        public IActionResult ViewReturnDetailsForYear(long id, int year)
        {
            Organisation organisation = dataRepository.Get<Organisation>(id);

            var viewModel = new AdminOrganisationReturnDetailsViewModel
            {
                Organisation = organisation,
                Year = year
            };

            return View("ViewReturnDetails", viewModel);
        }

        [HttpGet("organisation/{id}/returns/download-details-csv")]
        public IActionResult DownloadReturnDetailsCsv(long id)
        {
            Organisation organisation = dataRepository.Get<Organisation>(id);

            var returns = organisation.Returns.OrderByDescending(r => r.AccountingDate).ThenByDescending(r => r.StatusDate);

            var records = returns.Select(
                ret =>
                    new
                    {
                        OrganisationId = ret.Organisation.OrganisationId,
                        OrganisationName = ret.Organisation.OrganisationName,
                        ReturnId = ret.ReturnId,

                        SnapshotDate = ret.AccountingDate,
                        DeadlineDate = ret.AccountingDate.AddYears(1).AddDays(-1),
                        ModifiedDate = ret.Modified,

                        Status = ret.Status,
                        Modifications = ret.Modifications,
                        Late = ret.IsLateSubmission,
                        LateReason = ret.LateReason,

                        Employees = ret.OrganisationSize.GetAttribute<DisplayAttribute>().Name,

                        HourlyPayGapMean = ret.DiffMeanHourlyPayPercent,
                        HourlyPayGapMedian = ret.DiffMedianHourlyPercent,

                        BonusPayPercentMale = ret.MaleMedianBonusPayPercent,
                        BonusPayPercentFemale = ret.FemaleMedianBonusPayPercent,
                        BonusPayGapMean = ret.DiffMeanBonusPercent,
                        BonusPayGapMedian = ret.DiffMedianBonusPercent,

                        UpperQuarterPercentMale = ret.MaleUpperQuartilePayBand,
                        UpperQuarterPercentFemale = ret.FemaleUpperQuartilePayBand,
                        UpperMiddleQuarterPercentMale = ret.MaleUpperPayBand,
                        UpperMiddleQuarterPercentFemale = ret.FemaleUpperPayBand,
                        LowerMiddleQuarterPercentMale = ret.MaleMiddlePayBand,
                        LowerMiddleQuarterPercentFemale = ret.FemaleMiddlePayBand,
                        LowerQuarterPercentMale = ret.MaleLowerPayBand,
                        LowerQuarterPercentFemale = ret.FemaleLowerPayBand,

                        LinkToCompanyWebsite = ret.CompanyLinkToGPGInfo,
                        SeniorResponsibleOfficer = ret.ResponsiblePerson
                    });

            string sanitisedOrganisationName = SanitiseOrganisationNameForFilename(organisation.OrganisationName);
            string fileDownloadName = $"ReturnsForOrganisation-{sanitisedOrganisationName}--{VirtualDateTime.Now:yyyy-MM-dd HH:mm}.csv";
            FileContentResult fileContentResult = CsvDownloadHelper.CreateCsvDownload(records, fileDownloadName);

            return fileContentResult;
        }

        private static string SanitiseOrganisationNameForFilename(string organisationName)
        {
            // Remove characters from organisation name that would be invalid in a file name
            foreach (char invalidFileNameChar in Path.GetInvalidFileNameChars())
            {
                organisationName = organisationName.Replace(invalidFileNameChar.ToString(), "");
            }

            return organisationName;
        }

        [HttpGet("organisation/{id}/returns/{year}/delete")]
        public IActionResult DeleteReturnsOfAYearGet(long id, int year)
        {
            var organisation = dataRepository.Get<Organisation>(id);
            List<long> returnsId = organisation.Returns.Where(r => r.AccountingDate.Year == year).Select(r => r.ReturnId).ToList();

            var viewModel = new AdminDeleteReturnViewModel {Organisation = organisation, ReturnIds = returnsId, Year = year};

            return View("DeleteReturns", viewModel);
        }

        [HttpGet("organisation/{id}/returns/{year}/delete/{returnId}")]
        public IActionResult DeleteReturnGet(long id, int year, long returnId)
        {
            var organisation = dataRepository.Get<Organisation>(id);

            if (!organisation.Returns.Select(r => r.ReturnId).Contains(returnId))
            {
                throw new Exception("The ReturnID that the user requested to be deleted does not belong to this Organisation");
            }
            var returnIds = new List<long>{ returnId };

            var viewModel = new AdminDeleteReturnViewModel {Organisation = organisation, ReturnIds = returnIds, Year = year};

            return View("DeleteReturns", viewModel);
        }

        [HttpPost("organisation/{id}/returns/{year}/delete")]
        [PreventDuplicatePost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteReturnsPost(long id, int year, AdminDeleteReturnViewModel viewModel)
        {
            var organisation = dataRepository.Get<Organisation>(id);
            User currentUser = ControllerHelper.GetGpgUserFromAspNetUser(User, dataRepository);

            viewModel.ParseAndValidateParameters(Request, m => m.Reason);
            if (viewModel.HasAnyErrors())
            {
                viewModel.Organisation = organisation;
                viewModel.Year = year;
                return View("DeleteReturns", viewModel);
            }

            foreach (long returnId in viewModel.ReturnIds)
            {
                if (!organisation.Returns.Select(r => r.ReturnId).Contains(returnId))
                {
                    throw new Exception("The ReturnID that the user requested to be deleted does not belong to this Organisation");
                }
            }
            foreach (long returnId in viewModel.ReturnIds)
            {
                dataRepository.Get<Return>(returnId).SetStatus(ReturnStatuses.Deleted, currentUser.UserId, viewModel.Reason);
            }

            // Audit log
            auditLogger.AuditChangeToOrganisation(
                AuditedAction.AdminDeleteReturn,
                organisation,
                new
                {
                    ReportingYear= year,
                    ReturnIds = string.Join(", ", viewModel.ReturnIds),
                    Reason = viewModel.Reason
                },
                User);

            return RedirectToAction("ViewReturns", "AdminOrganisationReturn", new {id});
        }

    }
}
