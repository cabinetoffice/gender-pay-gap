using GovUkDesignSystem.Attributes.ValidationAttributes;

namespace GenderPayGap.WebUI.Models.AddOrganisation
{
    public class AddOrganisationChooseSectorViewModel 
    {

        // This is a bool? (nullable) because it makes for better "Back" links
        //   e.g. in Views/AddOrganisation/Search.cshtml, we create a new AddOrganisationChooseSectorViewModel,
        //   but we only want to pass the Sector back to the previous page (we haven't specified a value for Validate)
        public bool? Validate { get; set; }

        [GovUkValidateRequiredIf(IsRequiredPropertyName = nameof(SectorRequired), ErrorMessageIfMissing = "Select which type of employer you would like to add")]
        public AddOrganisationSector? Sector { get; set; }

        public bool SectorRequired => Validate is true;

    }

}
