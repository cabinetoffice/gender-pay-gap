using GenderPayGap.Core.Interfaces;
using GenderPayGap.WebUI.Helpers;
using GenderPayGap.WebUI.Models.AddOrganisation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GenderPayGap.WebUI.Controllers.AddOrganisation
{
    [Authorize(Roles = LoginRoles.GpgEmployer)]
    [Route("add-organisation")]
    public class AddOrganisationSearchController : Controller
    {

        private readonly IDataRepository dataRepository;

        public AddOrganisationSearchController(IDataRepository dataRepository)
        {
            this.dataRepository = dataRepository;
        }


        [HttpGet("{sector}/search")]
        public IActionResult Search(AddOrganisationSearchViewModel viewModel)
        {
            ControllerHelper.ThrowIfUserAccountRetiredOrEmailNotVerified(User, dataRepository);

            return View("Search", viewModel);
        }

    }
}
