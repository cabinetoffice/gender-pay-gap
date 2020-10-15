using System.Collections.Generic;
using GenderPayGap.Database;
using GovUkDesignSystem;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace GenderPayGap.WebUI.Models.Organisation
{
    public class ManageOrganisationViewModel : GovUkViewModel
    {

        [BindNever]
        public Database.Organisation Organisation { get; set; }
        
        [BindNever]
        public List<int> YearsWithDraftReturns { get; set; }

    }
}
