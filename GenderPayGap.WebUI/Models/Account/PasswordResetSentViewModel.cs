using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace GenderPayGap.WebUI.Models.Account
{
    public class PasswordResetSentViewModel 
    {

        [BindNever]
        public string EmailAddress { get; set; }

    }
}
