using GenderPayGap.Database;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace GenderPayGap.WebUI.Models.AddOrganisation
{
    public class AddOrganisationAlreadyRegisteringViewModel
    {

        [BindNever /* Output Only - only used for sending data from the Controller to the View */]
        public long Id { get; set; }

        [BindNever /* Output Only - only used for sending data from the Controller to the View */]
        public string Query { get; set; }
        
        [BindNever /* Output Only - only used for sending data from the Controller to the View */]
        public AddOrganisationSector Sector { get; set; }
        
        [BindNever /* Output Only - only used for sending data from the Controller to the View */]
        public UserOrganisation ExistingUserOrganisation { get; set; }

    }
}
