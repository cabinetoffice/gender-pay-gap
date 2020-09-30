using GenderPayGap.Core.Interfaces;
using GenderPayGap.Database;
using GenderPayGap.WebUI.Classes;
using GenderPayGap.WebUI.Helpers;
using GenderPayGap.WebUI.Models.Account;
using Microsoft.AspNetCore.Mvc;

namespace GenderPayGap.WebUI.Controllers.Account
{
    [Route("manage-account")]
    public class CloseAccountNewController : Controller
    {

        private readonly IDataRepository dataRepository;

        public CloseAccountNewController(
            IDataRepository dataRepository)
        {
            this.dataRepository = dataRepository;
        }

        // The user asks to close their account
        // We direct them to a page that asks for confirmation
        [HttpGet("close")]
        public IActionResult CloseAccountGet()
        {
            // Get the current user
            User currentUser = ControllerHelper.GetGpgUserFromAspNetUser(User, dataRepository);
            
            var viewModel = new CloseAccountNewViewModel
            {
                User = currentUser
            };

            // Return the Change Personal Details form
            return View("CloseAccountNew", viewModel);
        }

        [HttpPost("close")]
        [PreventDuplicatePost]
        [ValidateAntiForgeryToken]
        public IActionResult CloseAccountPost()
        {
            return null;
        }
        

    }
}
