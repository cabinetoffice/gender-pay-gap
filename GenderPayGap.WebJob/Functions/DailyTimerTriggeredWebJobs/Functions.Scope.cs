using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GenderPayGap.Database;
using GenderPayGap.Extensions;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace GenderPayGap.WebJob
{

    public partial class Functions
    {

        //Set presumed scope of previous years and current years
        public async Task SetPresumedScopes([TimerTrigger("50 4 * * *" /* 04:50 once per day */)]
            TimerInfo timer,
            ILogger log)
        {
            var runId = CreateRunId();
            var startTime = VirtualDateTime.Now;
            LogFunctionStart(runId,  nameof(SetPresumedScopes), startTime);
            try
            {
                //Initialise any unknown scope statuses
                HashSet<Organisation> changedOrgs = await _ScopeBL.SetScopeStatusesAsync();

                //Initialise the presumed scoped
                changedOrgs.AddRange(await _ScopeBL.SetPresumedScopesAsync());

                //Update the search indexes
                if (changedOrgs.Count > 0)
                {
                    await _SearchBusinessLogic.UpdateSearchIndexAsync(changedOrgs.ToArray());
                }

                LogFunctionEnd(runId, nameof(SetPresumedScopes), startTime);
            }
            catch (Exception ex)
            {
                LogFunctionError(runId, nameof(SetPresumedScopes), startTime, ex );

                //Send Email to GEO reporting errors
                await _Messenger.SendGeoMessageAsync("GPG - WEBJOBS ERROR", 
                    $"Failed webjob ({nameof(SetPresumedScopes)}):{ex.Message}:{ex.GetDetailsText()}");
                //Rethrow the error
                throw;
            }
        }

    }

}
