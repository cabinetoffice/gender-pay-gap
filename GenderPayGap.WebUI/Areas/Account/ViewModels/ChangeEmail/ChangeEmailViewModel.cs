using System;
using System.ComponentModel.DataAnnotations;
using GenderPayGap.WebUI.Areas.Account.Resources;
using GenderPayGap.WebUI.Classes;

namespace GenderPayGap.WebUI.Areas.Account.ViewModels
{

    [Serializable]
    [DefaultResource(typeof(AccountResources))]
    public class ChangeEmailViewModel
    {

        [Required(AllowEmptyStrings = false, ErrorMessageResourceName = nameof(AccountResources.EmailAddressRequired))]
        [EmailAddress]
        [Display(Name = nameof(EmailAddress))]
        public string EmailAddress { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessageResourceName = nameof(AccountResources.ConfirmEmailAddressRequired))]
        [Compare(nameof(EmailAddress), ErrorMessageResourceName = nameof(AccountResources.ConfirmEmailAddressCompare))]
        [Display(Name = nameof(ConfirmEmailAddress))]
        public string ConfirmEmailAddress { get; set; }

    }

}
