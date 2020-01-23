using System;
using System.ComponentModel.DataAnnotations;
using GenderPayGap.WebUI.Areas.Account.Resources;
using GenderPayGap.WebUI.Classes;

namespace GenderPayGap.WebUI.Areas.Account.ViewModels
{

    [Serializable]
    [DefaultResource(typeof(AccountResources))]
    public class ChangeDetailsViewModel
    {

        [Required(AllowEmptyStrings = false, ErrorMessageResourceName = nameof(AccountResources.FirstNameRequired))]
        [Display(Name = nameof(FirstName))]
        public string FirstName { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessageResourceName = nameof(AccountResources.LastNameRequired))]
        [Display(Name = nameof(LastName))]
        public string LastName { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessageResourceName = nameof(AccountResources.JobTitleRequired))]
        [StringLength(50, ErrorMessageResourceName = nameof(AccountResources.JobTitleLength))]
        [Display(Name = nameof(JobTitle))]
        public string JobTitle { get; set; }

        [MaxLength(20, ErrorMessageResourceName = nameof(AccountResources.ContactPhoneNumberLength))]
        [Display(Name = nameof(ContactPhoneNumber))]
        [Phone]
        public string ContactPhoneNumber { get; set; }

        [Display(Name = nameof(SendUpdates))]
        public bool SendUpdates { get; set; }

        [Display(Name = nameof(AllowContact))]
        public bool AllowContact { get; set; }

        public override bool Equals(object obj)
        {
            // Check for null values and compare run-time types.
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            var target = (ChangeDetailsViewModel) obj;

            return target.FirstName == FirstName
                   && target.LastName == LastName
                   && target.JobTitle == JobTitle
                   && target.ContactPhoneNumber == ContactPhoneNumber
                   && target.SendUpdates == SendUpdates
                   && target.AllowContact == AllowContact;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(
                FirstName,
                LastName,
                JobTitle,
                ContactPhoneNumber,
                SendUpdates,
                AllowContact);
        }

    }

}
