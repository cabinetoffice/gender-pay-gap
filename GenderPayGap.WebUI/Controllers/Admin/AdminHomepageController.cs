using System.Linq;
using GenderPayGap.Core;
using GenderPayGap.Core.Interfaces;
using GenderPayGap.Database.Models;
using GenderPayGap.WebUI.Helpers;
using GenderPayGap.WebUI.Models.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GenderPayGap.WebUI.Controllers.Admin
{
    [Authorize(Roles = LoginRoles.GpgAdmin)]
    [Route("admin")]
    public class AdminHomepageController : Controller
    {

        private readonly IDataRepository dataRepository;

        public AdminHomepageController(IDataRepository dataRepository)
        {
            this.dataRepository = dataRepository;
        }


        [HttpGet]
        public IActionResult AdminHomePage()
        {
            var viewModel = new AdminHomepageViewModel
            {
                NewFeedbackCount = dataRepository.GetAll<Feedback>().Count(f => f.FeedbackStatus == FeedbackStatus.New),
                NonSpamFeedbackCount = dataRepository.GetAll<Feedback>().Count(f => f.FeedbackStatus == FeedbackStatus.NotSpam)
            };

            return View("Home", viewModel);
        }

    }
}
