using System.Collections.Generic;
using Microsoft.Extensions.Configuration;

namespace GenderPayGap.WebJob
{
    //Allows disabling of named webjob methods in DisabledWebjobs section of appsettings.json
    //This only applies to timed-functions
    public class DisableWebjobProvider
    {

        private readonly Dictionary<string, bool> DisabledWebjobsSettings = new Dictionary<string, bool>();

        public DisableWebjobProvider(IConfiguration config)
        {
            config.GetSection("DisabledWebjobs").Bind(DisabledWebjobsSettings);
        }

    }
}
