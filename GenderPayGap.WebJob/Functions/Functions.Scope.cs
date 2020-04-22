﻿using System;
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
            var runId = JobHelpers.CreateRunId();
            var startTime = VirtualDateTime.Now;
            JobHelpers.LogFunctionStart(runId,  nameof(SetPresumedScopes), startTime);
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

                JobHelpers.LogFunctionEnd(runId, nameof(SetPresumedScopes), startTime);
            }
            catch (Exception ex)
            {
                JobHelpers.LogFunctionError(runId, nameof(SetPresumedScopes), startTime, ex );

                //Rethrow the error
                throw;
            }
        }

    }

}
