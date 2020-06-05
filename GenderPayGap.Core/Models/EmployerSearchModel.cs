using System;
using System.ComponentModel.DataAnnotations;
using GenderPayGap.Extensions;

namespace GenderPayGap.Core.Models
{
    [Serializable]
    public class EmployerSearchModel
    {

        public bool Equals(EmployerSearchModel model)
        {
            return model != null && model.OrganisationId == OrganisationId;
        }

        public override bool Equals(object obj)
        {
            var target = obj as EmployerSearchModel;
            return target != null && target.OrganisationId == OrganisationId;
        }

        public override int GetHashCode()
        {
            return OrganisationId.GetHashCode();
        }

        public string GetEncryptedOrganisionId()
        {
            return Encryption.EncryptQuerystring(OrganisationId);
        }

        #region Organisation Properties

        [Key]

        public string OrganisationId { get; set; }

        public string OrganisationIdEncrypted { get; set; }

        public string Name { get; set; }

        public string PreviousName { get; set; }

        public string PartialNameForSuffixSearches { get; set; }

        public string PartialNameForCompleteTokenSearches { get; set; }

        public string[] Abbreviations { get; set; }

        public int Size { get; set; }

        public string[] SicSectionIds { get; set; }

        public string[] SicSectionNames { get; set; }

        public string[] SicCodeIds { get; set; }

        public string[] SicCodeListOfSynonyms { get; set; }

        public string Address { get; set; }

        public string[] ReportedYears { get; set; }

        public DateTimeOffset LatestReportedDate { get; set; }

        public string[] ReportedLateYears { get; set; }

        public string[] ReportedExplanationYears { get; set; }

        #endregion

    }
}
