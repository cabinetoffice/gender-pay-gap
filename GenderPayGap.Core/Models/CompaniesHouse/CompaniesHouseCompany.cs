using System.Collections.Generic;
using Newtonsoft.Json;

namespace GenderPayGap.Core.Models.CompaniesHouse
{
    public class CompaniesHouseCompany
    {

        [JsonProperty("company_name")]
        public string CompanyName { get; set; }

        [JsonProperty("company_number")]
        public string CompanyNumber { get; set; }

        [JsonProperty("registered_office_address")]
        public CompaniesHouseAddress RegisteredOfficeAddress { get; set; }

        [JsonProperty("sic_codes")]
        public List<string> SicCodes { get; set; }

    }
}
