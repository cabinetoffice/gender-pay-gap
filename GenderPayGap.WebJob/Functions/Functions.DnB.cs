using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using GenderPayGap.Core;
using GenderPayGap.Core.Classes;
using GenderPayGap.Core.Models;
using GenderPayGap.Database;
using GenderPayGap.Extensions;
using GenderPayGap.Extensions.AspNetCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GenderPayGap.WebJob
{
    public partial class Functions
    {

        public async Task DnBImportAsync(ILogger log, long currentUserId)
        {
            if (RunningJobs.Contains(nameof(CompaniesHouseCheck)) || RunningJobs.Contains(nameof(DnBImportAsync)))
            {
                return;
            }

            RunningJobs.Add(nameof(DnBImportAsync));
            string userEmail = null;
            string error = null;
            DateTime startTime = VirtualDateTime.Now;
            var totalChanges = 0;
            var totalInserts = 0;

            try
            {
                #region Load and Prechecks

                //Load the D&B records
                IEnumerable<string> dnbOrgsPaths = await Global.FileRepository.GetFilesAsync(Global.DataPath, Filenames.DnBOrganisations);
                string dnbOrgsPath = dnbOrgsPaths.OrderByDescending(f => f).FirstOrDefault();
                if (string.IsNullOrEmpty(dnbOrgsPath))
                {
                    return;
                }

                if (!await Global.FileRepository.GetFileExistsAsync(dnbOrgsPath))
                {
                    throw new Exception("Could not find " + dnbOrgsPath);
                }

                List<DnBOrgsModel> AllDnBOrgs = await Global.FileRepository.ReadCSVAsync<DnBOrgsModel>(dnbOrgsPath);

                if (!AllDnBOrgs.Any())
                {
                    log.LogWarning($"No records found in '{dnbOrgsPath}'");
                    return;
                }

                AllDnBOrgs = AllDnBOrgs.OrderBy(o => o.OrganisationName).ToList();

                //Check for duplicate DUNS
                int count = AllDnBOrgs.Count() - AllDnBOrgs.Select(o => o.DUNSNumber).Distinct().Count();
                if (count > 0)
                {
                    throw new Exception($"There are {count} duplicate DUNS numbers detected");
                }

                //Check for no addresses
                count = AllDnBOrgs.Count(o => !o.IsValidAddress());
                if (count > 0)
                {
                    throw new Exception(
                        $"There are {count} organisations with no address detected (i.e., no AddressLine1, AddressLine2, PostalCode, and PoBox).");
                }

                //Check for no organisation name
                count = AllDnBOrgs.Count(o => string.IsNullOrWhiteSpace(o.OrganisationName));
                if (count > 0)
                {
                    throw new Exception($"There are {count} organisations with no OrganisationName detected.");
                }

                //Check for duplicate employer references
                int allEmployerReferenceCount = AllDnBOrgs.Count(o => !string.IsNullOrWhiteSpace(o.EmployerReference));
                var employerReferences = new SortedSet<string>(
                    AllDnBOrgs.Where(o => !string.IsNullOrWhiteSpace(o.EmployerReference)).Select(o => o.EmployerReference).Distinct());
                count = allEmployerReferenceCount - employerReferences.Count;
                if (count > 0)
                {
                    throw new Exception($"There are {count} duplicate EmployerReferences detected");
                }

                //Check companies have been updated
                count = AllDnBOrgs.Count(
                    o => !string.IsNullOrWhiteSpace(o.CompanyNumber)
                         && o.DateOfCessation == null
                         && (o.StatusCheckedDate == null || o.StatusCheckedDate.Value.AddMonths(1) < VirtualDateTime.Now));
                if (count > 0)
                {
                    throw new Exception(
                        $"There are {count} active companies who have not been checked with companies house within the last month");
                }

                //Fix Company Number
                Parallel.ForEach(
                    AllDnBOrgs.Where(o => !string.IsNullOrWhiteSpace(o.CompanyNumber)),
                    dnbOrg => {
                        if (dnbOrg.CompanyNumber.IsNumber())
                        {
                            dnbOrg.CompanyNumber = dnbOrg.CompanyNumber.PadLeft(8, '0');
                        }
                    });

                //Check for duplicate company numbers
                IEnumerable<string> companyNumbers =
                    AllDnBOrgs.Where(o => !string.IsNullOrWhiteSpace(o.CompanyNumber)).Select(o => o.CompanyNumber);
                count = companyNumbers.Count() - companyNumbers.Distinct().Count();
                if (count > 0)
                {
                    throw new Exception($"There are {count} duplicate CompanyNumbers detected");
                }

                //Get the current users email address
                User user = await _DataRepository.GetAll<User>().FirstOrDefaultAsync(u => u.UserId == currentUserId);
                userEmail = user?.EmailAddress;

                //Count records requiring import
                count = AllDnBOrgs.Count(
                    o => !o.GetIsDissolved()
                         && (o.ImportedDate == null || string.IsNullOrWhiteSpace(o.CompanyNumber) || o.ImportedDate < o.StatusCheckedDate));
                if (count == 0)
                {
                    return;
                }

                List<Organisation> dbOrgs = _DataRepository.GetAll<Organisation>().ToList();

                #endregion

                //Set all existing org employer references
                await ReferenceEmployersAsync();

                #region Set all existing org DUNS 

                List<DnBOrgsModel> dnbOrgs = AllDnBOrgs.Where(o => o.OrganisationId > 0 && string.IsNullOrWhiteSpace(o.EmployerReference))
                    .ToList();
                if (dnbOrgs.Count > 0)
                {
                    foreach (DnBOrgsModel dnbOrg in dnbOrgs)
                    {
                        Organisation org = dbOrgs.FirstOrDefault(o => o.OrganisationId == dnbOrg.OrganisationId);
                        if (org == null)
                        {
                            if (!Config.IsProduction())
                            {
                                continue;
                            }

                            throw new Exception($"OrganisationId:{dnbOrg.OrganisationId} does not exist in database");
                        }

                        if (!string.IsNullOrWhiteSpace(org.DUNSNumber))
                        {
                            continue;
                        }

                        org.DUNSNumber = dnbOrg.DUNSNumber;
                        dnbOrg.OrganisationId = null;
                    }

                    await _DataRepository.SaveChangesAsync();
                    dbOrgs = await _DataRepository.GetAll<Organisation>().ToListAsync();

                    await Global.FileRepository.SaveCSVAsync(AllDnBOrgs, dnbOrgsPath);
                    AllDnBOrgs = await Global.FileRepository.ReadCSVAsync<DnBOrgsModel>(dnbOrgsPath);
                    AllDnBOrgs = AllDnBOrgs.OrderBy(o => o.OrganisationName).ToList();
                }

                #endregion

                List<SicCode> allSicCodes = await _DataRepository.GetAll<SicCode>().ToListAsync();

                dnbOrgs = AllDnBOrgs.Where(o => o.ImportedDate == null || o.ImportedDate < o.StatusCheckedDate).ToList();
                while (dnbOrgs.Count > 0)
                {
                    var allBadSicCodes = new ConcurrentBag<OrganisationSicCode>();

                    var c = 0;
                    var dbChanges = 0;
                    var dnbChanges = 0;

                    foreach (DnBOrgsModel dnbOrg in dnbOrgs)
                    {
                        //Only do 100 records at a time
                        if (c > 100)
                        {
                            break;
                        }

                        var dbChanged = false;
                        Organisation dbOrg = dbOrgs.FirstOrDefault(o => o.DUNSNumber == dnbOrg.DUNSNumber);

                        string dataSource = string.IsNullOrWhiteSpace(dnbOrg.NameSource) ? "D&B" : dnbOrg.NameSource;
                        var orgName = new OrganisationName {Name = dnbOrg.OrganisationName.Left(100), Source = dataSource};

                        if (dbOrg == null)
                        {
                            dbOrg = string.IsNullOrWhiteSpace(dnbOrg.CompanyNumber)
                                ? null
                                : dbOrgs.FirstOrDefault(o => o.CompanyNumber == dnbOrg.CompanyNumber);
                            if (dbOrg != null)
                            {
                                dbOrg.DUNSNumber = dnbOrg.DUNSNumber;
                            }
                            else
                            {
                                dbOrg = new Organisation {
                                    DUNSNumber = dnbOrg.DUNSNumber,
                                    EmployerReference = dnbOrg.EmployerReference,
                                    OrganisationName = orgName.Name,
                                    CompanyNumber = string.IsNullOrWhiteSpace(dnbOrg.CompanyNumber) ? null : dnbOrg.CompanyNumber,
                                    SectorType = dnbOrg.SectorType,
                                    DateOfCessation = dnbOrg.DateOfCessation
                                };
                                dbOrg.OrganisationNames.Add(orgName);

                                //Create a presumed in-scope for current year
                                var newScope = new OrganisationScope {
                                    Organisation = dbOrg,
                                    ScopeStatus = ScopeStatuses.PresumedInScope,
                                    ScopeStatusDate = VirtualDateTime.Now,
                                    Status = ScopeRowStatuses.Active,
                                    SnapshotDate = dbOrg.SectorType.GetAccountingStartDate()
                                };
                                _DataRepository.Insert(newScope);
                                dbOrg.OrganisationScopes.Add(newScope);

                                //Create a presumed out-of-scope for previous year
                                var oldScope = new OrganisationScope {
                                    Organisation = dbOrg,
                                    ScopeStatus = ScopeStatuses.PresumedOutOfScope,
                                    ScopeStatusDate = VirtualDateTime.Now,
                                    Status = ScopeRowStatuses.Active,
                                    SnapshotDate = newScope.SnapshotDate.AddYears(-1)
                                };
                                _DataRepository.Insert(oldScope);
                                dbOrg.OrganisationScopes.Add(oldScope);

                                dbOrg.SetStatus(OrganisationStatuses.Active, currentUserId, "Imported from D&B");
                            }
                        }
                        //Skip dissolved companies
                        else if (dbOrg.GetIsDissolved())
                        {
                            dnbOrg.ImportedDate = VirtualDateTime.Now;
                            dnbChanges++;
                            continue;
                        }
                        else if (dbOrg.OrganisationName != orgName.Name)
                        {
                            OrganisationName oldOrgName = dbOrg.GetName();
                            if (oldOrgName == null || SourceComparer.CanReplace(orgName.Source, oldOrgName.Source))
                            {
                                dbOrg.OrganisationName = orgName.Name;
                                dbOrg.OrganisationNames.Add(orgName);
                                dbChanged = true;
                            }
                        }

                        //Ensure D&B gas an organisationID
                        if (dnbOrg.OrganisationId == null || dnbOrg.OrganisationId.Value == 0)
                        {
                            dnbOrg.OrganisationId = dbOrg.OrganisationId;
                            dnbChanges++;
                        }

                        //Add the cessation date
                        if (dbOrg.DateOfCessation == null && dbOrg.DateOfCessation != dnbOrg.DateOfCessation)
                        {
                            dbOrg.DateOfCessation = dnbOrg.DateOfCessation;
                            dbChanged = true;
                        }

                        //Set the employer reference
                        if (string.IsNullOrWhiteSpace(dbOrg.EmployerReference))
                        {
                            string employerReference;
                            do
                            {
                                employerReference = _OrganisationBL.GenerateEmployerReference();
                            } while (employerReferences.Contains(employerReference));

                            dbOrg.EmployerReference = employerReference;
                            employerReferences.Add(employerReference);
                            dbChanged = true;
                        }

                        if (dnbOrg.EmployerReference != dbOrg.EmployerReference)
                        {
                            dnbOrg.EmployerReference = dbOrg.EmployerReference;
                            dnbChanges++;
                        }

                        //Add the new address
                        string fullAddress = dnbOrg.GetAddress();
                        OrganisationAddress newAddress = dbOrg.LatestAddress;

                        //add the address if there isnt one
                        dataSource = string.IsNullOrWhiteSpace(dnbOrg.AddressSource) ? "D&B" : dnbOrg.AddressSource;
                        if (newAddress == null
                            || !newAddress.GetAddressString().EqualsI(fullAddress)
                            && SourceComparer.CanReplace(dataSource, newAddress.Source))
                        {
                            DateTime statusDate = VirtualDateTime.Now;

                            newAddress = new OrganisationAddress();
                            newAddress.Organisation = dbOrg;
                            newAddress.CreatedByUserId = currentUserId;
                            newAddress.Address1 = dnbOrg.AddressLine1;
                            newAddress.Address2 = dnbOrg.AddressLine2;
                            newAddress.Address3 = dnbOrg.AddressLine3;
                            newAddress.County = dnbOrg.County;
                            newAddress.Country = dnbOrg.Country;
                            newAddress.TownCity = dnbOrg.City;
                            newAddress.PostCode = dnbOrg.PostalCode;
                            newAddress.PoBox = dnbOrg.PoBox;
                            newAddress.Source = dataSource;
                            newAddress.SetStatus(AddressStatuses.Active, currentUserId, "Imported from D&B");
                            if (dbOrg.LatestAddress != null)
                            {
                                dbOrg.LatestAddress.SetStatus(AddressStatuses.Retired, currentUserId, $"Replaced by {newAddress.Source}");
                            }
                        }

                        //Update the sic codes
                        SortedSet<int> newCodeIds = dnbOrg.GetSicCodesIds();
                        List<int> newCodesList = dnbOrg.GetSicCodesIds().ToList();
                        for (var i = 0; i < newCodesList.Count; i++)
                        {
                            int code = newCodesList[i];
                            if (code <= 0)
                            {
                                continue;
                            }

                            SicCode sicCode = allSicCodes.FirstOrDefault(sic => sic.SicCodeId == code);
                            if (sicCode != null)
                            {
                                continue;
                            }

                            sicCode = allSicCodes.FirstOrDefault(
                                sic => sic.SicCodeId == code * 10 && sic.Description.EqualsI(dnbOrg.SicDescription));
                            if (sicCode != null)
                            {
                                newCodesList[i] = sicCode.SicCodeId;
                            }
                        }

                        newCodeIds = new SortedSet<int>(newCodesList);

                        var newCodes = new List<OrganisationSicCode>();
                        List<OrganisationSicCode> oldCodes = dbOrg.GetSicCodes().ToList();
                        string oldSicSource = dbOrg.GetSicSource();
                        IEnumerable<int> oldCodeIds = oldCodes.Select(s => s.SicCodeId);
                        if (dbOrg.SectorType == SectorTypes.Public)
                        {
                            newCodeIds.Add(1);
                        }

                        if (!Config.IsProduction())
                        {
                            Debug.WriteLine(
                                $"OLD:{oldCodes.Select(s => s.SicCodeId).ToDelimitedString()} NEW:{newCodeIds.ToDelimitedString()}");
                        }

                        dataSource = string.IsNullOrWhiteSpace(dnbOrg.SicSource) ? "D&B" : dnbOrg.SicSource;
                        if (!newCodeIds.SetEquals(oldCodeIds) && SourceComparer.CanReplace(dataSource, oldSicSource))
                        {
                            foreach (int code in newCodeIds)
                            {
                                if (code <= 0)
                                {
                                    continue;
                                }

                                SicCode sicCode = allSicCodes.FirstOrDefault(sic => sic.SicCodeId == code);
                                var newSic = new OrganisationSicCode {Organisation = dbOrg, SicCodeId = code, Source = dataSource};
                                if (sicCode == null)
                                {
                                    allBadSicCodes.Add(newSic);
                                    continue;
                                }

                                newCodes.Add(newSic);
                            }

                            if (newCodes.Any())
                            {
                                //Add new codes only
                                foreach (OrganisationSicCode newSic in newCodes)
                                {
                                    dbOrg.OrganisationSicCodes.Add(newSic);
                                    dbChanged = true;
                                }

                                //Retire the old codes
                                foreach (OrganisationSicCode oldSic in oldCodes)
                                {
                                    oldSic.Retired = VirtualDateTime.Now;
                                    dbChanged = true;
                                }
                            }
                        }

                        await _DataRepository.BeginTransactionAsync(
                            async () => {
                                try
                                {
                                    //Save the name, Sic, EmployerReference, DateOfCessasion changes
                                    if (dbChanged)
                                    {
                                        await _DataRepository.SaveChangesAsync();
                                    }

                                    //Save the changes
                                    dnbOrg.ImportedDate = VirtualDateTime.Now;
                                    dnbChanges++;
                                    var insert = false;
                                    if (dbOrg.OrganisationId == 0)
                                    {
                                        _DataRepository.Insert(dbOrg);
                                        await _DataRepository.SaveChangesAsync();
                                        dbChanged = true;
                                        insert = true;
                                    }

                                    if (newAddress != null && newAddress.AddressId == 0)
                                    {
                                        dbOrg.OrganisationAddresses.Add(newAddress);
                                        dbOrg.LatestAddress = newAddress;
                                        await _DataRepository.SaveChangesAsync();
                                        dbChanged = true;
                                    }

                                    if (dbChanged)
                                    {
                                        dbChanges++;
                                        _DataRepository.CommitTransaction();
                                        totalChanges++;
                                        if (insert)
                                        {
                                            totalInserts++;
                                        }

                                        //Add or remove this organisation to/from the search index
                                        await _SearchBusinessLogic.UpdateSearchIndexAsync(dbOrg);
                                    }
                                }
                                catch
                                {
                                    _DataRepository.RollbackTransaction();
                                }
                            });
                        c++;
                    }

                    //Reload all the changes
                    if (dbChanges > 0)
                    {
                        dbOrgs = await _DataRepository.GetAll<Organisation>().ToListAsync();
                    }

                    //Save the D&B records
                    if (dnbChanges > 0)
                    {
                        await Global.FileRepository.SaveCSVAsync(AllDnBOrgs, dnbOrgsPath);
                        AllDnBOrgs = await Global.FileRepository.ReadCSVAsync<DnBOrgsModel>(dnbOrgsPath);
                        AllDnBOrgs = AllDnBOrgs.OrderBy(o => o.OrganisationName).ToList();
                        dnbOrgs = AllDnBOrgs.Where(o => o.ImportedDate == null || o.ImportedDate < o.StatusCheckedDate).ToList();
                    }

                    //Save the bad sic codes 
                    if (allBadSicCodes.Count > 0)
                    {
                        //Create the logging tasks
                        var badSicLoggingtasks = new List<Task>();
                        allBadSicCodes.ForEach(
                            bsc => badSicLoggingtasks.Add(
                                Global.BadSicLog.WriteAsync(
                                    new BadSicLogModel {
                                        OrganisationId = bsc.Organisation.OrganisationId,
                                        OrganisationName = bsc.Organisation.OrganisationName,
                                        SicCode = bsc.SicCodeId,
                                        Source = bsc.Source
                                    })));

                        //Wait for all the logging tasks to complete
                        await Task.WhenAll(badSicLoggingtasks);
                    }
                }
            }
            catch (Exception ex)
            {
                error = ex.Message;
                throw;
            }
            finally
            {
                if (!string.IsNullOrWhiteSpace(userEmail))
                {
                    DateTime endTime = VirtualDateTime.Now;
                    TimeSpan duration = endTime - startTime;
                    try
                    {
                        if (!string.IsNullOrWhiteSpace(error))
                        {
                            await _Messenger.SendMessageAsync(
                                "D&B Import Failed",
                                userEmail,
                                $"The D&B import failed at {endTime} after {duration.ToFriendly()}.\nChanged {totalChanges} organisations including {totalInserts} new.\n\nERROR:{error}");
                        }
                        else if (totalChanges == 0)
                        {
                            await _Messenger.SendMessageAsync(
                                "D&B Import Complete",
                                userEmail,
                                "The D&B import process completed successfully with no records requiring import.");
                        }
                        else
                        {
                            await _Messenger.SendMessageAsync(
                                "D&B Import Complete",
                                userEmail,
                                $"The D&B import process completed successfully at {endTime} after {duration.ToFriendly()}.\nChanged {totalChanges} organisations including {totalInserts} new.");
                        }
                    }
                    catch (Exception ex)
                    {
                        log.LogError(ex, ex.Message);
                    }
                }

                RunningJobs.Remove(nameof(DnBImportAsync));
            }
        }

    }
}
