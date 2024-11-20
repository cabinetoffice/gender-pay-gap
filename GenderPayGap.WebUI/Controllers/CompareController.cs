using GenderPayGap.Core.Interfaces;
using GenderPayGap.Core.Models.HttpResultModels;
using GenderPayGap.Database;
using GenderPayGap.Extensions;
using GenderPayGap.WebUI.Classes.Presentation;
using GenderPayGap.WebUI.Helpers;
using GenderPayGap.WebUI.Models;
using Microsoft.AspNetCore.Mvc;

namespace GenderPayGap.WebUI.Controllers
{
    [Route("viewing")]
    public class CompareController : Controller
    {

        private readonly IDataRepository dataRepository;
        public ICompareViewService CompareViewService { get; }

        public CompareController(ICompareViewService compareViewService,
            IDataRepository dataRepository)
        {
            CompareViewService = compareViewService;
            this.dataRepository = dataRepository;
        }

        [HttpGet("add-employer/{employerIdentifier}")]
        public IActionResult AddEmployer(string employerIdentifier, string returnUrl)
        {
            //Check the parameters are populated
            if (string.IsNullOrWhiteSpace(employerIdentifier))
            {
                return new HttpBadRequestResult($"Missing {nameof(employerIdentifier)}");
            }

            if (string.IsNullOrWhiteSpace(returnUrl))
            {
                return new HttpBadRequestResult($"Missing {nameof(returnUrl)}");
            }

            //Get the employer from the encrypted identifier
            long organisationId = ControllerHelper.DeObfuscateOrganisationIdOrThrow404(employerIdentifier);
            Organisation organisation = ControllerHelper.LoadOrganisationOrThrow404(organisationId, dataRepository);
            ControllerHelper.Throw404IfOrganisationIsNotSearchable(organisation);
            
            // Load the current compared employers from the cookie
            CompareViewService.LoadComparedEmployersFromCookie();

            //Add the employer to the compare list
            CompareViewService.AddToBasket(organisation.GetEncryptedId());

            //Save the compared employers to the cookie
            CompareViewService.SaveComparedEmployersToCookie(Request);

            //Redirect the user to the original page
            return LocalRedirect(returnUrl);
        }

        [HttpGet("add-employer-js/{employerIdentifier}")]
        public IActionResult AddEmployerJs(string employerIdentifier, string returnUrl)
        {
            //Check the parameters are populated
            if (string.IsNullOrWhiteSpace(employerIdentifier))
            {
                return new HttpBadRequestResult($"Missing {nameof(employerIdentifier)}");
            }

            if (string.IsNullOrWhiteSpace(returnUrl))
            {
                return new HttpBadRequestResult($"Missing {nameof(returnUrl)}");
            }

            //Get the employer from the encrypted identifier
            long organisationId = ControllerHelper.DeObfuscateOrganisationIdOrThrow404(employerIdentifier);
            Organisation organisation = ControllerHelper.LoadOrganisationOrThrow404(organisationId, dataRepository);
            ControllerHelper.Throw404IfOrganisationIsNotSearchable(organisation);

            // Load the current compared employers from the cookie
            CompareViewService.LoadComparedEmployersFromCookie();

            //Add the employer to the compare list
            CompareViewService.AddToBasket(organisation.GetEncryptedId());

            //Save the compared employers to the cookie
            CompareViewService.SaveComparedEmployersToCookie(Request);

            //Setup compare basket
            bool fromSearchResults = returnUrl.Contains("/search-results");
            bool fromEmployer = returnUrl.StartsWithI("/employer");
            ViewBag.BasketViewModel = new CompareBasketViewModel {
                CanAddEmployers = false,
                CanClearCompare = true,
                CanViewCompare = fromSearchResults && CompareViewService.BasketItemCount > 1
                                 || fromEmployer && CompareViewService.BasketItemCount > 0,
                IsSearchPage = fromSearchResults,
                IsEmployerPage = fromEmployer
            };

            ViewBag.ReturnUrl = returnUrl;

            var model = new AddRemoveButtonViewModel {
                OrganisationIdEncrypted = organisation.GetEncryptedId(), OrganisationName = organisation.OrganisationName
            };

            return PartialView("Basket_Button", model);
        }

