using System;
using System.Collections.Generic;
using System.Reflection;
using GenderPayGap.Extensions;
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

        public bool IsDisabled(MethodInfo method)
        {
            //Check using the full name first
            string methodName = method.Name;

            if (!DisabledWebjobsSettings.ContainsKey(method.Name) && method.IsAsyncMethod())
            {
                int i = method.Name.LastIndexOf("Async", StringComparison.OrdinalIgnoreCase);
                if (i > 0)
                {
                    methodName = method.Name.Substring(0, i);
                }
            }

            return DisabledWebjobsSettings.ContainsKey(methodName) ? DisabledWebjobsSettings[methodName] : false;
        }

    }
}
