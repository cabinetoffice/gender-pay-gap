using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GenderPayGap.BusinessLogic.Services;
using GenderPayGap.Core;
using GenderPayGap.Core.Interfaces;
using GenderPayGap.Database;
using GenderPayGap.Extensions;
using GenderPayGap.Extensions.AspNetCore;
using GenderPayGap.WebUI.Classes;
using Microsoft.AspNetCore.Mvc;

namespace GenderPayGap.WebUI.Controllers
{
    [Route("dummy")]
    public class DummyController : Controller
    {

        private readonly AuditLogger auditLogger;
        private readonly IDataRepository dataRepository;
        private readonly UpdateFromCompaniesHouseService updateFromCompaniesHouseService;

        public DummyController(IDataRepository dataRepository,
            AuditLogger auditLogger,
            UpdateFromCompaniesHouseService updateFromCompaniesHouseService)
        {
            this.dataRepository = dataRepository;
            this.auditLogger = auditLogger;
            this.updateFromCompaniesHouseService = updateFromCompaniesHouseService;
        }

        [HttpGet("audit")]
        public void Dummy(long? organisationId)
        {
            if (Config.IsDevelopment() || Config.IsLocal())
            {
                auditLogger.AuditAction(
                    this,
                    AuditedAction.AdminChangeLateFlag,
                    organisationId,
                    new Dictionary<string, string> {{"SomeText", "It's fun to change the late flag"}, {"ANumber", "2"}});
            }
        }

        [HttpGet("companiesHouse")]
        public async Task CompaniesHouseTesting(string companyNumber)
        {
            if (Config.IsDevelopment() || Config.IsLocal())
            {
                Organisation organisation = dataRepository.GetAll<Organisation>().FirstOrDefault(org => org.CompanyNumber == companyNumber);
                updateFromCompaniesHouseService.UpdateOrganisationDetails(organisation.OrganisationId);
            }
        }

    }
}
