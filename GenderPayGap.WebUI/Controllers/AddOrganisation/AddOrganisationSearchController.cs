using GenderPayGap.Core;
using GenderPayGap.Core.Interfaces;
using GenderPayGap.WebUI.Helpers;
using GenderPayGap.WebUI.Models.AddOrganisation;
using GenderPayGap.WebUI.Search;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GenderPayGap.WebUI.Controllers.AddOrganisation
{
    [Authorize(Roles = LoginRoles.GpgEmployer)]
    [Route("add-organisation")]
    public class AddOrganisationSearchController : Controller
    {

        private readonly IDataRepository dataRepository;
        private readonly AddOrganisationSearchService searchService;

        public AddOrganisationSearchController(
            IDataRepository dataRepository,
            AddOrganisationSearchService searchService)
        {
            this.dataRepository = dataRepository;
            this.searchService = searchService;
        }


        [HttpGet("{sector}/search")]
        public IActionResult Search(AddOrganisationSearchViewModel viewModel)
        {
            ControllerHelper.Throw404IfFeatureDisabled(FeatureFlag.NewAddOrganisationJourney);

            ControllerHelper.ThrowIfUserAccountRetiredOrEmailNotVerified(User, dataRepository);

            if (!string.IsNullOrWhiteSpace(viewModel.Query))
            {
                if (viewModel.Sector == AddOrganisationSector.Public)
                {
                    viewModel.SearchResults = searchService.SearchPublic(viewModel.Query);
                }
                else
                {
                    viewModel.SearchResults = searchService.SearchPrivate(viewModel.Query);
                }
            }

            return View("Search", viewModel);
        }

    }
}
