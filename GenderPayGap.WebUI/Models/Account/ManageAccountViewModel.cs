using GenderPayGap.Database;
using GovUkDesignSystem;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace GenderPayGap.WebUI.Models.Account
{
    public class ManageAccountViewModel : GovUkViewModel
    {

        [BindNever]
        public User User { get; set; }
        
        [BindNever]
        public bool IsUserBeingImpersonated { get; set; }
        
        [BindNever]
        public bool ShowNewEmail { get; set; }

    }
}
