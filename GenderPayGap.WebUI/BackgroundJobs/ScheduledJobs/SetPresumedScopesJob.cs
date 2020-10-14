using GenderPayGap.WebUI.BusinessLogic.Services;

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
        public void SetPresumedScopes()
        {
            JobHelpers.RunAndLogJob(SetPresumedScopesAction, nameof(SetPresumedScopes));
        }

        private void SetPresumedScopesAction()
        {
            // Fix any scope statuses (e.g. set the most recent to Active and all others to Retired)
            // TODO: work out whether this is really necessary
            // 1. I'd like to think we're careful enough about the business logic that we won't mess this up
            // 2. Maybe we shouldn't have 2 ways of specifying the most recent scope - maybe just use the latest one, rather than have statuses?
            scopeBusinessLogic.SetScopeStatuses();

            // Find any organisations that have "missing" scopes and fill them in
            // This mainly happens at the start of the new reporting year when this code creates new scopes for all organisations for the new year
            // We create new scopes based on the previous year's scope - e.g. InScope becomes PresumedInScope
            scopeBusinessLogic.SetPresumedScopes();
        }

    }

}
