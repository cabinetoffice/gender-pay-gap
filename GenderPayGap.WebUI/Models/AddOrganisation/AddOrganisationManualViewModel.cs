using GovUkDesignSystem;
using GovUkDesignSystem.Attributes.ValidationAttributes;

namespace GenderPayGap.WebUI.Models.AddOrganisation
{
    public class AddOrganisationManualViewModel : GovUkViewModel
    {

        // This is a bool? (nullable) because it makes for better "Back" links
        //   e.g. in Views/AddOrganisation/Search.cshtml, we create a new AddOrganisationChooseSectorViewModel,
        //   but we only want to pass the Sector back to the previous page (we haven't specified a value for Validate)
        public bool? Validate { get; set; }

        [GovUkValidateRequired(ErrorMessageIfMissing = "Choose which type of organisation you would like to add")]
        public AddOrganisationSector? Sector { get; set; }

        public string Query { get; set; }

        [GovUkValidateRequired(ErrorMessageIfMissing = "Enter the name of the organisation")]
        public string OrganisationName { get; set; }

        public string PoBox { get; set; }
        [GovUkValidateRequired(ErrorMessageIfMissing = "Enter the registered address of the organisation")]
        public string Address1 { get; set; }
        public string Address2 { get; set; }
        public string Address3 { get; set; }
        public string TownCity { get; set; }
        public string County { get; set; }
        public string Country { get; set; }
        public string PostCode { get; set; }
        [GovUkValidateRequired(ErrorMessageIfMissing = "Select yes if this organisation's address (above) is a UK address")]
        public AddOrganisationIsUkAddress? IsUkAddress { get; set; }


    }
}
