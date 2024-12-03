using GenderPayGap.Database;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace GenderPayGap.WebUI.Models.Account
{
    public class ManageAccountViewModel 
    {

        [BindNever]
        public User User { get; set; }
        
        [BindNever]
        public bool IsUserBeingImpersonated { get; set; }
        
        [BindNever]
        public bool ShowNewEmail { get; set; }

    }
}
