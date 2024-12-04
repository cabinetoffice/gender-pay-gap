using GenderPayGap.Database;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace GenderPayGap.WebUI.Models.ManageOrganisations
{
    public class ManageOrganisationsViewModel

    {

        [BindNever]
        public IOrderedEnumerable<UserOrganisation> UserOrganisations { get; set; }

    }
}
