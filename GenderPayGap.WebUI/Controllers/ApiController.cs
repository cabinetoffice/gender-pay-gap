using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using GenderPayGap.Core;
using GenderPayGap.Core.Interfaces;
using GenderPayGap.Database;
using GenderPayGap.Extensions;
using GenderPayGap.WebUI.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GenderPayGap.WebUI.Controllers
{
    [Route("api")]
    public class ApiController : Controller
    {

        private readonly IDataRepository dataRepository;

        public ApiController(IDataRepository dataRepository)
        {
            this.dataRepository = dataRepository;
        }


        [HttpGet("scopes-and-returns")]
        public IActionResult ScopesAndReturns(string password)
        {
            string expectedPassword = Global.TomsAppApiPassword;
            if (string.IsNullOrWhiteSpace(expectedPassword))
            {
                throw new ArgumentException("TomsAppApiPassword must be set to enable the API");
            }
            if (password != expectedPassword)
            {
                return Unauthorized();
            }

            List<Organisation> organisations = dataRepository
                .GetAll<Organisation>()
                .Include(o => o.OrganisationScopes)
                .Include(o => o.Returns)
                .Include(o => o.OrganisationSicCodes)
                .Include(o => o.LatestPublicSectorType)
                .ToList();

            List<int> reportingYears = ReportingYearsHelper.GetReportingYears();

            var records = new List<object>();
            foreach (Organisation organisation in organisations)
            {
                foreach (int reportingYear in reportingYears)
                {
                    OrganisationScope scopeForYear = organisation.GetLatestScopeForSnapshotYear(reportingYear);
                    
                    if (scopeForYear != null)
                    {
                        Return returnForYear = organisation.GetReturn(reportingYear);

                        var record = new
                        {
                            organisationid = organisation.OrganisationId,
                            organisationname = organisation.OrganisationName,
                            employerreference = organisation.EmployerReference,
                            companynumber = organisation.CompanyNumber,
                            organisationstatus = $"{organisation.Status.ToString()} ({(int)organisation.Status})",
                            sectortype = $"{organisation.SectorType.ToString()} ({(int)organisation.SectorType})",
                            scopestatus = $"{ScopeStatusToString(scopeForYear.ScopeStatus)} ({(int)scopeForYear.ScopeStatus})",
                            snapshotdate = reportingYear,
                            siccodesectiondescription = organisation.OrganisationSicCodes.FirstOrDefault()?.SicCode?.SicSection?.Description,
                            returnid = returnForYear?.ReturnId,
                            publicsectordescription = organisation.LatestPublicSectorType?.PublicSectorType?.Description,
                            organisationsize = returnForYear?.OrganisationSize.GetAttribute<DisplayAttribute>().Name
                        };

                        records.Add(record);
                    }
                }
            }
            
            return Json(records);
        }

        private static string ScopeStatusToString(ScopeStatuses scope)
        {
            switch (scope)
            {
                case ScopeStatuses.InScope:
                    return "In scope";
                case ScopeStatuses.OutOfScope:
                    return "Out of scope";
                case ScopeStatuses.PresumedInScope:
                    return "Presumed in scope";
                case ScopeStatuses.PresumedOutOfScope:
                    return "Presumed out of scope";
                case ScopeStatuses.Unknown:
                default:
                    return "Unknown";
            }
        }


        [HttpGet("submissions")]
        public IActionResult Submissions(string password)
        {
            string expectedPassword = Global.TomsAppApiPassword;
            if (string.IsNullOrWhiteSpace(expectedPassword))
            {
                throw new ArgumentException("TomsAppApiPassword must be set to enable the API");
            }
            if (password != expectedPassword)
            {
                return Unauthorized();
            }

            List<Return> returns = dataRepository
                .GetAll<Return>()
                .Include(r => r.ReturnStatuses)
                .Include(r => r.Organisation)
                .ThenInclude(o => o.Returns)
                .ToList();

            var records = new List<object>();

            foreach (Return ret in returns)
            {
                Return firstReturnThisYear = ret.Organisation.Returns
                    .Where(r => r.AccountingDate == ret.AccountingDate)
                    .OrderBy(r => r.Modified)
                    .First();

                foreach (ReturnStatus returnStatus in ret.ReturnStatuses)
                {
                    var record = new
                    {
                        organisationid = ret.OrganisationId,
                        latestreturnaccountingdate = ret.AccountingDate,
                        statusid = $"{returnStatus.Status.ToString()} ({(int)returnStatus.Status})",
                        statusdate = returnStatus.StatusDate,
                        statusdetails = returnStatus.StatusDetails,
                        latestreturnstatus = $"{ret.Status.ToString()} ({(int)ret.Status})",
                        latestreturnstatusdate = ret.StatusDate,
                        datefirstreportedinyear = firstReturnThisYear.Modified,
                        submittedby = $"{ret.FirstName} {ret.LastName} [{ret.JobTitle}]",
                        latestreturnlatereason = ret.LateReason,
                        returnmodifiedfields = ret.Modifications,
                        ehrcresponse = ret.EHRCResponse,
                        diffmeanbonuspercent = ret.DiffMeanBonusPercent,
                        diffmeanhourlypaypercent = ret.DiffMeanHourlyPayPercent,
                        diffmedianbonuspercent = ret.DiffMedianBonusPercent,
                        diffmedianhourlypercent = ret.DiffMedianHourlyPercent,
                        femalelowerpayband = ret.FemaleLowerPayBand,
                        femalemedianbonuspaypercent = ret.FemaleMedianBonusPayPercent,
                        femalemiddlepayband = ret.FemaleMiddlePayBand,
                        femaleupperpayband = ret.FemaleUpperPayBand,
                        femaleupperquartilepayband = ret.FemaleUpperQuartilePayBand,
                        malelowerpayband = ret.MaleLowerPayBand,
                        malemedianbonuspaypercent = ret.MaleMedianBonusPayPercent,
                        malemiddlepayband = ret.MaleMiddlePayBand,
                        maleupperpayband = ret.MaleUpperPayBand,
                        maleupperquartilepayband = ret.MaleUpperQuartilePayBand
                    };

                    records.Add(record);
                }
            }

            return Json(records);
        }

    }
}
