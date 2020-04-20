using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GenderPayGap.Core;
using GenderPayGap.Core.Api;
using GenderPayGap.Core.Classes;
using GenderPayGap.Core.Classes.Logger;
using GenderPayGap.Core.Models;
using GenderPayGap.Core.Models.HttpResultModels;
using GenderPayGap.Database;
using GenderPayGap.Extensions;
using GenderPayGap.Extensions.AspNetCore;
using GenderPayGap.WebUI.Classes;
using GenderPayGap.WebUI.Models.Register;
using GenderPayGap.WebUI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GenderPayGap.WebUI.Controllers
{
    public partial class RegisterController : BaseController
    {

        #region Organisation Type

        [Authorize]
        [HttpGet("organisation-type")]
        public IActionResult OrganisationType()
        {
            //Ensure user has completed the registration process
            User currentUser;
            IActionResult checkResult = CheckUserRegisteredOk(out currentUser);
            if (checkResult != null)
            {
                return checkResult;
            }

            var model = new OrganisationViewModel();
            model.Employers = new PagedResult<EmployerRecord>();
            this.StashModel(model);
            if (currentUser.UserOrganisations.Any())
            {
                model.BackAction = Url.Action(nameof(OrganisationController.ManageOrganisations), "Organisation");
            }

            return View("OrganisationType", model);
        }

        /// <summary>
        ///     Get the sector type
        /// </summary>
        [PreventDuplicatePost]
        [ValidateAntiForgeryToken]
        [Authorize]
        [HttpPost("organisation-type")]
        public IActionResult OrganisationType(OrganisationViewModel model)
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

            //TODO validate the submitted fields
            ModelState.Clear();

            if (!model.SectorType.EqualsI(SectorTypes.Private, SectorTypes.Public))
            {
                AddModelError(3005, "SectorType");
                this.CleanModelErrors<OrganisationViewModel>();
                return View("OrganisationType", model);
            }

            CompaniesHouseFailures = 0;

            this.StashModel(model);
            return RedirectToAction("OrganisationSearch");
        }

        #endregion

        #region Organisation Search

        /// Search employer
        [Authorize]
        [HttpGet("organisation-search")]
        public IActionResult OrganisationSearch()
        {
            //Ensure user has completed the registration process
            User currentUser;
            IActionResult checkResult = CheckUserRegisteredOk(out currentUser);
            if (checkResult != null)
            {
                return checkResult;
            }

            //Make sure we can load employers from session
            var model = this.UnstashModel<OrganisationViewModel>();
            if (model == null)
            {
                return View("CustomError", new ErrorViewModel(1112));
            }

            model.ManualRegistration = true;
            model.BackAction = "OrganisationSearch";
            model.OrganisationName = null;
            model.CompanyNumber = null;
            model.Address1 = null;
            model.Address2 = null;
            model.Address3 = null;
            model.Country = null;
            model.Postcode = null;
            model.PoBox = null;

            this.StashModel(model);

            return View("OrganisationSearch", model);
        }

        /// <summary>
        ///     Search employer submit
        /// </summary>
        [PreventDuplicatePost]
        [ValidateAntiForgeryToken]
        [Authorize]
        [HttpPost("organisation-search")]
        public async Task<IActionResult> OrganisationSearch(OrganisationViewModel model)
        {
            //Ensure user has completed the registration process
            User currentUser;
            IActionResult checkResult = CheckUserRegisteredOk(out currentUser);
            if (checkResult != null)
            {
                return checkResult;
            }

            ModelState.Include("SearchText");
            if (!ModelState.IsValid)
            {
                this.CleanModelErrors<OrganisationViewModel>();
                return View("OrganisationSearch", model);
            }

            //Make sure we can load employers from session
            var m = this.UnstashModel<OrganisationViewModel>();
            if (m == null)
            {
                return View("CustomError", new ErrorViewModel(1112));
            }

            model.Employers = m.Employers;
            model.ManualRegistration = true;
            model.BackAction = "OrganisationSearch";
            model.SelectedEmployerIndex = -1;
            model.OrganisationName = null;
            model.CompanyNumber = null;
            model.Address1 = null;
            model.Address2 = null;
            model.Address3 = null;
            model.Country = null;
            model.Postcode = null;
            model.PoBox = null;

            model.SearchText = model.SearchText.TrimI();

            switch (model.SectorType)
            {
                case SectorTypes.Private:
                    try
                    {
                        model.Employers = await PrivateSectorRepository.SearchAsync(
                            model.SearchText,
                            1,
                            Global.EmployerPageSize,
                            currentUser.EmailAddress.StartsWithI(Global.TestPrefix));
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, ex.Message);

                        CompaniesHouseFailures++;
                        if (CompaniesHouseFailures < 3)
                        {
                            model.Employers?.Results?.Clear();
                            this.StashModel(model);
                            ModelState.AddModelError(1141);
                            return View(model);
                        }

                        DateTime time = VirtualDateTime.Now;
                        CustomLogger.Error(
                            $"Companies House failed: {nameof(OrganisationSearch)}",
                            new {environment = Config.EnvironmentName, time, Exception = ex, model.SearchText});

                        return View("CustomError", new ErrorViewModel(1140));
                    }

                    break;
                case SectorTypes.Public:
                    model.Employers = await PublicSectorRepository.SearchAsync(
                        model.SearchText,
                        1,
                        Global.EmployerPageSize,
                        currentUser.EmailAddress.StartsWithI(Global.TestPrefix));

                    break;

                default:
                    throw new NotImplementedException();
            }

            ModelState.Clear();
            model.LastPrivateSearchRemoteTotal = LastPrivateSearchRemoteTotal;
            if (LastPrivateSearchRemoteTotal == -1)
            {
                CompaniesHouseFailures++;
                if (CompaniesHouseFailures >= 3)
                {
                    return View("CustomError", new ErrorViewModel(1140));
                }
            }
            else
            {
                CompaniesHouseFailures = 0;
            }

            this.StashModel(model);

            //Search again if no results
            if (model.Employers.Results.Count < 1)
            {
                return View("OrganisationSearch", model);
            }

            //Go to step 5 with results
            if (Request.Query["fail"].ToBoolean())
            {
                return RedirectToAction("ChooseOrganisation", "Register", new {fail = true});
            }

            return RedirectToAction("ChooseOrganisation");
        }

        #endregion

        #region Choose Organisation

        /// <summary>
        ///     Choose employer view results
        /// </summary>
        [Authorize]
        [HttpGet("choose-organisation")]
        public IActionResult ChooseOrganisation()
        {
            //Ensure user has completed the registration process
            User currentUser;
            IActionResult checkResult = CheckUserRegisteredOk(out currentUser);
            if (checkResult != null)
            {
                return checkResult;
            }

            //Make sure we can load employers from session
            var model = this.UnstashModel<OrganisationViewModel>();
            if (model == null)
            {
                return View("CustomError", new ErrorViewModel(1112));
            }

            model.NoReference = false;
            model.ManualRegistration = false;
            model.AddressReturnAction = null;
            model.ConfirmReturnAction = null;
            model.ManualAuthorised = false;
            model.ManualAddress = false;
            model.BackAction = "ChooseOrganisation";
            model.OrganisationName = null;
            model.CompanyNumber = null;
            model.AddressSource = null;
            model.Address1 = null;
            model.Address2 = null;
            model.Address3 = null;
            model.City = null;
            model.County = null;
            model.Country = null;
            model.PoBox = null;
            model.Postcode = null;
            model.ContactEmailAddress = null;
            model.ContactFirstName = null;
            model.ContactLastName = null;
            model.ContactJobTitle = null;
            model.ContactPhoneNumber = null;
            model.SicCodeIds = null;

            this.StashModel(model);

            return View("ChooseOrganisation", model);
        }


        /// <summary>
        ///     Choose employer with paging or search
        /// </summary>
        [PreventDuplicatePost]
        [ValidateAntiForgeryToken]
        [Authorize]
        [HttpPost("choose-organisation")]
        public async Task<IActionResult> ChooseOrganisation(OrganisationViewModel model, string command)
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

            int nextPage = m.Employers.CurrentPage;

            model.SelectedEmployerIndex = -1;
            model.OrganisationName = null;
            model.CompanyNumber = null;
            model.Address1 = null;
            model.Address2 = null;
            model.Address3 = null;
            model.Country = null;
            model.Postcode = null;
            model.PoBox = null;
            model.ManualAuthorised = false;
            model.ManualAddress = false;

            var doSearch = false;
            ModelState.Include("SearchText");
            if (command == "search")
            {
                model.SearchText = model.SearchText.TrimI();

                if (!ModelState.IsValid)
                {
                    this.CleanModelErrors<OrganisationViewModel>();
                    return View("ChooseOrganisation", model);
                }

                nextPage = 1;
                doSearch = true;
            }
            else if (command == "pageNext")
            {
                if (nextPage >= model.Employers.PageCount)
                {
                    throw new Exception("Cannot go past last page");
                }

                nextPage++;
                doSearch = true;
            }
            else if (command == "pagePrev")
            {
                if (nextPage <= 1)
                {
                    throw new Exception("Cannot go before previous page");
                }

                nextPage--;
                doSearch = true;
            }
            else if (command.StartsWithI("page_"))
            {
                var page = command.AfterFirst("page_").ToInt32();
                if (page < 1 || page > model.Employers.PageCount)
                {
                    throw new Exception("Invalid page selected");
                }

                if (page != nextPage)
                {
                    nextPage = page;
                    doSearch = true;
                }
            }

            if (doSearch)
            {
                switch (model.SectorType)
                {
                    case SectorTypes.Private:
                        try
                        {
                            model.Employers = await PrivateSectorRepository.SearchAsync(
                                model.SearchText,
                                nextPage,
                                Global.EmployerPageSize,
                                currentUser.EmailAddress.StartsWithI(Global.TestPrefix));
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, ex.Message);

                            CompaniesHouseFailures++;
                            if (CompaniesHouseFailures < 3)
                            {
                                model.Employers?.Results?.Clear();
                                this.StashModel(model);
                                ModelState.AddModelError(1141);
                                return View(model);
                            }

                            DateTime time = VirtualDateTime.Now;
                            CustomLogger.Error(
                                $"Companies House failed: {nameof(ChooseOrganisation)}",
                                new {environment = Config.EnvironmentName, time, Exception = ex, model.SearchText});

                            return View("CustomError", new ErrorViewModel(1140));
                        }

                        model.LastPrivateSearchRemoteTotal = LastPrivateSearchRemoteTotal;
                        if (LastPrivateSearchRemoteTotal == -1)
                        {
                            CompaniesHouseFailures++;
                            if (CompaniesHouseFailures >= 3)
                            {
                                return View("CustomError", new ErrorViewModel(1140));
                            }
                        }
                        else
                        {
                            CompaniesHouseFailures = 0;
                        }

                        break;

                    case SectorTypes.Public:
                        model.Employers = await PublicSectorRepository.SearchAsync(
                            model.SearchText,
                            nextPage,
                            Global.EmployerPageSize,
                            currentUser.EmailAddress.StartsWithI(Global.TestPrefix));
                        break;

                    default:
                        throw new NotImplementedException();
                }

                ModelState.Clear();
                this.StashModel(model);

                //Go back if no results
                if (model.Employers.Results.Count < 1)
                {
                    return RedirectToAction("OrganisationSearch");
                }

                //Otherwise show results
                return View("ChooseOrganisation", model);
            }

            if (command.StartsWithI("employer_"))
            {
                var employerIndex = command.AfterFirst("employer_").ToInt32();
                EmployerRecord employer = model.Employers.Results[employerIndex];

                //Ensure employers from companies house have a sector
                if (employer.SectorType == SectorTypes.Unknown)
                {
                    employer.SectorType = model.SectorType.Value;
                }

                //Make sure user is fully registered for one private org before registering another 
                if (model.SectorType == SectorTypes.Private
                    && currentUser.UserOrganisations.Any()
                    && !currentUser.UserOrganisations.Any(uo => uo.PINConfirmedDate != null))
                {
                    AddModelError(3022);
                    this.CleanModelErrors<OrganisationViewModel>();
                    return View("ChooseOrganisation", model);
                }

                //Get the organisation from the database
                Organisation org = employer.OrganisationId > 0 ? DataRepository.Get<Organisation>(employer.OrganisationId) : null;
                if (org == null && !string.IsNullOrWhiteSpace(employer.CompanyNumber))
                {
                    org = DataRepository.GetAll<Organisation>()
                        .FirstOrDefault(o => o.CompanyNumber != null && o.CompanyNumber == employer.CompanyNumber);
                }

                if (org == null && !string.IsNullOrWhiteSpace(employer.EmployerReference))
                {
                    org = DataRepository.GetAll<Organisation>().FirstOrDefault(o => o.EmployerReference == employer.EmployerReference);
                }

                if (org != null)
                {
                    //Make sure the found organisation is active or pending
                    if (org.Status != OrganisationStatuses.Active && org.Status != OrganisationStatuses.Pending)
                    {
                        _logger.LogWarning(
                            $"Attempt to register a {org.Status} organisation",
                            $"Organisation: '{org.OrganisationName}' Reference: '{org.EmployerReference}' User: '{currentUser.EmailAddress}'");
                        return View("CustomError", new ErrorViewModel(1149));
                    }

                    //Make sure the found organisation is of the correct sector type
                    if (org.SectorType != model.SectorType)
                    {
                        return View("CustomError", new ErrorViewModel(model.SectorType == SectorTypes.Private ? 1146 : 1147));
                    }

                    //Ensure user is not already registered for this organisation
                    UserOrganisation userOrg = DataRepository.GetAll<UserOrganisation>()
                        .FirstOrDefault(uo => uo.OrganisationId == org.OrganisationId && uo.UserId == currentUser.UserId);
                    if (userOrg != null)
                    {
                        AddModelError(userOrg.PINConfirmedDate == null ? 3021 : 3020);
                        this.CleanModelErrors<OrganisationViewModel>();
                        return View("ChooseOrganisation", model);
                    }

                    //Ensure there isnt another pending registeration for this organisation
                    //userOrg = DataRepository.GetAll<UserOrganisation>().FirstOrDefault(uo => uo.OrganisationId == org.OrganisationId && uo.UserId != currentUser.UserId && uo.PINSentDate!=null && uo.PINConfirmedDate==null);
                    //if (userOrg != null)
                    //{
                    //    var remainingTime = userOrg.PINSentDate.Value.AddDays(Global.PinInPostExpiryDays) - VirtualDateTime.Now;
                    //    return View("CustomError", new ErrorViewModel(1148, new { remainingTime = remainingTime.ToFriendly(maxParts: 2) }));
                    //}

                    employer.OrganisationId = org.OrganisationId;
                }

                model.SelectedEmployerIndex = employerIndex;


                //Make sure the organisation has an address
                if (employer.SectorType == SectorTypes.Public)
                {
                    model.ManualRegistration = false;
                    model.SelectedAuthorised = employer.IsAuthorised(currentUser.EmailAddress);
                    if (!model.SelectedAuthorised || !employer.HasAnyAddress())
                    {
                        model.ManualAddress = true;
                        model.AddressReturnAction = nameof(ChooseOrganisation);
                        this.StashModel(model);
                        return RedirectToAction("AddAddress");
                    }
                }
                else if (employer.SectorType == SectorTypes.Private && !employer.HasAnyAddress())
                {
                    model.AddressReturnAction = nameof(ChooseOrganisation);
                    model.ManualRegistration = false;
                    model.ManualAddress = true;
                    this.StashModel(model);
                    return RedirectToAction("AddAddress");
                }

                model.ManualRegistration = false;
                model.ManualAddress = false;
                model.AddressReturnAction = null;
            }

            ModelState.Clear();

            //If we havend selected one the reshow same view
            if (model.SelectedEmployerIndex < 0)
            {
                return View("ChooseOrganisation", model);
            }

            model.ConfirmReturnAction = nameof(ChooseOrganisation);
            this.StashModel(model);
            //If private sector add organisation address
            return RedirectToAction(nameof(ConfirmOrganisation));
        }

        #endregion

        #region Add Organisation

        [Authorize]
        [HttpGet("add-organisation")]
        public IActionResult AddOrganisation()
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

            //Prepopulate name if it empty
            if (string.IsNullOrWhiteSpace(model.OrganisationName)
                && !string.IsNullOrWhiteSpace(model.SearchText)
                && (model.SearchText.Length != 8 || !model.SearchText.ContainsNumber()))
            {
                model.OrganisationName = model.SearchText;
            }

            model.ManualRegistration = true;
            model.ManualAuthorised = false;
            model.ManualAddress = false;
            this.StashModel(model);

            return View("AddOrganisation", model);
        }

        [PreventDuplicatePost]
        [ValidateAntiForgeryToken]
        [Authorize]
        [HttpPost("add-organisation")]
        public async Task<IActionResult> AddOrganisation(OrganisationViewModel model)
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

            //Exclude the address details
            var excludes = new HashSet<string>();
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

            //Exclude the SIC Codes
            excludes.Add(nameof(model.SicCodeIds));

            if (model.NoReference)
            {
                excludes.AddRange(
                    nameof(model.CompanyNumber),
                    nameof(model.CharityNumber),
                    nameof(model.MutualNumber),
                    nameof(model.OtherName),
                    nameof(model.OtherValue));
                if (!string.IsNullOrWhiteSpace(model.CompanyNumber)
                    || !string.IsNullOrWhiteSpace(model.CharityNumber)
                    || !string.IsNullOrWhiteSpace(model.MutualNumber)
                    || !string.IsNullOrWhiteSpace(model.OtherName)
                    || !string.IsNullOrWhiteSpace(model.OtherValue))
                {
                    ModelState.AddModelError("", "You must clear all your reference fields");
                }
            }
            else if (!string.IsNullOrWhiteSpace(model.CompanyNumber)
                     || !string.IsNullOrWhiteSpace(model.CharityNumber)
                     || !string.IsNullOrWhiteSpace(model.MutualNumber)
                     || !string.IsNullOrWhiteSpace(model.OtherName)
                     || !string.IsNullOrWhiteSpace(model.OtherValue))
            {
                if (string.IsNullOrWhiteSpace(model.CompanyNumber))
                {
                    excludes.Add(nameof(model.CompanyNumber));
                }

                if (string.IsNullOrWhiteSpace(model.CharityNumber))
                {
                    excludes.Add(nameof(model.CharityNumber));
                }

                if (string.IsNullOrWhiteSpace(model.MutualNumber))
                {
                    excludes.Add(nameof(model.MutualNumber));
                }

                if (string.IsNullOrWhiteSpace(model.OtherName))
                {
                    if (model.OtherName.ReplaceI(" ").EqualsI("CompanyNumber", "CompanyNo", "CompanyReference", "CompanyRef"))
                    {
                        ModelState.AddModelError(nameof(model.OtherName), "Cannot user Company Number as an Other reference");
                    }
                    else if (model.OtherName.ReplaceI(" ").EqualsI("CharityNumber", "CharityNo", "CharityReference", "CharityRef"))
                    {
                        ModelState.AddModelError(nameof(model.OtherName), "Cannot user Charity Number as an Other reference");
                    }
                    else if (model.OtherName.ReplaceI(" ")
                        .EqualsI(
                            "MutualNumber",
                            "MutualNo",
                            "MutualReference",
                            "MutualRef",
                            "MutualPartnsershipNumber",
                            "MutualPartnsershipNo",
                            "MutualPartnsershipReference",
                            "MutualPartnsershipRef"))
                    {
                        ModelState.AddModelError(nameof(model.OtherName), "Cannot user Mutual Partnership Number as an Other reference");
                    }

                    if (string.IsNullOrWhiteSpace(model.OtherValue))
                    {
                        excludes.Add(nameof(model.OtherName));
                        excludes.Add(nameof(model.OtherValue));
                    }
                }
            }

            //Check model is valid
            ModelState.Exclude(excludes.ToArray());
            if (!ModelState.IsValid)
            {
                this.CleanModelErrors<OrganisationViewModel>();
                return View("AddOrganisation", model);
            }

            //Check the company doesnt already exist
            IEnumerable<long> results;
            var orgIds = new HashSet<long>();
            var orgIdrefs = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            if (!model.NoReference)
            {
                if (!string.IsNullOrWhiteSpace(model.CompanyNumber))
                {
                    results = DataRepository.GetAll<Organisation>()
                        .Where(o => o.CompanyNumber == model.CompanyNumber)
                        .Select(o => o.OrganisationId);
                    if (results.Any())
                    {
                        orgIdrefs.Add(nameof(model.CompanyNumber));
                        orgIds.AddRange(results);
                    }
                }

                if (!string.IsNullOrWhiteSpace(model.CharityNumber))
                {
                    results = DataRepository.GetAll<OrganisationReference>()
                        .Where(
                            r => r.ReferenceName.ToLower() == nameof(OrganisationViewModel.CharityNumber).ToLower()
                                 && r.ReferenceValue.ToLower() == model.CharityNumber.ToLower())
                        .Select(r => r.OrganisationId);
                    if (results.Any())
                    {
                        orgIdrefs.Add(nameof(model.CharityNumber));
                        orgIds.AddRange(results);
                    }
                }

                if (!string.IsNullOrWhiteSpace(model.MutualNumber))
                {
                    results = DataRepository.GetAll<OrganisationReference>()
                        .Where(
                            r => r.ReferenceName.ToLower() == nameof(OrganisationViewModel.MutualNumber).ToLower()
                                 && r.ReferenceValue.ToLower() == model.MutualNumber.ToLower())
                        .Select(r => r.OrganisationId);
                    if (results.Any())
                    {
                        orgIdrefs.Add(nameof(model.MutualNumber));
                        orgIds.AddRange(results);
                    }
                }

                if (!string.IsNullOrWhiteSpace(model.OtherName) && !string.IsNullOrWhiteSpace(model.OtherValue))
                {
                    results = DataRepository.GetAll<OrganisationReference>()
                        .Where(
                            r => r.ReferenceName.ToLower() == model.OtherName.ToLower()
                                 && r.ReferenceValue.ToLower() == model.OtherValue.ToLower())
                        .Select(r => r.OrganisationId);
                    if (results.Any())
                    {
                        orgIdrefs.Add(nameof(model.OtherName));
                        orgIds.AddRange(results);
                    }
                }
            }

            model.MatchedReferenceCount = orgIds.Count;

            //Only show orgs matching names when none matching references
            if (model.MatchedReferenceCount == 0)
            {
                string orgName = model.OrganisationName.ToLower().ReplaceI("limited", "").ReplaceI("ltd", "");
                results = DataRepository.GetAll<Organisation>()
                    .Where(o => o.OrganisationName.Contains(orgName))
                    .Select(o => o.OrganisationId);
                if (results.Any())
                {
                    orgIds.AddRange(results);
                }

                results = Organisation.Search(DataRepository.GetAll<Organisation>(), model.OrganisationName, 49, Global.LevenshteinDistance)
                    .Select(o => o.OrganisationId);
                if (results.Any())
                {
                    orgIds.AddRange(results);
                }
            }

            model.ManualRegistration = true;
            model.ManualEmployerIndex = -1;
            model.NameSource = currentUser.EmailAddress;

            if (!orgIds.Any())
            {
                model.AddressReturnAction = nameof(AddOrganisation);
                this.StashModel(model);
                return RedirectToAction("AddAddress", "Register");
            }

            List<Organisation> employers = await DataRepository.GetAll<Organisation>()
                .Where(o => orgIds.Contains(o.OrganisationId))
                .OrderBy(o => o.OrganisationName)
                .ToListAsync();
            model.ManualEmployers = employers.Select(o => o.ToEmployerRecord()).ToList();

            //Ensure exact match shown at top
            if (model.ManualEmployers != null && model.ManualEmployers.Count > 1)
            {
                int index = model.ManualEmployers.FindIndex(e => e.OrganisationName.EqualsI(model.OrganisationName));
                if (index > 0)
                {
                    model.ManualEmployers.Insert(0, model.ManualEmployers[index]);
                    model.ManualEmployers.RemoveAt(index + 1);
                }
            }

            if (model.MatchedReferenceCount == 1)
            {
                model.ManualEmployerIndex = 0;
                this.StashModel(model);
                return await SelectOrganisation(currentUser, model, model.ManualEmployerIndex, nameof(AddOrganisation));
            }

            this.StashModel(model);
            return RedirectToAction("SelectOrganisation");
        }

        #endregion

        #region Select Organisation

        [Authorize]
        [HttpGet("select-organisation")]
        public IActionResult SelectOrganisation()
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

            model.ManualAuthorised = false;
            model.ManualRegistration = true;
            model.ManualAddress = false;
            this.StashModel(model);
            return View("SelectOrganisation", model);
        }

        [PreventDuplicatePost]
        [ValidateAntiForgeryToken]
        [Authorize]
        [HttpPost("select-organisation")]
        public async Task<IActionResult> SelectOrganisation(string command)
        {
            //Ensure user has completed the registration process
            User currentUser;
            IActionResult checkResult = CheckUserRegisteredOk(out currentUser);
            if (checkResult != null)
            {
                return checkResult;
            }

            //Make sure we can load employers from session
            var model = this.UnstashModel<OrganisationViewModel>();
            if (model == null)
            {
                return View("CustomError", new ErrorViewModel(1112));
            }

            if (command.EqualsI("Continue"))
            {
                //Ensure they select one of the matched references
                if (model.MatchedReferenceCount > 0)
                {
                    throw new ArgumentException(nameof(model.MatchedReferenceCount));
                }

                model.ManualRegistration = true;
                model.ManualEmployerIndex = -1;
                model.AddressReturnAction = nameof(SelectOrganisation);
                this.StashModel(model);
                return RedirectToAction("AddAddress", "Register");
            }

            var employerIndex = command.AfterFirst("employer_").ToInt32();

            return await SelectOrganisation(currentUser, model, employerIndex, nameof(SelectOrganisation));
        }

        public async Task<IActionResult> SelectOrganisation(User currentUser,
            OrganisationViewModel model,
            int employerIndex,
            string returnAction)
        {
            if (employerIndex < 0)
            {
                return new HttpBadRequestResult($"Invalid employer index {employerIndex}");
            }

            model.ManualEmployerIndex = employerIndex;
            model.ManualAuthorised = false;

            EmployerRecord employer = model.GetManualEmployer();

            Organisation org = DataRepository.GetAll<Organisation>().FirstOrDefault(o => o.OrganisationId == employer.OrganisationId);

            //Make sure the found organisation is active or pending
            if (org.Status != OrganisationStatuses.Active && org.Status != OrganisationStatuses.Pending)
            {
                _logger.LogWarning(
                    $"Attempt to register a {org.Status} organisation",
                    $"Organisation: '{org.OrganisationName}' Reference: '{org.EmployerReference}' User: '{currentUser.EmailAddress}'");
                return View("CustomError", new ErrorViewModel(1149));
            }

            if (org.SectorType == SectorTypes.Private)
            {
                //Make sure they are fully registered for one before requesting another
                if (currentUser.UserOrganisations.Any() && !currentUser.UserOrganisations.Any(uo => uo.PINConfirmedDate != null))
                {
                    AddModelError(3022);
                    this.CleanModelErrors<OrganisationViewModel>();
                    return View(returnAction, model);
                }
            }


            UserOrganisation userOrg = await DataRepository.GetAll<UserOrganisation>()
                .FirstOrDefaultAsync(uo => uo.OrganisationId == org.OrganisationId && uo.UserId == currentUser.UserId);
            if (userOrg != null)
            {
                AddModelError(userOrg.PINConfirmedDate == null ? 3021 : 3020);
                this.CleanModelErrors<OrganisationViewModel>();
                return View(returnAction, model);
            }

            //If the organisation already exists in DB then use its address and not that from CoHo
            //if (org.LatestAddress != null) employer.ActiveAddressId = org.LatestAddress.AddressId;

            //Make sure the organisation has an address
            if (employer.SectorType == SectorTypes.Public)
            {
                model.ManualAuthorised = employer.IsAuthorised(currentUser.EmailAddress);
                if (!model.ManualAuthorised || !employer.HasAnyAddress())
                {
                    model.ManualAddress = true;
                }
            }
            else if (employer.SectorType == SectorTypes.Private && !employer.HasAnyAddress())
            {
                model.ManualAddress = true;
            }

            model.ManualRegistration = false;
            if (model.ManualAddress)
            {
                model.AddressReturnAction = returnAction;
                this.StashModel(model);
                return RedirectToAction("AddAddress", "Register");
            }

            model.ConfirmReturnAction = returnAction;
            model.AddressSource = null;
            model.Address1 = null;
            model.Address2 = null;
            model.Address3 = null;
            model.City = null;
            model.County = null;
            model.Country = null;
            model.PoBox = null;
            model.Postcode = null;
            model.ContactEmailAddress = null;
            model.ContactFirstName = null;
            model.ContactLastName = null;
            model.ContactJobTitle = null;
            model.ContactPhoneNumber = null;
            model.SicCodeIds = null;

            this.StashModel(model);
            //If private sector add organisation address
            return RedirectToAction(nameof(ConfirmOrganisation));
        }

        #endregion

        #region Confirm Organisation

        /// <summary>
        ///     Show user the confirm organisation view
        /// </summary>
        [Authorize]
        [HttpGet("confirm-organisation")]
        public async Task<IActionResult> ConfirmOrganisation()
        {
            //Ensure user has completed the registration process
            User currentUser;
            IActionResult checkResult = CheckUserRegisteredOk(out currentUser);
            if (checkResult != null)
            {
                return checkResult;
            }

            //Make sure we can load employers from session
            var model = this.UnstashModel<OrganisationViewModel>();
            if (model == null)
            {
                return View("CustomError", new ErrorViewModel(1112));
            }

            //Get the sic codes from companies house
            EmployerRecord employer = null;
            if (!model.ManualRegistration)
            {
                employer = model.GetManualEmployer() ?? model.GetSelectedEmployer();
            }

            #region Get the sic codes if there isnt any

            if (employer != null)
            {
                if (!model.ManualRegistration && string.IsNullOrWhiteSpace(employer.SicCodeIds))
                {
                    employer.SicSource = "CoHo";
                    if (currentUser.EmailAddress.StartsWithI(Global.TestPrefix))
                    {
                        SicCode sic = await DataRepository.GetAll<SicCode>().FirstOrDefaultAsync(s => s.SicSectionId != "X");
                        employer.SicCodeIds = sic?.SicCodeId.ToString();
                    }
                    else
                    {
                        try
                        {
                            if (employer.SectorType == SectorTypes.Public)
                            {
                                employer.SicCodeIds = await PublicSectorRepository.GetSicCodesAsync(employer.CompanyNumber);
                            }
                            else
                            {
                                employer.SicCodeIds = await PrivateSectorRepository.GetSicCodesAsync(employer.CompanyNumber);
                            }

                            employer.SicSource = "CoHo";
                        }
                        catch (Exception ex)
                        {
                            CompaniesHouseFailures++;
                            if (CompaniesHouseFailures < 3)
                            {
                                this.StashModel(model);
                                ModelState.AddModelError(1142);
                                return View(model);
                            }

                            DateTime time = VirtualDateTime.Now;
                            CustomLogger.Error(
                                $"Companies House failed: {nameof(ConfirmOrganisation)}",
                                new
                                {
                                    environment = Config.EnvironmentName,
                                    time,
                                    Exception = ex,
                                    employer.OrganisationName,
                                    employer.CompanyNumber
                                });

                            return View("CustomError", new ErrorViewModel(1140));
                        }

                        CompaniesHouseFailures = 0;
                    }
                }

                model.SicCodeIds = employer.SicCodeIds;
                model.SicSource = employer.SicSource;
            }

            if (!string.IsNullOrWhiteSpace(model.SicCodeIds))
            {
                SortedSet<int> codes = model.GetSicCodeIds();
                if (codes.Count > 0)
                {
                    model.SicCodes = codes.ToList();
                }
            }

            if (employer != null && employer.SectorType == SectorTypes.Public || employer == null && model.SectorType == SectorTypes.Public)
            {
                if (model.SicCodes == null)
                {
                    model.SicCodes = new List<int>();
                }

                if (!model.SicCodes.Any(s => s == 1))
                {
                    model.SicCodes.Insert(0, 1);
                }
            }

            #endregion

            this.StashModel(model);

            #region Populate the view model

            model = model.GetClone();
            if (employer != null)
            {
                model.DateOfCessation = employer.DateOfCessation;
                model.NameSource = employer.NameSource;
                ViewBag.LastOrg = model.OrganisationName;
                model.OrganisationName = employer.OrganisationName;
                model.SectorType = employer.SectorType;
                model.CompanyNumber = employer.CompanyNumber;
                model.CharityNumber = employer.References.ContainsKey(nameof(model.CharityNumber))
                    ? employer.References[nameof(model.CharityNumber)]
                    : null;
                model.MutualNumber = employer.References.ContainsKey(nameof(model.MutualNumber))
                    ? employer.References[nameof(model.MutualNumber)]
                    : null;
                model.OtherName = !string.IsNullOrWhiteSpace(model.OtherName) && employer.References.ContainsKey(model.OtherName)
                    ? model.OtherName
                    : null;
                model.OtherValue = !string.IsNullOrWhiteSpace(model.OtherName) && employer.References.ContainsKey(model.OtherName)
                    ? employer.References[model.OtherName]
                    : null;
                if (!model.ManualAddress)
                {
                    model.AddressSource = employer.AddressSource;
                    model.Address1 = employer.Address1;
                    model.Address2 = employer.Address2;
                    model.Address3 = employer.Address3;
                    model.City = employer.City;
                    model.County = employer.County;
                    model.Country = employer.Country;
                    model.Postcode = employer.PostCode;
                    model.PoBox = employer.PoBox;
                    if (employer.IsUkAddress.HasValue)
                    {
                        model.IsUkAddress = employer.IsUkAddress;
                    }
                    else
                    {
                        model.IsUkAddress = await PostcodesIoApi.IsValidPostcode(employer.PostCode) ? true : (bool?) null;
                    }
                }

                model.SicCodeIds = employer.SicCodeIds;
                model.SicSource = employer.SicSource;
            }

            #endregion

            return View(nameof(ConfirmOrganisation), model);
        }

        /// <summary>
        ///     On confirmation save the organisation
        /// </summary>
        [PreventDuplicatePost]
        [ValidateAntiForgeryToken]
        [Authorize]
        [HttpPost("confirm-organisation")]
        public async Task<IActionResult> ConfirmOrganisation(OrganisationViewModel model, string command = null)
        {
            //Ensure user has completed the registration process
            User currentUser;
            IActionResult checkResult = CheckUserRegisteredOk(out currentUser);
            if (checkResult != null)
            {
                return checkResult;
            }

            //Cancel quick fasttrack
            if (command.EqualsI("CancelFasttrack"))
            {
                PendingFasttrackCodes = null;
                this.ClearStash();
                if (CurrentUser.UserOrganisations.Any())
                {
                    return RedirectToAction(nameof(OrganisationController.ManageOrganisations), "Organisation");
                }

                return RedirectToAction(nameof(OrganisationType));
            }

            #region Load the employers from session

            var m = this.UnstashModel<OrganisationViewModel>();
            if (m == null)
            {
                return View("CustomError", new ErrorViewModel(1112));
            }

            model.Employers = m.Employers;
            model.ManualEmployers = m.ManualEmployers;
            if (!command.EqualsI("confirm"))
            {
                m.AddressReturnAction = nameof(ConfirmOrganisation);
                m.WrongAddress = true;
                m.ManualRegistration = false;
                m.AddressSource = null;
                m.Address1 = null;
                m.Address2 = null;
                m.Address3 = null;
                m.City = null;
                m.County = null;
                m.Country = null;
                m.PoBox = null;
                m.Postcode = null;
                m.IsUkAddress = null;
                m.SectorType = model.SectorType;
                this.StashModel(m);
                return RedirectToAction("AddAddress", "Register");
            }

            #endregion

            //Save the registration
            UserOrganisation userOrg;
            try
            {
                userOrg = await SaveRegistrationAsync(currentUser, model);
            }
            catch (Exception ex)
            {
                //This line is to help diagnose object reference not found exception raised at this point 
                _logger.LogWarning(ex, Extensions.Json.SerializeObjectDisposed(m));
                throw;
            }

            PendingFasttrackCodes = null;

            //Save the model state
            this.StashModel(model);

            //Select the organisation
            ReportingOrganisationId = userOrg.OrganisationId;

            //Remove any previous searches from the cache
            PrivateSectorRepository.ClearSearch();

            var authorised = false;
            var hasAddress = false;
            EmployerRecord employer = null;
            if (!model.ManualRegistration)
            {
                employer = model.GetManualEmployer();
                if (employer != null)
                {
                    authorised = model.ManualAuthorised;
                    hasAddress = employer.HasAnyAddress();
                }
                else
                {
                    employer = model.GetSelectedEmployer();
                    authorised = model.SelectedAuthorised;
                    if (employer != null)
                    {
                        hasAddress = employer.HasAnyAddress();
                    }
                }
            }

            SectorTypes? sector = employer == null ? model.SectorType : employer.SectorType;

            //If manual registration then show confirm receipt
            if (model.ManualRegistration || model.ManualAddress && (sector == SectorTypes.Private || !authorised || hasAddress))
            {
                string reviewCode = Encryption.EncryptQuerystring(
                    userOrg.UserId + ":" + userOrg.OrganisationId + ":" + VirtualDateTime.Now.ToSmallDateTime());

                if (currentUser.EmailAddress.StartsWithI(Global.TestPrefix))
                {
                    TempData["TestUrl"] = Url.Action("ReviewRequest", new {code = reviewCode});
                }

                return RedirectToAction("RequestReceived");
            }

            //If public sector or fasttracked then we are complete
            if (sector == SectorTypes.Public || model.IsFastTrackAuthorised)
            {
                //Log the registration
                if (!userOrg.User.EmailAddress.StartsWithI(Global.TestPrefix))
                {
                    await Global.RegistrationLog.WriteAsync(
                        new RegisterLogModel
                        {
                            StatusDate = VirtualDateTime.Now,
                            Status = "Public sector email confirmed",
                            ActionBy = currentUser.EmailAddress,
                            Details = "",
                            Sector = userOrg.Organisation.SectorType,
                            Organisation = userOrg.Organisation.OrganisationName,
                            CompanyNo = userOrg.Organisation.CompanyNumber,
                            Address = userOrg?.Address.GetAddressString(),
                            SicCodes = userOrg.Organisation.GetSicCodeIdsString(),
                            UserFirstname = userOrg.User.Firstname,
                            UserLastname = userOrg.User.Lastname,
                            UserJobtitle = userOrg.User.JobTitle,
                            UserEmail = userOrg.User.EmailAddress,
                            ContactFirstName = userOrg.User.ContactFirstName,
                            ContactLastName = userOrg.User.ContactLastName,
                            ContactJobTitle = userOrg.User.ContactJobTitle,
                            ContactOrganisation = userOrg.User.ContactOrganisation,
                            ContactPhoneNumber = userOrg.User.ContactPhoneNumber
                        });
                }

                if (model.IsFastTrackAuthorised)
                {
                    //Send notification email to existing users
                    EmailSendingServiceHelpers.SendUserAddedEmailToExistingUsers(userOrg.Organisation, userOrg.User);
                }

                this.StashModel(
                    new CompleteViewModel
                    {
                        OrganisationId = userOrg.OrganisationId, AccountingDate = sector.Value.GetAccountingStartDate()
                    });

                //BUG: the return keyword was missing here so no redirection would occur
                return RedirectToAction("ServiceActivated");
            }


            //If private sector then send the pin, unless Manual Registration feature flag is turned on
            if (!FeatureFlagHelper.IsFeatureEnabled(FeatureFlag.PrivateManualRegistration)
                && model.IsUkAddress.HasValue
                && model.IsUkAddress.Value)
            {
                return RedirectToAction("PINSent");
            }

            return RedirectToAction("RequestReceived");
        }

        #endregion

        /// <summary>
        ///     Save the current users registration
        /// </summary>
        /// <param name="currentUser"></param>
        /// <param name="model"></param>
        private async Task<UserOrganisation> SaveRegistrationAsync(User currentUser, OrganisationViewModel model)
        {
            UserOrganisation userOrg = null;
            var authorised = false;
            var hasAddress = false;
            EmployerRecord employer = null;
            DateTime now = VirtualDateTime.Now;
            if (!model.ManualRegistration)
            {
                employer = model.GetManualEmployer();

                if (employer != null)
                {
                    authorised = model.ManualAuthorised;
                    hasAddress = employer.HasAnyAddress();
                }
                else
                {
                    employer = model.GetSelectedEmployer();
                    authorised = model.SelectedAuthorised;
                    if (employer != null)
                    {
                        hasAddress = employer.HasAnyAddress();
                    }
                }
            }

            Organisation org = employer == null || employer.OrganisationId == 0
                ? null
                : DataRepository.Get<Organisation>(employer.OrganisationId);

            #region Create a new organisation

            var badSicCodes = new SortedSet<int>();
            if (org == null)
            {
                org = new Organisation();
                org.SectorType = employer == null ? model.SectorType.Value : employer.SectorType;
                org.CompanyNumber = employer == null ? model.CompanyNumber : employer.CompanyNumber;
                org.DateOfCessation = employer == null ? model.DateOfCessation : employer.DateOfCessation;
                org.Created = now;
                org.Modified = now;
                org.Status = OrganisationStatuses.New;

                //Create a presumed in-scope for current year
                var newScope = new OrganisationScope
                {
                    Organisation = org,
                    ScopeStatus = ScopeStatuses.PresumedInScope,
                    ScopeStatusDate = now,
                    Status = ScopeRowStatuses.Active,
                    StatusDetails = "Generated by the system",
                    SnapshotDate = org.SectorType.GetAccountingStartDate()
                };
                DataRepository.Insert(newScope);
                org.OrganisationScopes.Add(newScope);

                //Create a presumed out-of-scope for previous year
                var oldScope = new OrganisationScope
                {
                    Organisation = org,
                    ScopeStatus = ScopeStatuses.PresumedOutOfScope,
                    ScopeStatusDate = now,
                    Status = ScopeRowStatuses.Active,
                    StatusDetails = "Generated by the system",
                    SnapshotDate = newScope.SnapshotDate.AddYears(-1)
                };
                DataRepository.Insert(oldScope);
                org.OrganisationScopes.Add(oldScope);

                if (employer == null)
                {
                    OrganisationReference reference;
                    //Add the charity number
                    if (!string.IsNullOrWhiteSpace(model.CharityNumber))
                    {
                        reference = new OrganisationReference
                        {
                            ReferenceName = nameof(model.CharityNumber), ReferenceValue = model.CharityNumber, Organisation = org
                        };
                        DataRepository.Insert(reference);
                        org.OrganisationReferences.Add(reference);
                    }

                    //Add the mutual number
                    if (!string.IsNullOrWhiteSpace(model.MutualNumber))
                    {
                        reference = new OrganisationReference
                        {
                            ReferenceName = nameof(model.MutualNumber), ReferenceValue = model.MutualNumber, Organisation = org
                        };
                        DataRepository.Insert(reference);
                        org.OrganisationReferences.Add(reference);
                    }

                    //Add the other reference 
                    if (!string.IsNullOrWhiteSpace(model.OtherName) && !string.IsNullOrWhiteSpace(model.OtherValue))
                    {
                        reference = new OrganisationReference
                        {
                            ReferenceName = model.OtherName, ReferenceValue = model.OtherValue, Organisation = org
                        };
                        DataRepository.Insert(reference);
                        org.OrganisationReferences.Add(reference);
                    }
                }

                org.SetStatus(
                    authorised && !model.ManualRegistration ? OrganisationStatuses.Active : OrganisationStatuses.Pending,
                    OriginalUser == null ? currentUser.UserId : OriginalUser.UserId);
                DataRepository.Insert(org);
            }

            #endregion

            #region Set Organisation name

            string oldName = org.OrganisationName;
            string newName = null;
            string newNameSource = null;
            if (model.ManualRegistration)
            {
                newName = model.OrganisationName;
                newNameSource = model.NameSource;
            }
            else if (employer != null)
            {
                newName = employer.OrganisationName;
                newNameSource = employer.NameSource;
            }

            if (string.IsNullOrWhiteSpace(newName))
            {
                throw new Exception("Cannot save a registration with no organisation name");
            }

            //Update the new organisation name
            if (string.IsNullOrWhiteSpace(oldName) || !newName.Equals(oldName))
            {
                OrganisationName oldOrgName = org.GetName();

                //Set the latest name if there isnt a name already or new name is from CoHo
                if (oldOrgName == null
                    || oldOrgName?.Name != newName
                    && SourceComparer.IsCoHo(newNameSource)
                    && SourceComparer.CanReplace(newNameSource, oldOrgName.Source))
                {
                    org.OrganisationName = newName;

                    var orgName = new OrganisationName {Name = newName, Source = newNameSource};
                    DataRepository.Insert(orgName);
                    org.OrganisationNames.Add(orgName);
                }
            }

            #endregion

            #region Set Organisation SIC codes

            var newSicCodeIds = new SortedSet<int>();
            string newSicSource = null;

            if (model.ManualRegistration)
            {
                newSicCodeIds = model.GetSicCodeIds();
                newSicSource = model.SicSource;
            }
            else if (employer != null)
            {
                newSicCodeIds = employer.GetSicCodes();
                newSicSource = employer.SicSource;
            }

            if (org.SectorType == SectorTypes.Public)
            {
                newSicCodeIds.Add(1);
            }

            //Remove invalid SicCodes
            if (newSicCodeIds.Any())
            {
                //TODO we should cache these SIC codes
                SortedSet<int> allSicCodes = DataRepository.GetAll<SicCode>().Select(s => s.SicCodeId).ToSortedSet();
                badSicCodes = newSicCodeIds.Except(allSicCodes).ToSortedSet();
                newSicCodeIds = newSicCodeIds.Except(badSicCodes).ToSortedSet();

                //Update the new and retire the old SIC codes
                if (newSicCodeIds.Count > 0)
                {
                    IEnumerable<OrganisationSicCode> oldSicCodes = org.GetSicCodes();
                    SortedSet<int> oldSicCodeIds = org.GetSicCodeIds();

                    //Set the sic codes if there arent any sic codes already or new sic codes are from CoHo
                    if (!oldSicCodes.Any()
                        || !newSicCodeIds.SequenceEqual(oldSicCodeIds)
                        && SourceComparer.IsCoHo(newSicSource)
                        && SourceComparer.CanReplace(newSicSource, oldSicCodes.Select(s => s.Source)))
                    {
                        //Retire the old SicCodes
                        foreach (OrganisationSicCode oldSicCode in oldSicCodes)
                        {
                            oldSicCode.Retired = now;
                        }

                        foreach (int newSicCodeId in newSicCodeIds)
                        {
                            var sicCode = new OrganisationSicCode {Organisation = org, SicCodeId = newSicCodeId, Source = newSicSource};
                            DataRepository.Insert(sicCode);
                            org.OrganisationSicCodes.Add(sicCode);
                        }
                    }
                }
            }

            #endregion

            #region Set the organisation address

            OrganisationAddress oldAddress = org.GetAddress();
            AddressModel oldAddressModel = oldAddress?.GetAddressModel();

            AddressModel newAddressModel = null;
            string oldAddressSource = oldAddress?.Source;
            string newAddressSource = null;

            if (model.ManualRegistration || model.ManualAddress)
            {
                newAddressModel = model.GetAddressModel();
                newAddressSource = model.AddressSource;
            }
            else if (employer != null)
            {
                newAddressModel = employer.GetAddressModel();
                newAddressModel.IsUkAddress = model.IsUkAddress;
                newAddressSource = employer.AddressSource;
            }

            if (newAddressModel == null || newAddressModel.IsEmpty())
            {
                throw new Exception("Cannot save a registration with no address");
            }

            if (oldAddressModel == null || !oldAddressModel.Equals(newAddressModel))
            {
                OrganisationAddress pendingAddress = org.FindAddress(newAddressModel, AddressStatuses.Pending);
                if (pendingAddress != null)
                {
                    oldAddress = pendingAddress;
                    oldAddressModel = pendingAddress.GetAddressModel();
                    oldAddressSource = pendingAddress.Source;
                }
            }


            //Use the old address for this registration
            OrganisationAddress address = oldAddress;

            //If the new address is different...
            if (oldAddressModel == null || oldAddressModel.IsEmpty() || !newAddressModel.Equals(oldAddressModel))
            {
                //Retire the old address
                oldAddress?.SetStatus(AddressStatuses.Retired, OriginalUser == null ? currentUser.UserId : OriginalUser.UserId);

                //Create address received from user
                address = new OrganisationAddress();
                address.Organisation = org;
                address.CreatedByUserId = currentUser.UserId;
                address.Address1 = newAddressModel.Address1;
                address.Address2 = newAddressModel.Address2;
                address.Address3 = newAddressModel.Address3;
                address.TownCity = newAddressModel.City;
                address.County = newAddressModel.County;
                address.Country = newAddressModel.Country;
                address.PostCode = newAddressModel.PostCode;
                address.PoBox = newAddressModel.PoBox;
                address.IsUkAddress = newAddressModel.IsUkAddress;
                address.Source = newAddressSource;
                address.SetStatus(AddressStatuses.Pending, OriginalUser == null ? currentUser.UserId : OriginalUser.UserId);
                DataRepository.Insert(address);
            }

            //This line is to help diagnose object reference not found exception raised at this point 
            if (address == null)
            {
                _logger.LogDebug("Address should not be null", Extensions.Json.SerializeObjectDisposed(model));
            }

            #endregion

            #region add the user org

            userOrg = org.OrganisationId == 0
                ? null
                : await DataRepository.GetAll<UserOrganisation>()
                    .FirstOrDefaultAsync(uo => uo.OrganisationId == org.OrganisationId && uo.UserId == currentUser.UserId);

            if (userOrg == null)
            {
                userOrg = new UserOrganisation {User = currentUser, Organisation = org, Created = now};
                DataRepository.Insert(userOrg);
            }

            //This line is to help diagnose object reference not found exception raised at this point 
            if (address == null)
            {
                _logger.LogWarning("Address should not be null", Extensions.Json.SerializeObjectDisposed(model));
            }

            userOrg.Address = address;
            userOrg.PIN = null;
            userOrg.PINSentDate = null;

            #endregion

            #region Save the contact details

            var sendRequest = false;
            if (FeatureFlagHelper.IsFeatureEnabled(FeatureFlag.PrivateManualRegistration)
                || model.ManualRegistration
                || model.ManualAddress && (org.SectorType == SectorTypes.Private || !authorised || hasAddress)
                || !model.IsUkAddress.HasValue
                || !model.IsUkAddress.Value)
            {
                currentUser.ContactFirstName = model.ContactFirstName;
                currentUser.ContactLastName = model.ContactLastName;
                currentUser.ContactJobTitle = model.ContactJobTitle;
                currentUser.ContactEmailAddress = model.ContactEmailAddress;
                currentUser.ContactPhoneNumber = model.ContactPhoneNumber;
                userOrg.Method = RegistrationMethods.Manual;

                //Send request to GEO
                sendRequest = FeatureFlagHelper.IsFeatureEnabled(FeatureFlag.SendRegistrationReviewEmails);
            }

            #endregion

            #region Activate organisation and address if the user is authorised

            if (authorised && !model.ManualRegistration && (!model.ManualAddress || !hasAddress))
            {
                //Set the user org as confirmed
                userOrg.Method = model.IsFastTrackAuthorised ? RegistrationMethods.Fasttrack : RegistrationMethods.EmailDomain;
                userOrg.ConfirmAttempts = 0;
                userOrg.PINConfirmedDate = now;

                //Set the pending organisation to active
                userOrg.Organisation.SetStatus(
                    OrganisationStatuses.Active,
                    OriginalUser == null ? currentUser.UserId : OriginalUser.UserId,
                    userOrg.Method == RegistrationMethods.Fasttrack ? "Fasttrack" : "Email Domain");

                //Retire the old address 
                OrganisationAddress latestAddress = userOrg.Organisation.GetAddress();
                if (latestAddress != null && latestAddress.AddressId != userOrg.Address.AddressId)
                {
                    latestAddress.SetStatus(
                        AddressStatuses.Retired,
                        OriginalUser == null ? currentUser.UserId : OriginalUser.UserId,
                        "Replaced by PIN in post");
                }

                //Activate the address the pin was sent to
                userOrg.Address.SetStatus(
                    AddressStatuses.Active,
                    OriginalUser == null ? currentUser.UserId : OriginalUser.UserId,
                    userOrg.Method == RegistrationMethods.Fasttrack ? "Fasttrack" : "Email Domain");
            }

            #endregion

            #region Save the changes to the database

            var saved = false;
            UserOrganisation tempUserOrg = userOrg; // Need to use a temporary UserOrg inside a lambda expression for out parameters
            await DataRepository.BeginTransactionAsync(
                async () =>
                {
                    try
                    {
                        await DataRepository.SaveChangesAsync();

                        ScopeBusinessLogic.FillMissingScopes(tempUserOrg.Organisation);

                        await DataRepository.SaveChangesAsync();

                        //Ensure the organisation has an employer reference
                        if (string.IsNullOrWhiteSpace(tempUserOrg.Organisation.EmployerReference))
                        {
                            await OrganisationBusinessLogic.SetUniqueEmployerReferenceAsync(tempUserOrg.Organisation);
                        }

                        DataRepository.CommitTransaction();
                        saved = true;
                    }
                    catch (Exception ex)
                    {
                        DataRepository.RollbackTransaction();
                        sendRequest = false;
                        _logger.LogWarning(ex, Extensions.Json.SerializeObjectDisposed(model));
                        throw;
                    }
                });
            userOrg = tempUserOrg; // Need to return temporary UserOrg inside a lambda expression back to out parameters

            #endregion

            #region Update search indexes, log bad SIC codes and send registration request

            //Add or remove this organisation to/from the search index
            if (saved)
            {
                await SearchBusinessLogic.UpdateSearchIndexAsync(userOrg.Organisation);
            }

            //Log the bad sic codes here to ensure organisation identifiers have been created when saved
            if (badSicCodes.Count > 0)
            {
                //Create the logging tasks
                var badSicLoggingtasks = new List<Task>();
                badSicCodes.ForEach(
                    code => badSicLoggingtasks.Add(
                        Global.BadSicLog.WriteAsync(
                            new BadSicLogModel
                            {
                                OrganisationId = org.OrganisationId,
                                OrganisationName = org.OrganisationName,
                                SicCode = code,
                                Source = "CoHo"
                            })));

                //Wait for all the logging tasks to complete
                await Task.WhenAll(badSicLoggingtasks);
            }

            //Send request to GEO
            if (sendRequest)
            {
                if (model.ManualRegistration)
                {
                    await SendGEORegistrationRequestAsync(
                        userOrg,
                        $"{model.ContactFirstName} {currentUser.ContactLastName} ({currentUser.JobTitle})",
                        org.OrganisationName,
                        address.GetAddressString(),
                        currentUser.EmailAddress.StartsWithI(Global.TestPrefix));
                }
                else
                {
                    await SendGEORegistrationRequestAsync(
                        userOrg,
                        $"{currentUser.Fullname} ({currentUser.JobTitle})",
                        org.OrganisationName,
                        address.GetAddressString(),
                        currentUser.EmailAddress.StartsWithI(Global.TestPrefix));
                }
            }

            return userOrg;
        }

        //Send the registration request
        protected async Task SendGEORegistrationRequestAsync(UserOrganisation userOrg,
            string contactName,
            string reportingOrg,
            string reportingAddress,
            bool test = false)
        {
            //Send a verification link to the email address
            string reviewCode = userOrg.GetReviewCode();
            string reviewUrl = Url.Action("ReviewRequest", "Register", new {code = reviewCode}, "https");

            //If the email address is a test email then simulate sending
            if (userOrg.User.EmailAddress.StartsWithI(Global.TestPrefix))
            {
                return;
            }

            EmailSendingService.SendGeoOrganisationRegistrationRequestEmail(
                Config.GetAppSetting("GEODistributionList"),
                contactName,
                reportingOrg,
                reportingAddress,
                reviewUrl);
        }


        [Authorize]
        [HttpGet("request-received")]
        public IActionResult RequestReceived()
        {
            //Ensure user has completed the registration process
            User currentUser;
            IActionResult checkResult = CheckUserRegisteredOk(out currentUser);
            if (checkResult != null)
            {
                return checkResult;
            }

            //Clear the stash
            this.ClearStash();

            if (currentUser.EmailAddress.StartsWithI(Global.TestPrefix) && TempData.ContainsKey("TestUrl"))
            {
                ViewBag.TestUrl = TempData["TestUrl"];
            }

            return View("RequestReceived");
        }

        #endregion

    }
}
