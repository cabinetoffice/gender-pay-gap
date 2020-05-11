using GenderPayGap.WebUI.Models.Admin;
using GenderPayGap.WebUI.Search;
using GenderPayGap.WebUI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GenderPayGap.WebUI.Controllers
{
    [Authorize(Roles = "GPGadmin")]
    [Route("admin")]
    public class AdminSearchController : Controller
    {

        private readonly SearchRepository searchRepository;

        public AdminSearchController(SearchRepository searchRepository)
        {
            this.searchRepository = searchRepository;
        }

        [HttpGet("search")]
        public IActionResult SearchGet(string query)
        {
            if (query == null)
            {
                return View("../Admin/Search", new AdminSearchViewModel());
            }

            var viewModel = new AdminSearchViewModel {SearchQuery = query};

            if (string.IsNullOrWhiteSpace(query))
            {
                viewModel.Error = "Search query must not be empty";
            }
            else
            {
                AdminSearchResultsViewModel results = searchRepository.Search(query);

                viewModel.SearchResults = results;
            }

            return View("../Admin/Search", viewModel);
        }

    }
}
