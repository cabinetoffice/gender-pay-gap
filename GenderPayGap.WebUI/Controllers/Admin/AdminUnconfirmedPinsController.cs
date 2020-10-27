using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GenderPayGap.Core;
using GenderPayGap.Core.Interfaces;
using GenderPayGap.Database;
using GenderPayGap.Extensions;
using GenderPayGap.WebUI.Classes;
using GenderPayGap.WebUI.Helpers;
using GenderPayGap.WebUI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GenderPayGap.WebUI.Controllers.Admin
{
    [Authorize(Roles = LoginRoles.GpgAdmin)]
    [Route("admin")]
    public class AdminUnconfirmedPinsController : Controller
    {

        private readonly IDataRepository dataRepository;
        private readonly EmailSendingService emailSendingService;

        public AdminUnconfirmedPinsController(
            IDataRepository dataRepository,
            EmailSendingService emailSendingService
        )
        {
            this.dataRepository = dataRepository;
            this.emailSendingService = emailSendingService;
        }

        [HttpGet("unconfirmed-pins")]
        public IActionResult UnconfirmedPins()
        {
            List<UserOrganisation> model = dataRepository.GetAll<UserOrganisation>()
                .Where(uo => uo.Method == RegistrationMethods.PinInPost)
                .Where(uo => uo.PINConfirmedDate == null)
                .Where(uo => uo.PIN != null)
                .OrderByDescending(uo => uo.PINConfirmedDate.Value)
                .ToList();

            return View("../Admin/UnconfirmedPins", model);
        }

        [HttpGet("send-pin")]
        public async Task<IActionResult> SendPinWarning(long userId, long organisationId)
        {
            UserOrganisation userOrganisation = await dataRepository.GetAll<UserOrganisation>()
                .FirstOrDefaultAsync(uo => uo.UserId == userId && uo.OrganisationId == organisationId);

            return View("../Admin/SendPinWarning", userOrganisation);
        }

        [HttpPost("send-pin")]
        [PreventDuplicatePost]
        [ValidateAntiForgeryToken]
        public IActionResult SendPin(long userId, long organisationId)
        {
            UserOrganisation userOrganisation = dataRepository.GetAll<UserOrganisation>()
                .FirstOrDefault(uo => uo.UserId == userId && uo.OrganisationId == organisationId);

            if (userOrganisation.PINSentDate.Value.AddDays(Global.PinInPostExpiryDays) < VirtualDateTime.Now)
            {
                string newPin = Crypto.GeneratePinInThePost();
                userOrganisation.PIN = newPin;
            }

            userOrganisation.PINSentDate = VirtualDateTime.Now;
            dataRepository.SaveChanges();

            emailSendingService.SendPinEmail(
                userOrganisation.User.EmailAddress,
                userOrganisation.PIN,
                userOrganisation.Organisation.OrganisationName);

            return View("../Admin/SendPinConfirmation", userOrganisation);
        }

    }

}
