using System;

namespace GenderPayGap.Core.Models
{
    public class DownloadResult
    {

        public string EmployerName { get; set; }
        public long EmployerId { get; set; }
        public string Address { get; set; }
        public string PostCode { get; set; }
        public string CompanyNumber { get; set; }
        public string SicCodes { get; set; }
        public decimal DiffMeanHourlyPercent { get; set; }
        public decimal DiffMedianHourlyPercent { get; set; }
        public decimal? DiffMeanBonusPercent { get; set; }
        public decimal? DiffMedianBonusPercent { get; set; }
        public decimal MaleBonusPercent { get; set; }
        public decimal FemaleBonusPercent { get; set; }
        public decimal MaleLowerQuartile { get; set; }
        public decimal FemaleLowerQuartile { get; set; }
        public decimal MaleLowerMiddleQuartile { get; set; }
        public decimal FemaleLowerMiddleQuartile { get; set; }
        public decimal MaleUpperMiddleQuartile { get; set; }
        public decimal FemaleUpperMiddleQuartile { get; set; }
        public decimal MaleTopQuartile { get; set; }
        public decimal FemaleTopQuartile { get; set; }
        public string CompanyLinkToGPGInfo { get; set; }
        public string ResponsiblePerson { get; set; }
        public string EmployerSize { get; set; }
        public string CurrentName { get; set; }
        public bool SubmittedAfterTheDeadline { get; set; }
        public DateTime DueDate { get; set; }
        public DateTime DateSubmitted { get; set; }

    }
}
