using GovUkDesignSystem;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace GenderPayGap.WebUI.Models.Account
{
    public class PasswordResetSentViewModel : GovUkViewModel
    {

        [BindNever]
        public string EmailAddress { get; set; }

    }
}
