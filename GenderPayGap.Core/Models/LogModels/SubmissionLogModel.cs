using System;

namespace GenderPayGap.Core.Models
{
    [Serializable]
    public class SubmissionLogModel
    {

        public DateTime StatusDate { get; set; }
        public ReturnStatuses Status { get; set; }
        public string Details { get; set; }
        public SectorTypes Sector { get; set; }
        public long ReturnId { get; set; }
        public string AccountingDate { get; set; }
        public long OrganisationId { get; set; }
        public string EmployerName { get; set; }
        public string Address { get; set; }
        public string CompanyNumber { get; set; }
        public string SicCodes { get; set; }
        public decimal? DiffMeanHourlyPayPercent { get; set; }
        public decimal? DiffMedianHourlyPercent { get; set; }
        public decimal? DiffMeanBonusPercent { get; set; }
        public decimal? DiffMedianBonusPercent { get; set; }
        public decimal? MaleMedianBonusPayPercent { get; set; }
        public decimal? FemaleMedianBonusPayPercent { get; set; }
        public decimal? MaleLowerPayBand { get; set; }
        public decimal? FemaleLowerPayBand { get; set; }
        public decimal? MaleMiddlePayBand { get; set; }
        public decimal? FemaleMiddlePayBand { get; set; }
        public decimal? MaleUpperPayBand { get; set; }
        public decimal? FemaleUpperPayBand { get; set; }
        public decimal? MaleUpperQuartilePayBand { get; set; }
        public decimal? FemaleUpperQuartilePayBand { get; set; }
        public string CompanyLinkToGPGInfo { get; set; }
        public string ResponsiblePerson { get; set; }
        public string UserFirstname { get; set; }
        public string UserLastname { get; set; }
        public string UserJobtitle { get; set; }
        public string UserEmail { get; set; }
        public string ContactFirstName { get; set; }
        public string ContactLastName { get; set; }
        public string ContactJobTitle { get; set; }
        public string ContactOrganisation { get; set; }
        public string ContactPhoneNumber { get; set; }
        public DateTime Created { get; set; }
        public DateTime Modified { get; set; }
        public string Browser { get; set; }
        public string SessionId { get; set; }

    }
}
