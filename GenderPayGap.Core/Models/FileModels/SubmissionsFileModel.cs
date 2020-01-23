using System;

namespace GenderPayGap.Core.Models
{
    public class SubmissionsFileModel
    {

        public long ReturnId { get; set; }
        public long OrganisationId { get; set; }
        public string OrganisationName { get; set; }
        public string DUNSNumber { get; set; }
        public string EmployerReference { get; set; }
        public string CompanyNumber { get; set; }
        public SectorTypes SectorType { get; set; }
        public ScopeStatuses? ScopeStatus { get; set; }
        public DateTime? ScopeStatusDate { get; set; }
        public DateTime AccountingDate { get; set; }
        public DateTime ModifiedDate { get; set; }
        public decimal? DiffMeanBonusPercent { get; set; }
        public decimal DiffMeanHourlyPayPercent { get; set; }
        public decimal? DiffMedianBonusPercent { get; set; }
        public decimal DiffMedianHourlyPercent { get; set; }
        public decimal FemaleLowerPayBand { get; set; }
        public decimal FemaleMedianBonusPayPercent { get; set; }
        public decimal FemaleMiddlePayBand { get; set; }
        public decimal FemaleUpperPayBand { get; set; }
        public decimal FemaleUpperQuartilePayBand { get; set; }
        public decimal MaleLowerPayBand { get; set; }
        public decimal MaleMedianBonusPayPercent { get; set; }
        public decimal MaleMiddlePayBand { get; set; }
        public decimal MaleUpperPayBand { get; set; }
        public decimal MaleUpperQuartilePayBand { get; set; }
        public string CompanyLink { get; set; }
        public string ResponsiblePerson { get; set; }
        public string OrganisationSize { get; set; }
        public string Modifications { get; set; }
        public bool EHRCResponse { get; set; }

    }
}
