using System;
using System.Linq;
using GenderPayGap.Core;
using GenderPayGap.Core.Interfaces;
using GenderPayGap.Database;
using GenderPayGap.Extensions;
using GenderPayGap.WebUI.ErrorHandling;
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
        public IActionResult Confirmation(string confirmationId)
        {
            ControllerHelper.Throw404IfFeatureDisabled(FeatureFlag.NewAddOrganisationJourney);

            ControllerHelper.ThrowIfUserAccountRetiredOrEmailNotVerified(User, dataRepository);

            UserOrganisation userOrganisation = LoadUserOrganisationWithConfirmationId(confirmationId);

            return View("Confirmation", userOrganisation);
        }

        private UserOrganisation LoadUserOrganisationWithConfirmationId(string confirmationId)
        {
            try
            {
                string userIdAndOrganisationId = Encryption.DecryptQuerystring(confirmationId);
                long userId = long.Parse(userIdAndOrganisationId.Split(":")[0]);
                long organisationId = long.Parse(userIdAndOrganisationId.Split(":")[1]);

                UserOrganisation userOrganisation = dataRepository.GetAll<UserOrganisation>()
                    .Where(uo => uo.UserId == userId && uo.OrganisationId == organisationId)
                    .FirstOrDefault();

                if (userOrganisation == null)
                {
                    throw new PageNotFoundException();
                }

                return userOrganisation;
            }
            catch (Exception e)
            {
                throw new PageNotFoundException();
            }
        }

    }
}
