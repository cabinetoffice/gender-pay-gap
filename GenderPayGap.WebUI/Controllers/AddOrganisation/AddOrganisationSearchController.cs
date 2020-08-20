using System.Collections.Generic;
using GenderPayGap.Core;
using GenderPayGap.Core.Interfaces;
using GenderPayGap.WebUI.Helpers;
using GenderPayGap.WebUI.Models.AddOrganisation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GenderPayGap.WebUI.Controllers.AddOrganisation
{
    [Authorize(Roles = LoginRoles.GpgEmployer)]
    [Route("add-organisation")]
    public class AddOrganisationSearchController : Controller
    {

        private readonly IDataRepository dataRepository;

        public AddOrganisationSearchController(IDataRepository dataRepository)
        {
            this.dataRepository = dataRepository;
        }


        [HttpGet("{sector}/search")]
        public IActionResult Search(AddOrganisationSearchViewModel viewModel)
        {
            ControllerHelper.Throw404IfFeatureDisabled(FeatureFlag.NewAddOrganisationJourney);

            ControllerHelper.ThrowIfUserAccountRetiredOrEmailNotVerified(User, dataRepository);

            if (!string.IsNullOrWhiteSpace(viewModel.Query))
            {
                if (viewModel.Query.Contains("softwire"))
                {
                    viewModel.SearchResults = ThreeSoftwires();
                }
                else if (viewModel.Query == "limited")
                {
                    viewModel.SearchResults = LotsOfCompanies();
                    viewModel.TooManyResults = true;
                }
                else
                {
                    // An empty list of results means "no results"
                    // A NULL list of results means "you haven't searched yet"
                    viewModel.SearchResults = new List<AddOrganisationSearchResult>();
                }

                return View("Search", viewModel);
            }

            return View("Search", viewModel);
        }

        private static List<AddOrganisationSearchResult> ThreeSoftwires()
        {
            return new List<AddOrganisationSearchResult>
            {
                new AddOrganisationSearchResult
                {
                    EncryptedOrganisationId = "111",
                    OrganisationName = "SOFTWIRE TECHNOLOGY LIMITED",
                    OrganisationAddress = "1st Floor Gallery Court, 28 Arcadia Avenue, London, N3 2FG",
                    Identifiers = new List<AddOrganisationSearchResultOrganisationIdentifier>
                    {
                        new AddOrganisationSearchResultOrganisationIdentifier
                        {
                            IdentifierType = "Company number", Identifier = "03824658"
                        }
                    }
                },
                new AddOrganisationSearchResult
                {
                    EncryptedOrganisationId = "222",
                    OrganisationName = "WITHANI LIMITED",
                    OrganisationAddress = "1st Floor Gallery Court, 28 Arcadia Avenue, London, N3 2FG",
                    Identifiers = new List<AddOrganisationSearchResultOrganisationIdentifier>
                    {
                        new AddOrganisationSearchResultOrganisationIdentifier
                        {
                            IdentifierType = "Company number", Identifier = "12430617"
                        }
                    }
                },
                new AddOrganisationSearchResult
                {
                    CompanyNumber = "09304061",
                    OrganisationName = "GHYSTON LIMITED",
                    OrganisationAddress = "Colston Tower, Colston Street, Bristol, BS1 4XE",
                    Identifiers = new List<AddOrganisationSearchResultOrganisationIdentifier>
                    {
                        new AddOrganisationSearchResultOrganisationIdentifier
                        {
                            IdentifierType = "Company number", Identifier = "09304061"
                        }
                    }
                },
            };
        }

        private List<AddOrganisationSearchResult> LotsOfCompanies()
        {
            var organisations = new List<AddOrganisationSearchResult>
            {
                new AddOrganisationSearchResult
                {
                    EncryptedOrganisationId = "444",
                    OrganisationName = "\"RED BAND\" CHEMICAL COMPANY, LIMITED",
                    OrganisationAddress = "19 Smith's Place, Leith Walk, Edinburgh, EH6 8NU",
                    Identifiers = new List<AddOrganisationSearchResultOrganisationIdentifier>
                    {
                        new AddOrganisationSearchResultOrganisationIdentifier
                        {
                            IdentifierType = "Company number", Identifier = "SC016876"
                        }
                    }
                },
                new AddOrganisationSearchResult
                {
                    CompanyNumber = "00266909",
                    OrganisationName = "00266909 LIMITED",
                    OrganisationAddress = "2-3 Pavilion Building, Brighton, BN1 1EE",
                    Identifiers = new List<AddOrganisationSearchResultOrganisationIdentifier>
                    {
                        new AddOrganisationSearchResultOrganisationIdentifier
                        {
                            IdentifierType = "Company number", Identifier = "00266909"
                        }
                    }
                },
                new AddOrganisationSearchResult
                {
                    EncryptedOrganisationId = "666",
                    OrganisationName = "118 LIMITED",
                    OrganisationAddress = "3 Alexandra Gate Ffordd Pengam, Ground Floor, Cardiff, Wales, CF24 2SA",
                    Identifiers = new List<AddOrganisationSearchResultOrganisationIdentifier>
                    {
                        new AddOrganisationSearchResultOrganisationIdentifier
                        {
                            IdentifierType = "Mutual partnership number", Identifier = "03951948"
                        }
                    }
                },
            };

            var allOrgs = new List<AddOrganisationSearchResult>();

            for (int i = 0; i < 100; i++)
            {
                allOrgs.AddRange(organisations);
            }

            return allOrgs;
        }

    }
}
