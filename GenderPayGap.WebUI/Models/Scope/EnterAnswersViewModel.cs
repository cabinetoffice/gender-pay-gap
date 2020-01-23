using System;
using System.ComponentModel.DataAnnotations;
using GenderPayGap.Extensions;

namespace GenderPayGap.WebUI.Models.Scope
{

    [Serializable]
    public class EnterAnswersViewModel
    {

        [Required(AllowEmptyStrings = false, ErrorMessage = "You must provide a reason for being out of scope")]
        public string Reason { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "You must tell us if you have read the guidance")]
        public string ReadGuidance { get; set; }

        [Display(Name = "Please give a brief explanation in 200 characters or less")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "You must provide your other reason for being out of scope")]
        [MaxLength(200, ErrorMessage = "Your reason can only be 200 characters or less")]
        public string OtherReason { get; set; }

        [Display(Name = "First name")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "Please enter your first name")]
        public string FirstName { get; set; }

        [Display(Name = "Last name")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "Please enter your last name")]
        public string LastName { get; set; }

        [EmailAddress]
        [Display(Name = "Email address")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "Please enter your email address")]
        public string EmailAddress { get; set; }

        [Display(Name = "Confirm your email address")]
        [Compare("EmailAddress", ErrorMessage = "The email addresses do not match")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "Please confirm your email address")]
        public string ConfirmEmailAddress { get; set; }

        public bool HasName => !string.IsNullOrEmpty(FirstName + LastName);

        public string FullName => $"{FirstName} {LastName}";

        public bool? HasReadGuidance()
        {
            if (!string.IsNullOrWhiteSpace(ReadGuidance))
            {
                return ReadGuidance.ToBoolean();
            }

            return null;
        }

    }

}
