using GovUkDesignSystem.Attributes.ValidationAttributes;

namespace GenderPayGap.WebUI.Models.AccountCreation
{
    public class CreateUserAccountViewModel 
    {

        [GovUkValidateRequired(ErrorMessageIfMissing = "Please enter an email address.")]
        [GovUkValidateCharacterCount(MaxCharacters = 255, NameAtStartOfSentence = "Email address", NameWithinSentence = "email address")]
        public string EmailAddress { get; set; }

        [GovUkValidateRequired(ErrorMessageIfMissing = "Please confirm your email address.")]
        public string ConfirmEmailAddress { get; set; }

        [GovUkValidateCharacterCount(MaxCharacters = 50, NameAtStartOfSentence = "First name", NameWithinSentence = "first name")]
        [GovUkValidateRequired(ErrorMessageIfMissing = "Please enter your first name.")]
        public string FirstName { get; set; }

        [GovUkValidateCharacterCount(MaxCharacters = 50, NameAtStartOfSentence = "Last name", NameWithinSentence = "last name")]
        [GovUkValidateRequired(ErrorMessageIfMissing = "Please enter your last name.")]
        public string LastName { get; set; }

        [GovUkValidateCharacterCount(MaxCharacters = 50, NameAtStartOfSentence = "Job title", NameWithinSentence = "job title")]
        [GovUkValidateRequired(ErrorMessageIfMissing = "Please enter your job title.")]
        public string JobTitle { get; set; }

        [GpgPasswordValidation]
        [GovUkValidateRequired(ErrorMessageIfMissing = "Please enter a password.")]
        public string Password { get; set; }

        [GovUkValidateRequired(ErrorMessageIfMissing = "Please confirm your password.")]
        public string ConfirmPassword { get; set; }

        public bool SendUpdates { get; set; }

        public bool AllowContact { get; set; }

        public bool IsPartOfGovUkReportingJourney { get; set; }

    }
}
