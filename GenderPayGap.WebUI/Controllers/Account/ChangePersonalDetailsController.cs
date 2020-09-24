using GenderPayGap.Core.Interfaces;
using GenderPayGap.Database;
using GenderPayGap.WebUI.Helpers;
using GenderPayGap.WebUI.Models.Account;
using GovUkDesignSystem.Parsers;
using Microsoft.AspNetCore.Mvc;

namespace GenderPayGap.WebUI.Controllers.Account
{
    [Route("manage-account")]
    public class ChangePersonalDetailsController : Controller
    {
        
        private readonly IDataRepository dataRepository;

        public ChangePersonalDetailsController(
            IDataRepository dataRepository)
        {
            this.dataRepository = dataRepository;
        }

        // The user asks to change their personal details
        // We direct them to a page that asks for their updated personal details
        [HttpGet("change-personal-details")]
        public IActionResult ChangePersonalDetailsGet()
        {
            var viewModel = new ChangePersonalDetailsViewModel();

            User currentUser = ControllerHelper.GetGpgUserFromAspNetUser(User, dataRepository);

            viewModel.FirstName = currentUser.Firstname;
            viewModel.LastName = currentUser.Lastname;
            viewModel.JobTitle = currentUser.JobTitle;
            viewModel.ContactPhoneNumber = currentUser.ContactPhoneNumber;
            
            return View("ChangePersonalDetails", viewModel);
        }
        
        // The user submits some new personal details
        // We validate it (e.g. that all fields are filled in)
        [HttpPost("change-personal-details")]
        public IActionResult ChangePersonalDetailsPost(ChangePersonalDetailsViewModel viewModel)
        {
            return null;
        }

    }
}
