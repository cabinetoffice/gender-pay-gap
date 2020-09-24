using GenderPayGap.Core.Interfaces;
using GenderPayGap.Database;
using GenderPayGap.WebUI.Classes;
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
        [PreventDuplicatePost]
        [ValidateAntiForgeryToken]
        public IActionResult ChangePersonalDetailsPost(ChangePersonalDetailsViewModel viewModel)
        {
            // We don't validate contact number, since this is manually checked and we'll accept anything here
            viewModel.ParseAndValidateParameters(Request, m => m.FirstName);
            viewModel.ParseAndValidateParameters(Request, m => m.LastName);
            viewModel.ParseAndValidateParameters(Request, m => m.JobTitle);

            if (viewModel.HasAnyErrors())
            {
                return View("ChangePersonalDetails", viewModel);
            }
            
            // Get the user db entry
            User currentUser = ControllerHelper.GetGpgUserFromAspNetUser(User, dataRepository);
            
            // Update the user's information
            currentUser.Firstname = viewModel.FirstName;
            currentUser.Lastname = viewModel.LastName;
            currentUser.JobTitle = viewModel.JobTitle;
            currentUser.ContactPhoneNumber = viewModel.ContactPhoneNumber;

            // Save updates
            dataRepository.SaveChangesAsync().Wait();
            
            // TODO: Add success notification to show on ManageAccount page after redirect

            // Return user to the Manage Account page
            return RedirectToAction("ManageAccount", "ManageAccount", new { Area = "Account" });
        }

    }
}
