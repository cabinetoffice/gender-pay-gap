using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GenderPayGap.Core;
using GenderPayGap.Database;
using GenderPayGap.Extensions;
using Microsoft.Azure.WebJobs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GenderPayGap.WebJob
{
    public partial class Functions
    {

        /// <summary>
        ///     Ensures only latest organisation address is active one
        /// </summary>
        public async Task OrganisationAddresses_Fix([TimerTrigger(typeof(OncePerWeekendRandomSchedule))]
            TimerInfo timer,
            ILogger log)
        {
            try
            {
                //Load all the organisation in the database
                List<Organisation> orgs = await _DataRepository.GetAll<Organisation>().ToListAsync();
                var count = 0;

                //Loop through each organisation in the list
                foreach (Organisation org in orgs)
                {
                    //Get all the active or retired addresses for this organisation sorted by when they are thoughted to have been first activated
                    List<OrganisationAddress> addresses = org.OrganisationAddresses
                        .Where(a => a.Status == AddressStatuses.Active || a.Status == AddressStatuses.Retired)
                        .OrderBy(a => a.GetFirstRegisteredDate())
                        .ToList();

                    var changed = false;
                    for (var i = 0; i < addresses.Count; i++)
                    {
                        //Get the current address
                        OrganisationAddress address = addresses[i];

                        //Get the next address if there is one
                        OrganisationAddress nextAddress = i + 1 < addresses.Count ? addresses[i + 1] : null;

                        //If current address is a previous address and its status is active
                        if (nextAddress != null && address.Status == AddressStatuses.Active)
                        {
                            //If its replacement address has no creator id
                            if (nextAddress.CreatedByUserId < 1)
                            {
                                //Lookup the status when the replacement address was last activated
                                AddressStatus activeStatus = nextAddress.AddressStatuses.OrderByDescending(st => st.StatusDate)
                                    .FirstOrDefault(st => st.Status == AddressStatuses.Active);
                                //Set the creator id of the replacement address to that saved with its activation status
                                nextAddress.CreatedByUserId = activeStatus.ByUserId;
                            }

                            //Retire the current address address
                            address.SetStatus(
                                AddressStatuses.Retired,
                                nextAddress.CreatedByUserId,
                                $"Replaced by {nextAddress.Source}",
                                nextAddress.StatusDate);
                            changed = true;
                        }
                    }

                    //Get the actual latest active address
                    OrganisationAddress latestAddress = addresses.LastOrDefault(a => a.Status == AddressStatuses.Active);

                    //If the LatestAddress of the organisation is wrong then fix it
                    if (latestAddress != null && (org.LatestAddress == null || org.LatestAddress.AddressId != latestAddress.AddressId))
                    {
                        org.LatestAddress = latestAddress;
                        changed = true;
                    }

                    //If there were database changes then save them
                    if (changed)
                    {
                        await _DataRepository.SaveChangesAsync();
                    }

                    count++;
                }
            }
            catch (Exception ex)
            {
                string message = $"Failed webjob ({nameof(OrganisationAddresses_Fix)}):{ex.Message}:{ex.GetDetailsText()}";
                log.LogError(ex, $"Failed webjob ({nameof(OrganisationAddresses_Fix)})");

                //Send Email to GEO reporting errors
                await _Messenger.SendGeoMessageAsync("GPG - WEBJOBS ERROR", message);
                //Rethrow the error
                throw;
            }
        }

        /// <summary>
        ///     Once per weekend execute healthchecks on database and file storage and report faults to logs and email to GEO
        ///     admins
        /// </summary>
        [Disable]
        public async Task Storage_HealthCheck([TimerTrigger(typeof(OncePerWeekendRandomSchedule))]
            TimerInfo timer,
            ILogger log)
        {
            try
            {
                //TODO: Check for any records with future date/times in when Tardis is zero

                //TODO: Check for organisations with multiple active addresses

                //TODO: Check for consecutive duplicate addresses from same source

                //TODO: Check for organisations with multiple active scopes per year

                //TODO: Check for active organisations with no active scopes for every year

                //TODO: Check address status matches latest status

                //TODO: Check organisation status matches latest status

                //TODO: Check organisation name matches latest organisation name

                //TODO: Check organisation latest address is correct

                //TODO: Check organisation latest return is correct

                //TODO: Check organisation latest scope is correct

                //TODO: Check organisation latest registration is correct

                //TODO: Check organisation latest public sector type is correct

                //TODO: Check scope snapshot dates for org sectors

                //TODO: Check return accounting dates for org sectors

                //TODO: Check for orgs with same name are not active

                //TODO: Check organisations with same company number 

                //TODO: Check organisations with same DUNS number 

                //TODO: Check organisations with same EmployerRef 

                //TODO: Check organisations with same OrganisationReferences

                //TODO: Check return status matches latest status

                //TODO: Check for multiple active returns per year

                //TODO: Check user status matches latest status

                //TODO: Check users with too many login attempts 
            }
            catch (Exception ex)
            {
                string message = $"Failed webjob ({nameof(Storage_HealthCheck)}):{ex.Message}:{ex.GetDetailsText()}";
                log.LogError(ex, $"Failed webjob ({nameof(Storage_HealthCheck)})");

                //Send Email to GEO reporting errors
                await _Messenger.SendGeoMessageAsync("GPG - WEBJOBS ERROR", message);
                //Rethrow the error
                throw;
            }
        }

    }
}
