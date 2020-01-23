using System;
using System.Collections.Generic;
using System.Linq;
using GenderPayGap.Core;
using GenderPayGap.Core.Models;
using GenderPayGap.Database;
using GenderPayGap.Extensions;
using GenderPayGap.WebUI.Classes;
using GenderPayGap.WebUI.Models.Register;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GenderPayGap.WebUI.Controllers
{
    public partial class RegisterController : BaseController
    {

        #region AddAddress

        [Authorize]
        [HttpGet("add-address")]
        public IActionResult AddAddress()
        {
            //Ensure user has completed the registration process
            User currentUser;
            IActionResult checkResult = CheckUserRegisteredOk(out currentUser);
            if (checkResult != null)
            {
                return checkResult;
            }

            //Get the model from the stash
            var model = this.UnstashModel<OrganisationViewModel>();
            if (model == null)
            {
                return View("CustomError", new ErrorViewModel(1112));
            }

            //Pre-populate address from selected employer
            EmployerRecord employer = model.ManualRegistration ? null : model.GetManualEmployer() ?? model.GetSelectedEmployer();

            if (employer != null)
            {
                List<string> list = employer.GetAddressList();
                model.Address1 = list.Count > 0 ? list[0] : null;
                model.Address2 = list.Count > 1 ? list[1] : null;
                model.Address3 = list.Count > 2 ? list[2] : null;
                model.City = null;
                model.County = null;
                model.Country = null;
                model.Postcode = employer.PostCode;
            }

            return View(nameof(AddAddress), model);
        }

        [PreventDuplicatePost]
        [ValidateAntiForgeryToken]
        [Authorize]
        [HttpPost("add-address")]
        public IActionResult AddAddress(OrganisationViewModel model)
        {
            //Ensure user has completed the registration process
            User currentUser;
            IActionResult checkResult = CheckUserRegisteredOk(out currentUser);
            if (checkResult != null)
            {
                return checkResult;
            }

            //Make sure we can load employers from session
            var m = this.UnstashModel<OrganisationViewModel>();
            if (m == null)
            {
                return View("CustomError", new ErrorViewModel(1112));
            }

            model.Employers = m.Employers;
            model.ManualEmployers = m.ManualEmployers;

            //Exclude the contact details
            var excludes = new HashSet<string>();
            excludes.AddRange(
                nameof(model.ContactFirstName),
                nameof(model.ContactLastName),
                nameof(model.ContactJobTitle),
                nameof(model.ContactEmailAddress),
                nameof(model.ContactPhoneNumber));

            //Exclude the organisation details
            excludes.AddRange(
                nameof(model.OrganisationName),
                nameof(model.CompanyNumber),
                nameof(model.CharityNumber),
                nameof(model.MutualNumber),
                nameof(model.OtherName),
                nameof(model.OtherValue));

            //Exclude the search
            excludes.AddRange(nameof(model.SearchText));

            //Exclude the SIC Codes
            excludes.Add(nameof(model.SicCodeIds));

            //Exclude the SIC Codes
            excludes.Add(nameof(model.DUNSNumber));

            //Check model is valid
            ModelState.Exclude(excludes.ToArray());
            if (!ModelState.IsValid)
            {
                this.CleanModelErrors<OrganisationViewModel>();
                return View(nameof(AddAddress), model);
            }

            SectorTypes? sector = model.SectorType;
            var authorised = false;
            EmployerRecord employer = null;
            if (!model.ManualRegistration)
            {
                employer = model.GetManualEmployer();

                if (employer != null)
                {
                    authorised = model.ManualAuthorised;
                }
                else
                {
                    employer = model.GetSelectedEmployer();
                    authorised = model.SelectedAuthorised;
                }
            }

            //Set the address source to the user or original source if unchanged
            if (employer != null && model.GetAddressModel().Equals(employer.GetAddressModel()))
            {
                model.AddressSource = employer.AddressSource;
            }
            else
            {
                model.AddressSource = currentUser.EmailAddress;
            }

            if (model.WrongAddress)
            {
                model.ManualAddress = true;
            }

            //When doing manual address only and user is already authorised redirect to confirm page
            if (model.ManualAddress && sector == SectorTypes.Public && authorised && !employer.HasAnyAddress())
            {
                //We don't need contact info if there is no address only when there is an address
                model.ConfirmReturnAction = nameof(AddAddress);
                this.StashModel(model);
                return RedirectToAction(nameof(ConfirmOrganisation));
            }

            //When manual registration
            this.StashModel(model);
            return RedirectToAction("AddContact");
        }

        #endregion

        #region AddContact

        [Authorize]
        [HttpGet("add-contact")]
        public IActionResult AddContact()
        {
            //Ensure user has completed the registration process
            User currentUser;
            IActionResult checkResult = CheckUserRegisteredOk(out currentUser);
            if (checkResult != null)
            {
                return checkResult;
            }

            //Get the model from the stash
            var model = this.UnstashModel<OrganisationViewModel>();
            if (model == null)
            {
                return View("CustomError", new ErrorViewModel(1112));
            }

            //Pre-load contact details
            if (string.IsNullOrWhiteSpace(model.ContactFirstName))
            {
                model.ContactFirstName = string.IsNullOrWhiteSpace(currentUser.ContactFirstName)
                    ? currentUser.Firstname
                    : currentUser.ContactFirstName;
            }

            if (string.IsNullOrWhiteSpace(model.ContactLastName))
            {
                model.ContactLastName = string.IsNullOrWhiteSpace(currentUser.ContactLastName)
                    ? currentUser.Lastname
                    : currentUser.ContactLastName;
            }

            if (string.IsNullOrWhiteSpace(model.ContactJobTitle))
            {
                model.ContactJobTitle = string.IsNullOrWhiteSpace(currentUser.ContactJobTitle)
                    ? currentUser.JobTitle
                    : currentUser.ContactJobTitle;
            }

            if (string.IsNullOrWhiteSpace(model.ContactEmailAddress))
            {
                model.ContactEmailAddress = string.IsNullOrWhiteSpace(currentUser.ContactEmailAddress)
                    ? currentUser.EmailAddress
                    : currentUser.ContactEmailAddress;
            }

            if (string.IsNullOrWhiteSpace(model.ContactPhoneNumber))
            {
                model.ContactPhoneNumber = currentUser.ContactPhoneNumber;
            }

            return View("AddContact", model);
        }

        [PreventDuplicatePost]
        [ValidateAntiForgeryToken]
        [Authorize]
        [HttpPost("add-contact")]
        public IActionResult AddContact(OrganisationViewModel model)
        {
            //Ensure user has completed the registration process
            User currentUser;
            IActionResult checkResult = CheckUserRegisteredOk(out currentUser);
            if (checkResult != null)
            {
                return checkResult;
            }

            //Make sure we can load employers from session
            var m = this.UnstashModel<OrganisationViewModel>();
            if (m == null)
            {
                return View("CustomError", new ErrorViewModel(1112));
            }

            model.Employers = m.Employers;
            model.ManualEmployers = m.ManualEmployers;

            //Exclude the organisation details
            var excludes = new HashSet<string>();
            excludes.AddRange(
                nameof(model.OrganisationName),
                nameof(model.CompanyNumber),
                nameof(model.CharityNumber),
                nameof(model.MutualNumber),
                nameof(model.OtherName),
                nameof(model.OtherValue));

            //Exclude the address details
            excludes.AddRange(
                nameof(model.Address1),
                nameof(model.Address2),
                nameof(model.Address3),
                nameof(model.City),
                nameof(model.County),
                nameof(model.Country),
                nameof(model.Postcode),
                nameof(model.PoBox));

            //Exclude the search
            excludes.AddRange(nameof(model.SearchText));

            //Exclude the SIC Codes
            excludes.Add(nameof(model.SicCodeIds));

            //Exclude the SIC Codes
            excludes.Add(nameof(model.DUNSNumber));

            //Check model is valid
            ModelState.Exclude(excludes.ToArray());
            if (!ModelState.IsValid)
            {
                this.CleanModelErrors<OrganisationViewModel>();
                return View("AddContact", model);
            }

            //Whenever doing a manual address change redirect to confirm page
            if (model.ManualAddress)
            {
                if (string.IsNullOrWhiteSpace(model.ConfirmReturnAction))
                {
                    model.ConfirmReturnAction = nameof(AddContact);
                }

                this.StashModel(model);
                return RedirectToAction(nameof(ConfirmOrganisation));
            }

            this.StashModel(model);
            return RedirectToAction("AddSector");
        }

        #endregion

        #region AddSector

        [Authorize]
        [HttpGet("add-sector")]
        public IActionResult AddSector()
        {
            //Ensure user has completed the registration process
            User currentUser;
            IActionResult checkResult = CheckUserRegisteredOk(out currentUser);
            if (checkResult != null)
            {
                return checkResult;
            }

            //Get the model from the stash
            var model = this.UnstashModel<OrganisationViewModel>();
            if (model == null)
            {
                return View("CustomError", new ErrorViewModel(1112));
            }

            return View("AddSector", model);
        }

        [PreventDuplicatePost]
        [ValidateAntiForgeryToken]
        [Authorize]
        [HttpPost("add-sector")]
        public IActionResult AddSector(OrganisationViewModel model)
        {
            //Ensure user has completed the registration process
            User currentUser;
            IActionResult checkResult = CheckUserRegisteredOk(out currentUser);
            if (checkResult != null)
            {
                return checkResult;
            }

            //Make sure we can load employers from session
            var m = this.UnstashModel<OrganisationViewModel>();
            if (m == null)
            {
                return View("CustomError", new ErrorViewModel(1112));
            }

            model.Employers = m.Employers;
            model.ManualEmployers = m.ManualEmployers;

            //Exclude the organisation details
            var excludes = new HashSet<string>();
            excludes.AddRange(
                nameof(model.OrganisationName),
                nameof(model.CompanyNumber),
                nameof(model.CharityNumber),
                nameof(model.MutualNumber),
                nameof(model.OtherName),
                nameof(model.OtherValue));

            //Exclude the address details
            excludes.AddRange(
                nameof(model.Address1),
                nameof(model.Address2),
                nameof(model.Address3),
                nameof(model.City),
                nameof(model.County),
                nameof(model.Country),
                nameof(model.Postcode),
                nameof(model.PoBox));

            //Exclude the contact details
            excludes.AddRange(
                nameof(model.ContactFirstName),
                nameof(model.ContactLastName),
                nameof(model.ContactJobTitle),
                nameof(model.ContactEmailAddress),
                nameof(model.ContactPhoneNumber));

            //Exclude the SIC codes when public sector
            if (model.SectorType != SectorTypes.Private)
            {
                excludes.Add(nameof(model.SicCodeIds));
            }

            //Exclude the SIC Codes
            excludes.Add(nameof(model.DUNSNumber));

            //Check model is valid
            ModelState.Exclude(excludes.ToArray());

            var codes = new SortedSet<int>();
            if (!string.IsNullOrWhiteSpace(model.SicCodeIds))
            {
                string separators = ";,: \n\r" + Environment.NewLine;
                if (!model.SicCodeIds.ContainsAll(Text.NumberChars + separators))
                {
                    ModelState.AddModelError("", "You have entered an invalid SIC code");
                }
                else
                {
                    foreach (string codeStr in model.SicCodeIds.SplitI(separators))
                    {
                        int code = codeStr.ToInt32();
                        if (code == 0)
                        {
                            ModelState.AddModelError("", codeStr + " is not a recognised SIC code");
                            break;
                        }

                        if (codes.Contains(code))
                        {
                            ModelState.AddModelError("", "Duplicate SIC code detected");
                            break;
                        }

                        var sic = DataRepository.Get<SicCode>(code);
                        if (sic == null || code == 1)
                        {
                            ModelState.AddModelError("", code + " is not a recognised SIC code");
                            break;
                        }

                        codes.Add(code);
                    }
                }
            }

            if (!ModelState.IsValid)
            {
                this.CleanModelErrors<OrganisationViewModel>();
                return View("AddSector", model);
            }


            model.SicSource = currentUser.EmailAddress;

            model.ConfirmReturnAction = nameof(AddSector);
            this.StashModel(model);
            return RedirectToAction(nameof(ConfirmOrganisation));
        }

        #endregion

    }
}
