using System;
using System.Collections.Generic;
using System.Linq;
using GenderPayGap.BusinessLogic.Classes;
using GenderPayGap.BusinessLogic.Models.Submit;
using GenderPayGap.BusinessLogic.Services;
using GenderPayGap.Core.Interfaces;
using GenderPayGap.Database;
using GenderPayGap.WebUI.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace GenderPayGap.WebUI.Controllers.Admin
{
    [Authorize(Roles = LoginRoles.GpgAdmin)]
    [Route("admin")]
    public class AdminMigrateDraftReturnsToDatabaseController : Controller
    {

        private IDataRepository dataRepository;
        private IFileRepository fileRepository;
        private IDraftFileBusinessLogic draftFileBusinessLogic;

        public AdminMigrateDraftReturnsToDatabaseController(IDataRepository dataRepository, IFileRepository fileRepository, IDraftFileBusinessLogic draftFileBusinessLogic)
        {
            this.dataRepository = dataRepository;
            this.fileRepository = fileRepository;
            this.draftFileBusinessLogic = draftFileBusinessLogic;
        }

        [HttpGet("migrate-draft-returns-to-database")]
        public IActionResult MigrateDraftReturnFilesToDatabase(int organisationId)
        {
            DateTime endTime = DateTime.Now.AddSeconds(10);

            List<long> nextOrganisations = dataRepository.GetAll<Organisation>()
                .Where(o => o.OrganisationId > organisationId)
                .OrderBy(o => o.OrganisationId)
                .Select(o => o.OrganisationId)
                .ToList();

            List<int> reportingYears = ReportingYearsHelper.GetReportingYears();

            long latestOrganisationId = organisationId;
            foreach (long currentOrganisationId in nextOrganisations)
            {
                foreach (int reportingYear in reportingYears)
                {
                    string filePath = $"App_Data\\draftReturns\\{currentOrganisationId}_{reportingYear}.json";

                    if (fileRepository.GetFileExists(filePath))
                    {
                        string fileContents = fileRepository.Read(filePath);

                        if (!string.IsNullOrWhiteSpace(fileContents))
                        {
                            ReturnViewModel returnViewModel = JsonConvert.DeserializeObject<ReturnViewModel>(fileContents);

                            Draft draft = new Draft(currentOrganisationId, reportingYear);
                            draftFileBusinessLogic.UpdateAndCommit(returnViewModel, draft, 0);
                        }
                    }
                }

                latestOrganisationId = currentOrganisationId;
                if (DateTime.Now > endTime)
                {
                    break;
                }
            }

            return Json(new {LatestOrganisationId = latestOrganisationId});
        }

    }
}
