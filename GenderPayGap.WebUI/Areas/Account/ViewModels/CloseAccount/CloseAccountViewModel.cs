using System;
using System.ComponentModel.DataAnnotations;
using GenderPayGap.WebUI.Areas.Account.Resources;
using GenderPayGap.WebUI.Classes;

namespace GenderPayGap.WebUI.Areas.Account.ViewModels
{

    [Serializable]
    [DefaultResource(typeof(AccountResources))]
    public class CloseAccountViewModel
    {

        public bool IsSoleUserOfOneOrMoreOrganisations { get; set; }

        [DataType(DataType.Password)]
        [Required(AllowEmptyStrings = false, ErrorMessageResourceName = nameof(AccountResources.CloseAccountEnterPasswordRequired))]
        [Display(Name = nameof(AccountResources.CloseAccountEnterPassword))]
        public string EnterPassword { get; set; }

    }

}
