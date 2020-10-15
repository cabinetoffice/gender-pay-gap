using System.Linq;
using GenderPayGap.Database;
using GovUkDesignSystem;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace GenderPayGap.WebUI.Models.Organisation
{
    public class ManageOrganisationsViewModel : GovUkViewModel

    {

        [BindNever]
        public IOrderedEnumerable<UserOrganisation> UserOrganisations { get; set; }

    }
}
