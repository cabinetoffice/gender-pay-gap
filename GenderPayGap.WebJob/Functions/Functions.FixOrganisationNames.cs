using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GenderPayGap.Core;
using GenderPayGap.Core.Models;
using GenderPayGap.Database;
using GenderPayGap.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace GenderPayGap.WebJob
{
    public partial class Functions
    {

        private async Task FixOrganisationsNamesAsync(ILogger log, string userEmail, string comment)
        {
            if (RunningJobs.Contains(nameof(FixOrganisationsNamesAsync)))
            {
                return;
            }

            RunningJobs.Add(nameof(FixOrganisationsNamesAsync));
            try
            {
                List<Organisation> orgs = await _DataRepository.GetAll<Organisation>().ToListAsync();

                var count = 0;
                var i = 0;
                foreach (Organisation org in orgs)
                {
                    i++;
                    List<OrganisationName> names = org.OrganisationNames.OrderBy(n => n.Created).ToList();

                    var changed = false;
                    ;
                    while (names.Count > 1 && names[1].Name.EqualsI(names[0].Name.Replace(" LTD.", " LIMITED").Replace(" Ltd", " Limited")))
                    {
                        await Global.ManualChangeLog.WriteAsync(
                            new ManualChangeLogModel(
                                nameof(FixOrganisationsNamesAsync),
                                ManualActions.Delete,
                                userEmail,
                                nameof(Organisation.EmployerReference),
                                org.EmployerReference,
                                null,
                                JsonConvert.SerializeObject(
                                    new {names[0].Name, names[0].Source, names[0].Created, names[0].OrganisationId}),
                                null,
                                comment));

                        names[1].Created = names[0].Created;
                        _DataRepository.Delete(names[0]);
                        names.RemoveAt(0);
                        changed = true;
                    }

                    ;
                    if (names.Count > 0)
                    {
                        string newValue = names[names.Count - 1].Name;
                        if (org.OrganisationName != newValue)
                        {
                            org.OrganisationName = newValue;
                            await Global.ManualChangeLog.WriteAsync(
                                new ManualChangeLogModel(
                                    nameof(FixOrganisationsNamesAsync),
                                    ManualActions.Update,
                                    userEmail,
                                    nameof(Organisation.EmployerReference),
                                    org.EmployerReference,
                                    nameof(org.OrganisationName),
                                    org.OrganisationName,
                                    newValue,
                                    comment));
                            changed = true;
                        }
                    }

                    if (changed)
                    {
                        count++;
                        await _DataRepository.SaveChangesAsync();
                    }
                }

                log.LogDebug($"Executed {nameof(FixOrganisationsNamesAsync)} successfully and deleted {count} names");
            }
            finally
            {
                RunningJobs.Remove(nameof(FixOrganisationsNamesAsync));
            }
        }

    }
}
