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
    }
}
