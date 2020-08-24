using System.Collections.Generic;
using Newtonsoft.Json;

namespace GenderPayGap.WebUI.ExternalServices.CompaniesHouse
{
    public class CompaniesHouseSearchResult
    {

        [JsonProperty("items")]
        public List<CompaniesHouseSearchResultCompany> Results { get; set; }

    }

    public class CompaniesHouseSearchResultCompany
    {

        [JsonProperty("company_number")]
        public string CompanyNumber { get; set; }

        [JsonProperty("title")]
        public string CompanyName { get; set; }

        [JsonProperty("address_snippet")]
        public string Address { get; set; }

    }
}
