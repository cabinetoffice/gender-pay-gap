using System;
using System.ComponentModel.DataAnnotations;
using GenderPayGap.Extensions;
using Microsoft.Azure.Search;
using Microsoft.Azure.Search.Models;

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

        [IsSearchable]
        [Analyzer(AnalyzerName.AsString.EnLucene)]
        [IsFilterable]
        [IsSortable]
        public string Name { get; set; }

        [IsSearchable]
        [Analyzer(AnalyzerName.AsString.EnLucene)]
        public string PreviousName { get; set; }

        [IsSearchable]
        public string PartialNameForSuffixSearches { get; set; }

        [IsSearchable]
        public string PartialNameForCompleteTokenSearches { get; set; }

        [IsSearchable]
        [Analyzer(AnalyzerName.AsString.EnLucene)]
        public string[] Abbreviations { get; set; }

        [IsFilterable]
        [IsSortable]
        [IsFacetable]
        public int Size { get; set; }

        [IsFilterable]
        [IsFacetable]
        public string[] SicSectionIds { get; set; }

        public string[] SicSectionNames { get; set; }

        [IsSearchable]
        [IsFilterable]
        [IsFacetable]
        public string[] SicCodeIds { get; set; }

        [IsSearchable]
        public string[] SicCodeListOfSynonyms { get; set; }

        public string Address { get; set; }

        [IsFilterable]
        [IsFacetable]
        public string[] ReportedYears { get; set; }

        [IsFilterable]
        [IsFacetable]
        public DateTimeOffset LatestReportedDate { get; set; }

        [IsFilterable]
        [IsFacetable]
        public string[] ReportedLateYears { get; set; }

        [IsFilterable]
        [IsFacetable]
        public string[] ReportedExplanationYears { get; set; }

        #endregion

    }
}
