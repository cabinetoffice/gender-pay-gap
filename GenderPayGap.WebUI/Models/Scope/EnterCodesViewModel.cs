using System;
using System.ComponentModel.DataAnnotations;

namespace GenderPayGap.WebUI.Models.Scope
{

    [Serializable]
    public class EnterCodesViewModel
    {

        [Required(AllowEmptyStrings = false, ErrorMessage = "You must enter an employer reference")]
        [Display(Name = "Enter your employer reference")]
        public string EmployerReference { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "You must enter a security code")]
        [Display(Name = "Enter your security code")]
        public string SecurityToken { get; set; }

    }

}
