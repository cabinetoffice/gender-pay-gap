using GenderPayGap.Core;
using GovUkDesignSystem;
using GovUkDesignSystem.Attributes;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace GenderPayGap.WebUI.Models.Report
{
    public class ReportSizeOfOrganisationViewModel 
    {

        [BindNever /* Output Only - only used for sending data from the Controller to the View */]
        public Database.Organisation Organisation { get; set; }
        [BindNever /* Output Only - only used for sending data from the Controller to the View */]
        public int ReportingYear { get; set; }
        [BindNever /* Output Only - only used for sending data from the Controller to the View */]
        public bool IsEditingSubmittedReturn { get; set; }
        public bool IsEditingForTheFirstTime { get; set; }

        public ReportSizeOfOrganisation? SizeOfOrganisation { get; set; }

        public OrganisationSizes? GetSizeOfOrganisation()
        {
            switch (SizeOfOrganisation)
            {
                case ReportSizeOfOrganisation.Employees0To249:
                    return OrganisationSizes.Employees0To249;
                case ReportSizeOfOrganisation.Employees250To499:
                    return OrganisationSizes.Employees250To499;
                case ReportSizeOfOrganisation.Employees500To999:
                    return OrganisationSizes.Employees500To999;
                case ReportSizeOfOrganisation.Employees1000To4999:
                    return OrganisationSizes.Employees1000To4999;
                case ReportSizeOfOrganisation.Employees5000To19999:
                    return OrganisationSizes.Employees5000To19999;
                case ReportSizeOfOrganisation.Employees20000OrMore:
                    return OrganisationSizes.Employees20000OrMore;
                case null:
                default:
                    return null;
            }
        }

        public void SetSizeOfOrganisation(OrganisationSizes? organisationSize)
        {
            switch (organisationSize)
            {
                case OrganisationSizes.Employees0To249:
                    SizeOfOrganisation = ReportSizeOfOrganisation.Employees0To249;
                    break;
                case OrganisationSizes.Employees250To499:
                    SizeOfOrganisation = ReportSizeOfOrganisation.Employees250To499;
                    break;
                case OrganisationSizes.Employees500To999:
                    SizeOfOrganisation = ReportSizeOfOrganisation.Employees500To999;
                    break;
                case OrganisationSizes.Employees1000To4999:
                    SizeOfOrganisation = ReportSizeOfOrganisation.Employees1000To4999;
                    break;
                case OrganisationSizes.Employees5000To19999:
                    SizeOfOrganisation = ReportSizeOfOrganisation.Employees5000To19999;
                    break;
                case OrganisationSizes.Employees20000OrMore:
                    SizeOfOrganisation = ReportSizeOfOrganisation.Employees20000OrMore;
                    break;
                case OrganisationSizes.NotProvided:
                case null:
                default:
                    SizeOfOrganisation = null;
                    break;
            }
        }

    }

    public enum ReportSizeOfOrganisation
    {
        [GovUkRadioCheckboxLabelText(Text = "Less than 250 – I want to report voluntarily")]
        Employees0To249,
        [GovUkRadioCheckboxLabelText(Text = "250 to 499")]
        Employees250To499,
        [GovUkRadioCheckboxLabelText(Text = "500 to 999")]
        Employees500To999,
        [GovUkRadioCheckboxLabelText(Text = "1000 to 4999")]
        Employees1000To4999,
        [GovUkRadioCheckboxLabelText(Text = "5000 to 19,999")]
        Employees5000To19999,
        [GovUkRadioCheckboxLabelText(Text = "20,000 or more")]
        Employees20000OrMore
    }
}
