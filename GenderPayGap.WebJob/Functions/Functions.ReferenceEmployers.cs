using System;
using System.Threading.Tasks;
using GenderPayGap.Extensions;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace GenderPayGap.WebJob
{
    public partial class Functions
    {

        //Ensure all organisations have a unique employer reference
        public async Task ReferenceEmployers([TimerTrigger("01:00:00:00")] TimerInfo timer, ILogger log)
        {
            try
            {
                if (RunningJobs.Contains(nameof(DnBImportAsync)))
                {
                    return;
                }

                await ReferenceEmployersAsync();
                log.LogDebug($"Executed {nameof(ReferenceEmployers)} successfully");
            }
            catch (Exception ex)
            {
                string message = $"Failed webjob ({nameof(ReferenceEmployers)}):{ex.Message}:{ex.GetDetailsText()}";

                //Send Email to GEO reporting errors
                await _Messenger.SendGeoMessageAsync("GPG - WEBJOBS ERROR", message);
                //Rethrow the error
                throw;
            }
        }

        private async Task ReferenceEmployersAsync()
        {
            if (RunningJobs.Contains(nameof(ReferenceEmployers)))
            {
                return;
            }

            RunningJobs.Add(nameof(ReferenceEmployers));

            try
            {
                await _OrganisationBL.SetUniqueEmployerReferencesAsync();
            }
            finally
            {
                RunningJobs.Remove(nameof(ReferenceEmployers));
            }
        }

    }
}
