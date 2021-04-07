using System;
using GenderPayGap.Core.Interfaces;
using GenderPayGap.Extensions.AspNetCore;
using GenderPayGap.WebUI.Classes;
using GenderPayGap.WebUI.Services;
using Microsoft.AspNetCore.Mvc;

namespace GenderPayGap.WebUI.Controllers
{
    [Route("Register")]
    public partial class RegisterController : BaseController
    {

        private readonly EmailSendingService emailSendingService;
        private readonly AuditLogger auditLogger;

        public RegisterController(
            IHttpCache cache,
            IHttpSession session,
            IDataRepository dataRepository,
            IWebTracker webTracker,
            EmailSendingService emailSendingService,
            AuditLogger auditLogger)
            : base(
            cache,
            session,
            dataRepository,
            webTracker)
        {
            this.emailSendingService = emailSendingService;
            this.auditLogger = auditLogger;
        }

        // This was the old url given out to create a user account that was shared. Since this was changed
        // they now get a number of user support queries asking for an up to date link. Instead redirect the 
        // old link to the new address.
        [HttpGet("about-you")]
        public IActionResult AboutYou()
        {
            return RedirectToActionPermanent("CreateUserAccountGet", "AccountCreation");
        }
    }
}
