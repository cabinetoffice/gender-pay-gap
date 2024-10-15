using System.Collections.Generic;
using GenderPayGap.Core;
using GenderPayGap.WebUI.Classes.Presentation;
using GenderPayGap.WebUI.Models.Search;
using Microsoft.AspNetCore.Mvc;

namespace GenderPayGap.WebUI.Controllers
{
    public class SearchController : Controller
    {

        private readonly ICompareViewService compareViewService;
        
        public SearchController(ICompareViewService compareViewService)
        {
            this.compareViewService = compareViewService;
        }

        [HttpGet("search")]
        public IActionResult SearchPage(SearchPageViewModel viewModel)
        {
            compareViewService.LoadComparedEmployersFromCookie();
            
            return View("SearchPage", viewModel);
        }


    }
}
