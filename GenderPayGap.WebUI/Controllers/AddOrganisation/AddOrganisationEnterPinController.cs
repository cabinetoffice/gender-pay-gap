using System.Linq;
using GenderPayGap.Core;
using GenderPayGap.Core.Interfaces;
using GenderPayGap.Database;
using GenderPayGap.Extensions;
using GenderPayGap.WebUI.Helpers;
using GenderPayGap.WebUI.Models.Report;
using GenderPayGap.WebUI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GenderPayGap.WebUI.Controllers.AddOrganisation
{
    [Authorize(Roles = LoginRoles.GpgEmployer)]
    [Route("add-employer")]
    public class AddOrganisationEnterPinController : Controller
    {

        private readonly IDataRepository dataRepository;
        private readonly EmailSendingService emailSendingService;
        private readonly AuditLogger auditLogger;

        public AddOrganisationEnterPinController(
            IDataRepository dataRepository,
            EmailSendingService emailSendingService,
            AuditLogger auditLogger)
        {
            this.dataRepository = dataRepository;
            this.emailSendingService = emailSendingService;
            this.auditLogger = auditLogger;
        }
        
        [HttpGet("enter-pin/{obfuscatedOrganisationId}")]
        public IActionResult EnterPinGet(string obfuscatedOrganisationId)
        {
            User user = ControllerHelper.GetGpgUserFromAspNetUser(User, dataRepository);
            long organisationId = ControllerHelper.DeObfuscateOrganisationIdOrThrow404(obfuscatedOrganisationId);
            ControllerHelper.ThrowIfUserAccountRetiredOrEmailNotVerified(User, dataRepository);
            ControllerHelper.ThrowIfUserIsNotAwaitingPinInThePostForGivenOrganisation(User, dataRepository, organisationId);
            UserOrganisation userOrganisation = LoadUserOrganisation(user, organisationId);

            var viewModel = new AddOrganisationEnterPinViewModel();
            PopulateViewModel(viewModel, userOrganisation);

            return View("EnterPin", viewModel);
        }

        [ValidateAntiForgeryToken]
        [HttpPost("enter-pin/{obfuscatedOrganisationId}")]
        public IActionResult EnterPinPost(string obfuscatedOrganisationId, AddOrganisationEnterPinViewModel viewModel)
        {
            User user = ControllerHelper.GetGpgUserFromAspNetUser(User, dataRepository);
            long organisationId = ControllerHelper.DeObfuscateOrganisationIdOrThrow404(obfuscatedOrganisationId);
            ControllerHelper.ThrowIfUserAccountRetiredOrEmailNotVerified(User, dataRepository);
            ControllerHelper.ThrowIfUserIsNotAwaitingPinInThePostForGivenOrganisation(User, dataRepository, organisationId);
            UserOrganisation userOrganisation = LoadUserOrganisation(user, organisationId);

            if (!ModelState.IsValid)
            {
                PopulateViewModel(viewModel, userOrganisation);
                return View("EnterPin", viewModel);
            }

            if (userOrganisation.HasExpiredPin())
            {
                ModelState.AddModelError(
                    nameof(viewModel.Pin),
                    $"The PIN has expired. Please contact {Global.GpgReportingEmail}");
                PopulateViewModel(viewModel, userOrganisation);
                return View("EnterPin", viewModel);
            }

            if (userOrganisation.HasAttemptedPinTooManyTimes())
            {
                ModelState.AddModelError(
                    nameof(viewModel.Pin),
                    $"You entered the PIN incorrectly too many times. You can try again in {Global.LockoutMinutes} minutes");
                PopulateViewModel(viewModel, userOrganisation);
                return View("EnterPin", viewModel);
            }

            if (!PinMatchesPinInDatabase(userOrganisation, viewModel.Pin))
            {
                userOrganisation.ConfirmAttempts++;
                userOrganisation.ConfirmAttemptDate = VirtualDateTime.Now;
                dataRepository.SaveChanges();
                
                ModelState.AddModelError(
                    nameof(viewModel.Pin),
                    $"Incorrect PIN");
                PopulateViewModel(viewModel, userOrganisation);
                return View("EnterPin", viewModel);
            }

            ActivateService(userOrganisation);
            
            return View("ServiceActivated", userOrganisation);
        }

        private UserOrganisation LoadUserOrganisation(User user, long organisationId)
        {
            return dataRepository
                .GetAll<UserOrganisation>()
                .Where(uo => uo.UserId == user.UserId)
                .Where(uo => uo.OrganisationId == organisationId)
                .FirstOrDefault();
        }

        private static void PopulateViewModel(AddOrganisationEnterPinViewModel viewModel, UserOrganisation userOrganisation)
        {
            viewModel.UserOrganisation = userOrganisation;
        }

        private static bool PinMatchesPinInDatabase(UserOrganisation userOrg, string modelPin)
        {
            if (modelPin == null)
            {
                return false;
            }

            string normalisedPin = modelPin.Trim().ToUpper();
            if (!string.IsNullOrWhiteSpace(userOrg.PIN) && userOrg.PIN == normalisedPin)
            {
                return true;
            }

            return false;
        }

        private void ActivateService(UserOrganisation userOrg)
        {
            // Set the user org as confirmed
            userOrg.PINConfirmedDate = VirtualDateTime.Now;

            // If the organisation is Pending, set it to be Active
            if (userOrg.Organisation.Status == OrganisationStatuses.Pending)
            {
                userOrg.Organisation.SetStatus(
                    OrganisationStatuses.Active,
                    userOrg.UserId,
                    "PIN Confirmed");
            }

            // Send an email to any other existing users registered to this organisation 
            EmailSendingServiceHelpers.SendUserAddedEmailToExistingUsers(userOrg.Organisation, userOrg.User, emailSendingService);

            // Log the registration
            auditLogger.AuditChangeToOrganisation(
                AuditedAction.RegistrationLog,
                userOrg.Organisation,
                new
                {
                    Status = "PIN Confirmed",
                    Sector = userOrg.Organisation.SectorType,
                    Organisation = userOrg.Organisation.OrganisationName,
                    CompanyNo = userOrg.Organisation.CompanyNumber,
                    SicCodes = userOrg.Organisation.GetSicCodeIdsString(),
                    UserFirstname = userOrg.User.Firstname,
                    UserLastname = userOrg.User.Lastname,
                    UserJobtitle = userOrg.User.JobTitle,
                    UserEmail = userOrg.User.EmailAddress,
                    userOrg.User.ContactPhoneNumber
                },
                User);

            // Save the changes
            dataRepository.SaveChanges();
        }

    }
}
