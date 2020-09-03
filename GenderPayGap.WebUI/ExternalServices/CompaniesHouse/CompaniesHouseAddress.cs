using Newtonsoft.Json;

namespace GenderPayGap.WebUI.ExternalServices.CompaniesHouse
{
    public class CompaniesHouseAddress
    {

        [JsonProperty("address_line_1")]
        public string AddressLine1 { get; set; }

        [JsonProperty("address_line_2")]
        public string AddressLine2 { get; set; }

        [JsonProperty("care_of")]
        public string CareOf { get; set; }

        [JsonProperty("country")]
        public string Country { get; set; }

        [JsonProperty("locality")]
        public string Locality { get; set; }

        [JsonProperty("po_box")]
        public string PoBox { get; set; }

        [JsonProperty("postal_code")]
        public string PostalCode { get; set; }

        [JsonProperty("premises")]
        public string Premises { get; set; }

        [JsonProperty("region")]
        public string Region { get; set; }

        public string GetAddressLineFromPremisesAndAddressLine1()
        {
            return Premises == null
                ? AddressLine1
                : Premises + "," + AddressLine1;
        }
    }
}
