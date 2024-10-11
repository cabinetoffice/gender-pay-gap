using System.Collections.Generic;
using GenderPayGap.Core;
using GenderPayGap.WebUI.Classes.Presentation;
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
        public IActionResult SearchPage(
            string employerName = "",
            List<OrganisationSizes> employerSize = null,
            List<string> sector = null,
            bool reportedLate = false,
            string orderBy = "relevance")
        {
            compareViewService.LoadComparedEmployersFromCookie();
            
            return View("SearchPage");
        }


    }
}
