using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace GenderPayGap.WebUI.Models.AddOrganisation
{
    public class AddOrganisationFoundViewModel 
    {

        public long? Id { get; set; }

        public string CompanyNumber { get; set; }
        public string Query { get; set; }
        // Used to construct the Back link
        public AddOrganisationSector Sector { get; set; }

        [BindNever /* Output Only - only used for sending data from the Controller to the View */]
        public string Name { get; set; }
        [BindNever /* Output Only - only used for sending data from the Controller to the View */]
        public List<string> AddressLines { get; set; }

        public AddOrganisationIsUkAddress? IsUkAddress { get; set; }

        public bool? GetIsUkAddressAsBoolean()
        {
            switch (IsUkAddress)
            {
                case AddOrganisationIsUkAddress.Yes:
                    return true;
                case AddOrganisationIsUkAddress.No:
                    return false;
                case null:
                    return null;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

    }

}
