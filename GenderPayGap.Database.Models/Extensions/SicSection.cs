using System;
using System.Collections.Generic;
using System.Linq;
using GenderPayGap.Core.Interfaces;
using GenderPayGap.Extensions;

namespace GenderPayGap.Database
{
    [Serializable]
    public partial class SicSection
    {

        /// <summary>
        ///     Returns Sector followed by list of SicCodes
        /// </summary>
        /// <param name="dataRepository"></param>
        /// <param name="sicCodes"></param>
        /// <returns></returns>
        public static IEnumerable<string> GetSectors(IDataRepository dataRepository, string sicCodes)
        {
            var results = new SortedDictionary<string, HashSet<long>>(StringComparer.OrdinalIgnoreCase);

            if (string.IsNullOrWhiteSpace(sicCodes))
            {
                yield break;
            }

            foreach (string sicCode in sicCodes.SplitI(@";, \n\r\t"))
            {
                long code = sicCode.ToInt64();
                if (code < 1)
                {
                    continue;
                }

                SicCode sic = dataRepository.GetAll<SicCode>().FirstOrDefault(s => s.SicCodeId == code);
                string sector = sic == null ? "Other" : sic.SicSection.Description;
                HashSet<long> sics = results.ContainsKey(sector) ? results[sector] : new HashSet<long>();
                sics.Add(code);
                results[sector] = sics;
            }

            foreach (string sector in results.Keys)
            {
                if (results[sector].Count == 0)
                {
                    continue;
                }

                yield return $"{sector} ({results[sector].ToDelimitedString(", ")})";
            }
        }

        public override bool Equals(object obj)
        {
            // Check for null values and compare run-time types.
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            var target = (SicSection) obj;
            return SicSectionId == target.SicSectionId;
        }

    }
}
