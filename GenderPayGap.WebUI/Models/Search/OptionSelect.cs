using System;
using System.Collections.Generic;

namespace GenderPayGap.WebUI.Models.Search
{
    [Serializable]
    public class OptionSelect
    {

        public string Id { get; set; }
        public string Label { get; set; }
        public bool Checked { get; set; }
        public bool Disabled { get; set; }
        public string Value { get; set; }

        public static List<string> GetCheckedString(List<OptionSelect> options)
        {
            var results = new List<string>();
            foreach (OptionSelect item in options)
            {
                if (item.Checked)
                {
                    results.Add(item.Label);
                }
            }

            return results;
        }

    }

}
