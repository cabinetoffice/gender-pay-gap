using System.Collections.Generic;
using System.Linq;
using GenderPayGap.Core;
using GenderPayGap.Core.Interfaces;
using GenderPayGap.Database;
using GenderPayGap.WebUI.Helpers;
using GenderPayGap.WebUI.Models.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GenderPayGap.WebUI.Controllers.Administration
{
    [Authorize(Roles = LoginRoles.GpgAdmin)]
    [Route("admin")]
    public class AdminController : Controller
    {

        private readonly IDataRepository dataRepository;

        public AdminController(IDataRepository dataRepository)
        {
            this.dataRepository = dataRepository;
        }

        [HttpGet("pending-registrations")]
        public IActionResult PendingRegistrations()
        {
            List<UserOrganisation> nonUkAddressUserOrganisations =
                dataRepository
                    .GetAll<UserOrganisation>()
                    .Where(uo => uo.User.Status == UserStatuses.Active)
                    .Where(uo => uo.PINConfirmedDate == null)
                    .Where(uo => uo.Method == RegistrationMethods.Manual)
                    .Where(uo => uo.Address.IsUkAddress.HasValue)
                    .Where(uo => uo.Address.IsUkAddress.Value == false)
                    .OrderBy(uo => uo.Modified)
                    .ToList();

            List<UserOrganisation> publicSectorUserOrganisations =
                dataRepository
                    .GetAll<UserOrganisation>()
                    .Where(uo => uo.User.Status == UserStatuses.Active)
                    .Where(uo => uo.PINConfirmedDate == null)
                    .Where(uo => uo.Method == RegistrationMethods.Manual)
                    .Where(uo => uo.Organisation.SectorType == SectorTypes.Public)
                    .OrderBy(uo => uo.Modified)
                    .ToList();

            List<UserOrganisation> allManuallyRegisteredUserOrganisations =
                dataRepository
                    .GetAll<UserOrganisation>()
                    .Where(uo => uo.User.Status == UserStatuses.Active)
                    .Where(uo => uo.PINConfirmedDate == null)
                    .Where(uo => uo.Method == RegistrationMethods.Manual)
                    .OrderBy(uo => uo.Modified)
                    .ToList();

            List<UserOrganisation> remainingManuallyRegisteredUserOrganisations =
                allManuallyRegisteredUserOrganisations
                    .Except(publicSectorUserOrganisations)
                    .Except(nonUkAddressUserOrganisations)
                    .ToList();

            var model = new PendingRegistrationsViewModel {
                PublicSectorUserOrganisations = publicSectorUserOrganisations,
                NonUkAddressUserOrganisations = nonUkAddressUserOrganisations,
                ManuallyRegisteredUserOrganisations = remainingManuallyRegisteredUserOrganisations
            };

            return View("PendingRegistrations", model);
        }

    }
}
