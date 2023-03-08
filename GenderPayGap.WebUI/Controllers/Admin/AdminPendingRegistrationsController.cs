using System.Collections.Generic;
using System.Linq;
using GenderPayGap.Core;
using GenderPayGap.Core.Interfaces;
using GenderPayGap.Database;
using GenderPayGap.WebUI.Helpers;
using GenderPayGap.WebUI.Models.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GenderPayGap.WebUI.Controllers.Admin
{
    [Authorize(Roles = LoginRoles.GpgAdmin)]
    [Route("admin")]
    public class AdminPendingRegistrationsController : Controller
    {

        private readonly IDataRepository dataRepository;

        public AdminPendingRegistrationsController(IDataRepository dataRepository)
        {
            this.dataRepository = dataRepository;
        }

        [HttpGet("pending-registrations")]
        public IActionResult PendingRegistrations()
        {
            List<UserOrganisation> allManualRegistrations =
                dataRepository
                    .GetAll<UserOrganisation>()
                    .Where(uo => uo.User.Status == UserStatuses.Active)
                    .Where(uo => uo.PINConfirmedDate == null)
                    .Where(uo => uo.Method == RegistrationMethods.Manual)
                    .OrderBy(uo => uo.Modified)
                    .ToList();

            List<UserOrganisation> nonUkAddressRegistrations = allManualRegistrations.Where(uo => uo.Address.IsUkAddress == false).ToList();
            List<UserOrganisation> publicSectorRegistrations = allManualRegistrations.Where(uo => uo.Organisation.SectorType == SectorTypes.Public).ToList();
            List<UserOrganisation> remainingRegistrations = allManualRegistrations.Except(publicSectorRegistrations).Except(nonUkAddressRegistrations).ToList();

            var model = new PendingRegistrationsViewModel {
                PublicSectorUserOrganisations = publicSectorRegistrations,
                NonUkAddressUserOrganisations = nonUkAddressRegistrations,
                ManuallyRegisteredUserOrganisations = remainingRegistrations
            };

            return View("PendingRegistrations", model);
        }

    }
}
