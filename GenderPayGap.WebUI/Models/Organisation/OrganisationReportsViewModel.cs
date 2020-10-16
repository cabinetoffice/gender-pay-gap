using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace GenderPayGap.WebUI.Models.Organisation
{
    public class OrganisationReportsViewModel
    {

        [BindNever]
        public Database.Organisation Organisation { get; set; }
        
        [BindNever]
        public List<int> YearsWithDraftReturns { get; set; }

    }
}
