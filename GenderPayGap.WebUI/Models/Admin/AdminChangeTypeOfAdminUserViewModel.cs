using GenderPayGap.Database;
using GovUkDesignSystem.ModelBinders;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace GenderPayGap.WebUI.Models.Admin
{
    public class AdminChangeTypeOfAdminUserViewModel 
    {

        [BindNever /* Output Only - only used for sending data from the Controller to the View */]
        public User User { get; set; }

        [ModelBinder(typeof(GovUkCheckboxBoolBinder))]
        public bool ReadOnly { get; set; }

    }
}
