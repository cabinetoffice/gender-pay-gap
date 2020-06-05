﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GenderPayGap.BusinessLogic;
using GenderPayGap.Database;
using GenderPayGap.Extensions;

namespace GenderPayGap.WebUI.BackgroundJobs.ScheduledJobs
{

    public class SetPresumedScopesJob
    {
        private readonly IScopeBusinessLogic scopeBusinessLogic;

        public SetPresumedScopesJob(
            IScopeBusinessLogic scopeBusinessLogic)
        {
            this.scopeBusinessLogic = scopeBusinessLogic;
        }


        //Set presumed scope of previous years and current years
        public async Task SetPresumedScopes()
        {
            var runId = JobHelpers.CreateRunId();
            var startTime = VirtualDateTime.Now;
            JobHelpers.LogFunctionStart(runId,  nameof(SetPresumedScopes), startTime);
            try
            {
                //Initialise any unknown scope statuses
                HashSet<Organisation> changedOrgs = await scopeBusinessLogic.SetScopeStatusesAsync();

                //Initialise the presumed scoped
                changedOrgs.AddRange(await scopeBusinessLogic.SetPresumedScopesAsync());
                
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
