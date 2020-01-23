using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

namespace GenderPayGap.WebUI.Classes
{

    public class Bundle
    {

        [JsonProperty("outputFileName")]
        public string OutputFileName { get; set; }

        [JsonProperty("inputFiles")]
        public List<string> InputFiles { get; set; } = new List<string>();

        public static Bundle ReadBundleFile(string configFile, string bundlePath)
        {
            var file = new FileInfo(configFile);
            if (!file.Exists)
            {
                return null;
            }

            var bundles = JsonConvert.DeserializeObject<IEnumerable<Bundle>>(File.ReadAllText(configFile));
            return bundles
                .Where(b => b.OutputFileName.EndsWith(bundlePath, StringComparison.InvariantCultureIgnoreCase))
                .FirstOrDefault();
        }

    }


}
