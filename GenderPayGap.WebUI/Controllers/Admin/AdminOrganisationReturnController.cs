﻿using System.ComponentModel.DataAnnotations;
using System.Linq;
using GenderPayGap.Core.Interfaces;
using GenderPayGap.Database;
using GenderPayGap.Extensions;
using GenderPayGap.WebUI.Classes;
using GenderPayGap.WebUI.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GenderPayGap.WebUI.Controllers.Admin
{
    [Authorize(Roles = "GPGadmin")]
    [Route("admin")]
    public class AdminOrganisationReturnController : Controller
    {

        private readonly IDataRepository dataRepository;

        public AdminOrganisationReturnController(IDataRepository dataRepository)
        {
            this.dataRepository = dataRepository;
        }

        [HttpGet("organisation/{id}/returns")]
        public IActionResult ViewReturns(long id)
        {
            Organisation organisation = dataRepository.Get<Organisation>(id);

            return View("ViewReturns", organisation);
        }

        [HttpGet("organisation/{id}/returns/details")]
        public IActionResult ViewReturnDetails(long id)
        {
            Organisation organisation = dataRepository.Get<Organisation>(id);

            return View("ViewReturnDetails", organisation);
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

            string fileDownloadName = $"ReturnsForOrganisation-{organisation.OrganisationId}--{VirtualDateTime.Now:yyyy-MM-dd HH:mm}.csv";
            FileContentResult fileContentResult = CsvDownloadHelper.CreateCsvDownload(records, fileDownloadName);

            return fileContentResult;
        }


    }
}
