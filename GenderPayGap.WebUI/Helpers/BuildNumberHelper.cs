using System.Reflection;
using Newtonsoft.Json;

namespace GenderPayGap.WebUI.Helpers
{
    public static class BuildNumberHelper
    {
        class BuildNumberObject
        {
            public string BuildNumber { get; set; }
        }


        private static BuildNumberObject cachedBuildNumber;
        
        public static string GetBuildNumber()
        {
            if (cachedBuildNumber == null)
            {
                string executablePath = Assembly.GetEntryAssembly().Location;
                string executableDirectory = Path.GetDirectoryName(executablePath);
                string pathToFile = Path.Combine(executableDirectory, "build-number.json");

                using (StreamReader streamReader = new StreamReader(pathToFile))
                {
                    string buildNumberJson = streamReader.ReadToEnd();
                    cachedBuildNumber = JsonConvert.DeserializeObject<BuildNumberObject>(buildNumberJson);
                }
            }

            return cachedBuildNumber.BuildNumber;
        }

    }
}
