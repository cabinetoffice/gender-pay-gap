using GenderPayGap.Core.API;
using GenderPayGap.Core.Interfaces;
using GenderPayGap.Database;
using GenderPayGap.WebUI.Classes;
using GenderPayGap.WebUI.Models.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GenderPayGap.WebUI.Controllers.Admin
{
    [Authorize(Roles = "GPGadmin")]
    [Route("admin")]
    public class AdminOrganisationCompanyNumberController : Controller
    {

        private readonly IDataRepository dataRepository;
        private readonly ICompaniesHouseAPI companiesHouseApi;
        private readonly AuditLogger auditLogger;

        public AdminOrganisationCompanyNumberController(
            IDataRepository dataRepository,
            ICompaniesHouseAPI companiesHouseApi,
            AuditLogger auditLogger)
        {
            this.dataRepository = dataRepository;
            this.companiesHouseApi = companiesHouseApi;
            this.auditLogger = auditLogger;
        }

        [HttpGet("organisation/{id}/change-company-number")]
        public IActionResult ChangeCompanyNumberGet(long id)
        {
            Organisation organisation = dataRepository.Get<Organisation>(id);

            var viewModel = new AdminOrganisationCompanyNumberViewModel
            {
                Organisation = organisation
            };

            if (!string.IsNullOrEmpty(organisation.CompanyNumber))
            {
                return View("ViewOrganisationCompanyNumber", viewModel);
            }
            else
            {
                return View("ViewOrganisationCompanyNumber", viewModel);
            }
        }

        [HttpPost("organisation/{id}/change-company-number")]
        public IActionResult ChangeCompanyNumberPost(long id, AdminOrganisationCompanyNumberViewModel viewModel)
        {
            Organisation organisation = dataRepository.Get<Organisation>(id);

            viewModel.Organisation = organisation;

            return View("ViewOrganisationCompanyNumber", viewModel);
        }

    }
}
