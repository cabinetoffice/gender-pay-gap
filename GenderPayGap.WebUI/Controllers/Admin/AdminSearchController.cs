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

        private readonly AdminSearchService adminSearchService;

        public AdminSearchController(AdminSearchService adminSearchService)
        {
            this.adminSearchService = adminSearchService;
        }

        [HttpGet("search")]
        public IActionResult SearchGet(string query, string orderBy = "relevance")
        {
            if (query == null)
            {
                return View("../Admin/Search", new AdminSearchViewModel());
            }

            var viewModel = new AdminSearchViewModel {SearchQuery = query, OrderBy = orderBy};

            if (string.IsNullOrWhiteSpace(query))
            {
                viewModel.Error = "Search query must not be empty";
            }
            else
            {
                bool orderByRelevance = orderBy == "relevance";
                AdminSearchResultsViewModel results = adminSearchService.Search(query, orderByRelevance);

                viewModel.SearchResults = results;
            }

            return View("../Admin/Search", viewModel);
        }

    }
}