        [HttpGet("remove-employer/{employerIdentifier}")]
        public IActionResult RemoveEmployer(string employerIdentifier, string returnUrl)
        {
            //Check the parameters are populated
            if (string.IsNullOrWhiteSpace(employerIdentifier))
            {
                return new HttpBadRequestResult($"Missing {nameof(employerIdentifier)}");
            }

            if (string.IsNullOrWhiteSpace(returnUrl))
            {
                return new HttpBadRequestResult($"Missing {nameof(returnUrl)}");
            }

            //Get the employer from the encrypted identifier
            long organisationId = ControllerHelper.DeObfuscateOrganisationIdOrThrow404(employerIdentifier);
            Organisation organisation = ControllerHelper.LoadOrganisationOrThrow404(organisationId, dataRepository);
            ControllerHelper.Throw404IfOrganisationIsNotSearchable(organisation);

            // Load the current compared employers from the cookie
            CompareViewService.LoadComparedEmployersFromCookie();

            //Remove the employer from the list
            CompareViewService.RemoveFromBasket(organisation.GetEncryptedId());

            //Save the compared employers to the cookie
            CompareViewService.SaveComparedEmployersToCookie(Request);

            return LocalRedirect(returnUrl);
        }

        [HttpGet("remove-employer-js/{employerIdentifier}")]
        public IActionResult RemoveEmployerJs(string employerIdentifier, string returnUrl)
        {
            //Check the parameters are populated
            if (string.IsNullOrWhiteSpace(employerIdentifier))
            {
                return new HttpBadRequestResult($"Missing {nameof(employerIdentifier)}");
            }

            if (string.IsNullOrWhiteSpace(returnUrl))
            {
                return new HttpBadRequestResult($"Missing {nameof(returnUrl)}");
            }

            //Get the employer from the encrypted identifier
            long organisationId = ControllerHelper.DeObfuscateOrganisationIdOrThrow404(employerIdentifier);
            Organisation organisation = ControllerHelper.LoadOrganisationOrThrow404(organisationId, dataRepository);
            ControllerHelper.Throw404IfOrganisationIsNotSearchable(organisation);

            // Load the current compared employers from the cookie
            CompareViewService.LoadComparedEmployersFromCookie();

            //Remove the employer from the list
            CompareViewService.RemoveFromBasket(organisation.GetEncryptedId());

            //Save the compared employers to the cookie
            CompareViewService.SaveComparedEmployersToCookie(Request);

            //Setup compare basket
            bool fromSearchResults = returnUrl.Contains("/search-results");
            bool fromEmployer = returnUrl.StartsWithI("/employer");
            ViewBag.BasketViewModel = new CompareBasketViewModel {
                CanAddEmployers = false,
                CanClearCompare = true,
                CanViewCompare = fromSearchResults && CompareViewService.BasketItemCount > 1
                                 || fromEmployer && CompareViewService.BasketItemCount > 0,
                IsSearchPage = fromSearchResults,
                IsEmployerPage = fromEmployer
            };

            ViewBag.ReturnUrl = returnUrl;

            var model = new AddRemoveButtonViewModel {
                OrganisationIdEncrypted = organisation.GetEncryptedId(), OrganisationName = organisation.OrganisationName
            };

            return PartialView("Basket_Button", model);
        }

        [HttpGet("clear-employers")]
        public IActionResult ClearEmployers(string returnUrl)
        {
            //Check the parameters are populated
            if (string.IsNullOrWhiteSpace(returnUrl))
            {
                return new HttpBadRequestResult($"Missing {nameof(returnUrl)}");
            }

            // Load the current compared employers from the cookie
            CompareViewService.LoadComparedEmployersFromCookie();

            CompareViewService.ClearBasket();

            //Save the compared employers to the cookie
            CompareViewService.SaveComparedEmployersToCookie(Request);

            return LocalRedirect(returnUrl);
        }

    }
}
