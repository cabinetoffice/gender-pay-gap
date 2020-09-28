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

            // Get the current user
            User currentUser = ControllerHelper.GetGpgUserFromAspNetUser(User, dataRepository);

            // Fill the viewModel with the current user's information
            viewModel.FirstName = currentUser.Firstname;
            viewModel.LastName = currentUser.Lastname;
            viewModel.JobTitle = currentUser.JobTitle;
            viewModel.ContactPhoneNumber = currentUser.ContactPhoneNumber;
            
            // Return the Change Personal Details form
            return View("ChangePersonalDetails", viewModel);
        }
        
        // The user submits some new personal details
        // We validate it (e.g. that all fields except contact phone number are filled in)
        // Then we save the updates and return the user to the Manage Account page
        [HttpPost("change-personal-details")]
        [PreventDuplicatePost]
        [ValidateAntiForgeryToken]
        public IActionResult ChangePersonalDetailsPost(ChangePersonalDetailsViewModel viewModel)
        {
            viewModel.ParseAndValidateParameters(Request, m => m.FirstName);
            viewModel.ParseAndValidateParameters(Request, m => m.LastName);
            viewModel.ParseAndValidateParameters(Request, m => m.JobTitle);
            viewModel.ParseAndValidateParameters(Request, m => m.ContactPhoneNumber);

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
            
            string nextPageUrl = Url.Action("ManageAccount", "ManageAccount", new {Area = "Account"});
            StatusMessageHelper.SetStatusMessage(Response, "Saved changes to personal details", nextPageUrl);

            // Return user to the Manage Account page
            return LocalRedirect(nextPageUrl);
        }

    }
}
