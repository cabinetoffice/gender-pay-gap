using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using GovUkDesignSystem;
using GovUkDesignSystem.Attributes;
using GovUkDesignSystem.Attributes.ValidationAttributes;

namespace GenderPayGap.WebUI.Models.AccountCreation
{
    public class CreateUserAccountViewModel : GovUkViewModel
    {
        [GovUkValidateRequired(
            ErrorMessageIfMissing = "Please enter an email address."
        )]
        public string EmailAddress { get; set; }   
        
        [GovUkValidateRequired(
            ErrorMessageIfMissing = "Please confirm your email address."
        )]
        public string ConfirmEmailAddress { get; set; }   
        
        [GovUkValidateRequired(
            ErrorMessageIfMissing = "Please enter your first name."
        )]
        public string FirstName { get; set; }  
        
        [GovUkValidateRequired(
            ErrorMessageIfMissing = "Please enter your last name."
        )]
        public string LastName { get; set; }  
        
        [GovUkDisplayNameForErrors(
            NameAtStartOfSentence = "Job title",
            NameWithinSentence = "job title"
        )]
        [GovUkValidateCharacterCount(
            MaxCharacters = 50
        )]
        [GovUkValidateRequired(
            ErrorMessageIfMissing = "Please enter your job title."
        )]
        public string JobTitle { get; set; }  
        
        [GpgPasswordValidationAttribute]
        [GovUkValidateRequired(
            ErrorMessageIfMissing = "Please enter a password."
        )]
        public string Password { get; set; }
        [GovUkValidateRequired(
            ErrorMessageIfMissing = "Please confirm your password."
        )]
        public string ConfirmPassword { get; set; }  
        
        public bool SendUpdates { get; set; }
        
        public bool AllowContact { get; set; }

    }
}
