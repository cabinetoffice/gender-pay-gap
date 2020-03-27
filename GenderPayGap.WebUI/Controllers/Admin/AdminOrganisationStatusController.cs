using System.Threading.Tasks;
using GenderPayGap.BusinessLogic.Services;
using GenderPayGap.Core.Interfaces;
using GenderPayGap.Database;
using GenderPayGap.WebUI.Classes;
using GenderPayGap.WebUI.Models.Admin;
using GovUkDesignSystem.Parsers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GenderPayGap.WebUI.Controllers
{
    [Authorize(Roles = "GPGadmin")]
    [Route("admin")]
    public class AdminOrganisationStatusController : Controller
    {

        private readonly AuditLogger auditLogger;

        private readonly IDataRepository dataRepository;

        public AdminOrganisationStatusController(IDataRepository dataRepository, AuditLogger auditLogger)
        {
            this.dataRepository = dataRepository;
            this.auditLogger = auditLogger;
        }

        [HttpGet("organisation/{id}/status")]
        public IActionResult ViewStatusHistory(long id)
        {
            var organisation = dataRepository.Get<Organisation>(id);

            return View("ViewOrganisationStatus", organisation);
        }


        [HttpGet("organisation/{id}/status/change")]
        public IActionResult ChangeStatusGet(long id)
        {
            var organisation = dataRepository.Get<Organisation>(id);

            var viewModel = new AdminChangeStatusViewModel
            {
                OrganisationName = organisation.OrganisationName,
                OrganisationId = organisation.OrganisationId,
                CurrentStatus = organisation.Status
            };

            return View("ChangeStatus", viewModel);
        }

        [HttpPost("organisation/{id}/status/change")]
        [PreventDuplicatePost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangeStatusPost(long id, AdminChangeStatusViewModel viewModel)
        {
            viewModel.ParseAndValidateParameters(Request, m => m.Reason);

            if (viewModel.HasAnyErrors())
            {
                return View("ChangeStatus", viewModel);
            }

            return RedirectToAction("ViewStatusHistory", "AdminOrganisationStatus", new {id});
        }

    }
}
