using System;
using System.ComponentModel.DataAnnotations;
using GenderPayGap.Extensions;
using Newtonsoft.Json;

namespace GenderPayGap.Core.Models
{
    [Serializable]
    public class SicCodeSearchModel
    {

        private string _consolidatedSynonyms;

        [Key]
        public string SicCodeId { get; set; }

        public string SicCodeDescription { get; set; }

        public string[] SicCodeListOfSynonyms { get; set; }

        [JsonIgnore]
        public string ConsolidatedSynonyms
        {
            get => _consolidatedSynonyms;
            set
            {
                _consolidatedSynonyms = value?.Trim();

                if (!string.IsNullOrEmpty(_consolidatedSynonyms))
                {
                    SicCodeListOfSynonyms = _consolidatedSynonyms.SplitI();
                }
            }
        }

        public override bool Equals(object value)
        {
            // Is null?
            if (ReferenceEquals(null, value))
            {
                return false;
            }

            // Is the same object?
            if (ReferenceEquals(this, value))
            {
                return true;
            }

            // Is the same type?
            if (value.GetType() != GetType())
            {
                return false;
            }

            return IsEqual((SicCodeSearchModel) value);
        }

        public bool Equals(SicCodeSearchModel sicCodeSearchModel)
        {
            // Is null?
            if (ReferenceEquals(null, sicCodeSearchModel))
            {
                return false;
            }

            // Is the same object?
            if (ReferenceEquals(this, sicCodeSearchModel))
            {
                return true;
            }

            return IsEqual(sicCodeSearchModel);
        }

        private bool IsEqual(SicCodeSearchModel sicCodeSearchModel)
        {
            return string.Equals(SicCodeId, sicCodeSearchModel.SicCodeId);
        }

        public override int GetHashCode()
        {
            return SicCodeId != null ? SicCodeId.GetHashCode() : 0;
        }

        public static bool operator ==(SicCodeSearchModel sicCodeSearchModelA, SicCodeSearchModel sicCodeSearchModelB)
        {
            if (ReferenceEquals(sicCodeSearchModelA, sicCodeSearchModelB))
            {
                return true;
            }

            // Ensure that source for equality "sicCodeSearchModelA" isn't null
            if (ReferenceEquals(null, sicCodeSearchModelA))
            {
                return false;
            }

            return sicCodeSearchModelA.Equals(sicCodeSearchModelB);
        }

        public static bool operator !=(SicCodeSearchModel sicCodeSearchModelA, SicCodeSearchModel sicCodeSearchModelB)
        {
            return !(sicCodeSearchModelA == sicCodeSearchModelB);
        }

        public string ToLogFriendlyString()
        {
            string sicCodeIdWithoutSpaces = SicCodeId?.Trim();
            string sicCodeDescriptionSubstring = SicCodeDescription?
                .Trim()
                .Substring(
                    0,
                    Math.Min(SicCodeDescription.Trim().Length, 7));

            return $"{sicCodeIdWithoutSpaces}-{sicCodeDescriptionSubstring}";
        }

    }
}
