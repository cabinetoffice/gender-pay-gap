using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using GenderPayGap.Core;
using GenderPayGap.Core.Classes;
using GenderPayGap.Core.Models;
using GenderPayGap.Extensions;
using GenderPayGap.WebUI.Classes;

namespace GenderPayGap.WebUI.Models.Register
{
    [Serializable]
    public class OrganisationViewModel
    {

        public bool PINExpired;
        public bool PINSent;

        public string ConfirmReturnAction { get; set; }
        public string AddressReturnAction { get; set; }

        public bool ManualRegistration { get; set; }
        public bool SelectedAuthorised { get; set; }
        public bool ManualAddress { get; set; }
        public string RegisteredAddress { get; set; }

        public string BackAction { get; set; }

        public string ReviewCode { get; set; }
        public string CancellationReason { get; set; }

        #region Search details

        [Required(AllowEmptyStrings = false)]
        public SectorTypes? SectorType { get; set; }

        [Required]
        [StringLength(
            100,
            ErrorMessage = "You must enter an employers name or company number between 3 and 100 characters in length",
            MinimumLength = 3)]
        [DisplayName("Search")]
        public string SearchText { get; set; }

        #endregion

        #region Organisation details

        [Required(AllowEmptyStrings = false)]
        [StringLength(100, MinimumLength = 3)]
        public string OrganisationName { get; set; }

        public string NameSource { get; set; }

        public DateTime? DateOfCessation { get; set; }

        public bool NoReference { get; set; }

        [Required(AllowEmptyStrings = false)]
        [CompanyNumber]
        public string CompanyNumber { get; set; }

        [Required(AllowEmptyStrings = false)]
        [StringLength(100, MinimumLength = 3)]
        public string CharityNumber { get; set; }

        [Required(AllowEmptyStrings = false)]
        [StringLength(100, MinimumLength = 3)]
        public string MutualNumber { get; set; }

        [Required(AllowEmptyStrings = false)]
        [StringLength(100, MinimumLength = 3)]
        public string OtherName { get; set; }

        [Required(AllowEmptyStrings = false)]
        [StringLength(100, MinimumLength = 3)]
        public string OtherValue { get; set; }


        #endregion

        #region Address details

        [Required(AllowEmptyStrings = false)]
        [MaxLength(100)]
        public string Address1 { get; set; }

        [MaxLength(100)]
        public string Address2 { get; set; }

        public string Address3 { get; set; }
        public string City { get; set; }
        public string County { get; set; }
        public string Country { get; set; }

        [Required(AllowEmptyStrings = false)]
        [StringLength(100, MinimumLength = 3)]
        public string Postcode { get; set; }

        public string PoBox { get; set; }
        public string AddressSource { get; set; }

        public string GetFullAddress()
        {
            var list = new List<string>();
            list.Add(Address1);
            list.Add(Address2);
            list.Add(Address3);
            list.Add(City);
            list.Add(County);
            list.Add(Country);
            list.Add(Postcode);
            list.Add(PoBox);
            return list.ToDelimitedString(", ");
        }

        #endregion

        #region Contact details

        [MaxLength(50)]
        [Required(AllowEmptyStrings = false)]
        public string ContactFirstName { get; set; }

        [MaxLength(50)]
        [Required(AllowEmptyStrings = false)]
        public string ContactLastName { get; set; }

        [MaxLength(50)]
        [Required(AllowEmptyStrings = false)]
        public string ContactJobTitle { get; set; }

        [Required(AllowEmptyStrings = false)]
        [DataType(DataType.EmailAddress)]
        public string ContactEmailAddress { get; set; }

        public string EmailAddress { get; set; }

        [Required(AllowEmptyStrings = false)]
        [Phone]
        [MaxLength(20)]
        public string ContactPhoneNumber { get; set; }

        #endregion

        #region SIC code details

        [Required(AllowEmptyStrings = false)]
        public string SicCodeIds { get; set; }

        public List<int> SicCodes { get; set; }
        public string SicSource { get; set; }

        #endregion

        #region Manual Employers

        public int MatchedReferenceCount { get; set; }

        public List<EmployerRecord> ManualEmployers { get; set; }
        public int ManualEmployerIndex { get; set; }

        public Dictionary<string, string> GetReferences(int i)
        {
            if (ManualEmployers == null || i >= ManualEmployers.Count)
            {
                return null;
            }

            EmployerRecord employer = ManualEmployers[i];
            var results = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            if (!string.IsNullOrWhiteSpace(employer.CompanyNumber))
            {
                results["Company No"] = employer.CompanyNumber;
            }

            foreach (string key in employer.References.Keys)
            {
                if (key.EqualsI(nameof(CharityNumber)))
                {
                    results["Charity No"] = employer.References[nameof(CharityNumber)];
                }
                else if (key.EqualsI(nameof(MutualNumber)))
                {
                    results["Mutual No"] = employer.References[nameof(MutualNumber)];
                }
                else
                {
                    results[key] = employer.References[key];
                }
            }

            return results;
        }

        #endregion

        #region Selected Employer details

        public PagedResult<EmployerRecord> Employers { get; set; }

        public int SelectedEmployerIndex { get; set; }

        #endregion

    }
}
