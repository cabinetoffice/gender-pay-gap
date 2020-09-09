using System;
using System.Collections.Generic;
using GenderPayGap.Core.Classes;
using GenderPayGap.Database;
using GovUkDesignSystem;
using GovUkDesignSystem.Attributes.ValidationAttributes;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace GenderPayGap.WebUI.Models.AddOrganisation
{
    public class AddOrganisationFoundViewModel : GovUkViewModel
    {

        public string Id { get; set; }
        [BindNever]
        public long DeObfuscatedId
        {
            get => Obfuscator.DeObfuscate(Id);
            set => Id = Obfuscator.Obfuscate((int) value);
        }

        public string CompanyNumber { get; set; }
        public string Query { get; set; }

        // Used to construct the Back link
        [BindNever /* Output Only - only used for sending data from the Controller to the View */]
        public AddOrganisationSector Sector { get; set; }
        [BindNever /* Output Only - only used for sending data from the Controller to the View */]
        public string Name { get; set; }
        [BindNever /* Output Only - only used for sending data from the Controller to the View */]
        public List<string> AddressLines { get; set; }
        [BindNever /* Output Only - only used for sending data from the Controller to the View */]
        public List<SicCode> SicCodes { get; set; }

        [GovUkValidateRequired(ErrorMessageIfMissing = "Select yes if this organisation's address (above) is a UK address")]
        public AddOrganisationIsUkAddress? IsUkAddress { get; set; }

        public bool? GetIsUkAddressAsBoolean()
        {
            switch (IsUkAddress)
            {
                case AddOrganisationIsUkAddress.Yes:
                    return true;
                case AddOrganisationIsUkAddress.No:
                    return false;
                case null:
                    return null;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

    }

}
