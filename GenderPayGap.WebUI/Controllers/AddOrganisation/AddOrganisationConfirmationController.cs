using GenderPayGap.Core.Interfaces;
using GenderPayGap.Extensions;
using GenderPayGap.WebUI.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GenderPayGap.WebUI.Controllers.AddOrganisation
{
    [Authorize(Roles = LoginRoles.GpgEmployer)]
    [Route("add-organisation")]
    public class AddOrganisationConfirmationController : Controller
    {

        private readonly IDataRepository dataRepository;

        public AddOrganisationConfirmationController(
            IDataRepository dataRepository)
        {
            this.dataRepository = dataRepository;
        }


        [HttpGet("confirmation")]
        public IActionResult Confirmation(string id)
        {
            string userIdAndOrganisationId = Encryption.DecryptQuerystring(id);
            long userId = long.Parse(userIdAndOrganisationId.Split(":")[0]);
            long organisationId = long.Parse(userIdAndOrganisationId.Split(":")[1]);

            return Json(new {userId, organisationId, from = "AddOrganisationConfirmationController"});
        }

    }
}
