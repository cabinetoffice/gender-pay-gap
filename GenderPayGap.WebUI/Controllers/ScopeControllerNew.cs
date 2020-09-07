using GenderPayGap.Core.Interfaces;
using GenderPayGap.WebUI.Helpers;
using GenderPayGap.WebUI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GenderPayGap.WebUI.Controllers
{
    
    [Authorize(Roles = LoginRoles.GpgEmployer)]
    [Microsoft.AspNetCore.Components.Route("scope")]
    public class ScopeControllerNew : Controller
    {

        private readonly EmailSendingService emailSendingService;
        private readonly IDataRepository dataRepository;
        
        public ScopeControllerNew(EmailSendingService emailSendingService, IDataRepository dataRepository)
        {
            this.emailSendingService = emailSendingService;
            this.dataRepository = dataRepository;
        }

    }
}
