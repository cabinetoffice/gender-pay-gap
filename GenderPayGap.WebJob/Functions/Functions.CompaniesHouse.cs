using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using GenderPayGap.Core;
using GenderPayGap.Core.Classes;
using GenderPayGap.Core.Models;
using GenderPayGap.Core.Models.CompaniesHouse;
using GenderPayGap.Extensions;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace GenderPayGap.WebJob
{
    public partial class Functions
    {

        public async Task CompaniesHouseCheck([TimerTrigger(typeof(MidnightSchedule))]
            TimerInfo timer,
            ILogger log)
        {
            try
            {
                await CompaniesHouseCheckAsync(log);
            }
            catch (Exception ex)
            {
                string message = $"Failed webjob ({nameof(CompaniesHouseCheck)}):{ex.Message}:{ex.GetDetailsText()}";

                //Send Email to GEO reporting errors
                await _Messenger.SendGeoMessageAsync("GPG - WEBJOBS ERROR", message);
                //Rethrow the error
                throw;
            }
        }

        public async Task CompaniesHouseCheckAsync(ILogger log, bool all = false)
        {
            //if (RunningJobs.Contains(nameof(CompaniesHouseCheck)) || RunningJobs.Contains(nameof(DnBImportAsync)))
            //{
            //    return;
            //}

            //RunningJobs.Add(nameof(CompaniesHouseCheck));

            //IEnumerable<string> filePaths = await Global.FileRepository.GetFilesAsync(Global.DataPath, "GPG-DnBOrgs_*.csv");
            //string filePath = filePaths.OrderByDescending(f => f).FirstOrDefault();
            //var updates = 0;
            //try
            //{
            //    if (string.IsNullOrEmpty(filePath))
            //    {
            //        return;
            //    }

            //    if (!await Global.FileRepository.GetFileExistsAsync(filePath))
            //    {
            //        throw new Exception("Could not find " + filePath);
            //    }

            //    string content = await Global.FileRepository.ReadAsync(filePath);
            //    if (string.IsNullOrWhiteSpace(content))
            //    {
            //        throw new Exception("No content in file " + filePath);
            //    }

            //    DataTable datatable = content.ToDataTable();


            //    if (!datatable.Columns.Contains("DateOfCessation"))
            //    {
            //        datatable.Columns.Add("DateOfCessation");
            //    }

            //    if (!datatable.Columns.Contains("CompanyStatus"))
            //    {
            //        datatable.Columns.Add("CompanyStatus");
            //    }

            //    if (!datatable.Columns.Contains("StatusCheckedDate"))
            //    {
            //        datatable.Columns.Add("StatusCheckedDate");
            //    }

            //    if (!datatable.Columns.Contains("AddressSource"))
            //    {
            //        datatable.Columns.Add("AddressSource");
            //    }

            //    if (!datatable.Columns.Contains("OldAddressSource"))
            //    {
            //        datatable.Columns.Add("OldAddressSource");
            //    }

            //    if (!datatable.Columns.Contains("AddressChanged"))
            //    {
            //        datatable.Columns.Add("AddressChanged");
            //    }

            //    if (!datatable.Columns.Contains("NameSource"))
            //    {
            //        datatable.Columns.Add("NameSource");
            //    }

            //    if (!datatable.Columns.Contains("OldNameSource"))
            //    {
            //        datatable.Columns.Add("OldNameSource");
            //    }

            //    if (!datatable.Columns.Contains("OldName"))
            //    {
            //        datatable.Columns.Add("OldName");
            //    }

            //    if (!datatable.Columns.Contains("NameChanged"))
            //    {
            //        datatable.Columns.Add("NameChanged");
            //    }

            //    if (!datatable.Columns.Contains("SicSource"))
            //    {
            //        datatable.Columns.Add("SicSource");
            //    }

            //    if (!datatable.Columns.Contains("OldSicSource"))
            //    {
            //        datatable.Columns.Add("OldSicSource");
            //    }

            //    if (!datatable.Columns.Contains("OldSicCode"))
            //    {
            //        datatable.Columns.Add("OldSicCode");
            //    }

            //    if (!datatable.Columns.Contains("SicChanged"))
            //    {
            //        datatable.Columns.Add("SicChanged");
            //    }

            //    var c = 0;
            //    string companyNumber = null;

            //    //Lookup the status
            //    foreach (DataRow row in datatable.Rows)
            //    {
            //        if (c >= 1000)
            //        {
            //            //If not doing all after 1000 record checked quit and continue later
            //            if (!all)
            //            {
            //                break;
            //            }

            //            //If doing all then save every 1000 records checked
            //            await datatable.SaveAsync(Global.FileRepository, filePath);
            //            c = 0;
            //        }

            //        //Ensure we have a company number
            //        companyNumber = row["CompanyNumber"].ToString();
            //        if (string.IsNullOrWhiteSpace(companyNumber))
            //        {
            //            continue;
            //        }


            //        //Ignore records checked within the last month
            //        DateTime statusCheckedDate = row["StatusCheckedDate"].ToDateTime();
            //        if (statusCheckedDate.AddMonths(1) > VirtualDateTime.Now)
            //        {
            //            continue;
            //        }

            //        //Ignore companies who were dissolved before last checked date
            //        DateTime dateOfCessation = row["DateOfCessation"].ToDateTime();
            //        var sector = row["SectorType"].ToEnum<SectorTypes>();
            //        if (dateOfCessation > DateTime.MinValue && dateOfCessation < sector.GetAccountingStartDate())
            //        {
            //            continue;
            //        }

            //        retry:
            //        try
            //        {
            //            string addressSource = row["AddressSource"].ToString();
            //            string sicSource = row["SicSource"].ToString();
            //            string nameSource = row["NameSource"].ToString();
            //            if (!SourceComparer.CanReplace("CoHo", nameSource)
            //                && !SourceComparer.CanReplace("CoHo", addressSource)
            //                && !SourceComparer.CanReplace("CoHo", sicSource))
            //            {
            //                continue;
            //            }

            //            row["StatusCheckedDate"] = VirtualDateTime.Now.ToString();

            //            Company company = await _CompaniesHouseAPI.GetCompanyAsync(companyNumber);

            //            //Save the status info
            //            if (string.IsNullOrWhiteSpace(row["DateOfCessation"]?.ToString()))
            //            {
            //                row["DateOfCessation"] = (string) company?.date_of_cessation;
            //            }

            //            row["CompanyStatus"] = (string) company?.company_status;

            //            //Save the new company address

            //            if (SourceComparer.CanReplace("CoHo", addressSource))
            //            {
            //                string oldAddress =
            //                    new[] {
            //                            row["AddressLine1"],
            //                            row["AddressLine2"],
            //                            row["AddressLine3"],
            //                            row["City"],
            //                            row["County"],
            //                            row["Country"],
            //                            row["PostalCode"],
            //                            row["PoBox"]
            //                        }.ToDelimitedString(Environment.NewLine)
            //                        .TrimI(", ;.-");
            //                if (company != null && company.registered_office_address != null)
            //                {
            //                    string premises = null,
            //                        addressLine1 = null,
            //                        addressLine2 = null,
            //                        addressLine3 = null,
            //                        city = null,
            //                        county = null,
            //                        country = null,
            //                        postalCode = null,
            //                        poBox = null;
            //                    if (company.registered_office_address.premises != null)
            //                    {
            //                        premises = ((string) company.registered_office_address.premises).CorrectNull().TrimI(", ");
            //                    }

            //                    if (company.registered_office_address.care_of != null)
            //                    {
            //                        addressLine1 = ((string) company.registered_office_address.care_of).CorrectNull().TrimI(", ");
            //                    }

            //                    if (company.registered_office_address.address_line_1 != null)
            //                    {
            //                        addressLine2 = ((string) company.registered_office_address.address_line_1).CorrectNull().TrimI(", ");
            //                    }

            //                    if (!string.IsNullOrWhiteSpace(premises))
            //                    {
            //                        addressLine2 = premises + ", " + addressLine2;
            //                    }

            //                    if (company.registered_office_address.address_line_2 != null)
            //                    {
            //                        addressLine3 = ((string) company.registered_office_address.address_line_2).CorrectNull().TrimI(", ");
            //                    }

            //                    if (company.registered_office_address.locality != null)
            //                    {
            //                        city = ((string) company.registered_office_address.locality).CorrectNull().TrimI(", ");
            //                    }

            //                    if (company.registered_office_address.region != null)
            //                    {
            //                        county = ((string) company.registered_office_address.region).CorrectNull().TrimI(", ");
            //                    }

            //                    if (company.registered_office_address.country != null)
            //                    {
            //                        country = ((string) company.registered_office_address.country).CorrectNull().TrimI(", ");
            //                    }

            //                    if (company.registered_office_address.postal_code != null)
            //                    {
            //                        postalCode = ((string) company.registered_office_address.postal_code).CorrectNull().TrimI(", ");
            //                    }

            //                    if (company.registered_office_address.po_box != null)
            //                    {
            //                        poBox = ((string) company.registered_office_address.po_box).CorrectNull().TrimI(", ");
            //                    }

            //                    string newAddress =
            //                        new[] {addressLine1, addressLine2, addressLine3, city, county, country, postalCode, poBox}
            //                            .ToDelimitedString(Environment.NewLine)
            //                            .TrimI(", ;.-");

            //                    if (!string.IsNullOrWhiteSpace(newAddress)
            //                        && !newAddress.EqualsI(oldAddress)
            //                        && !newAddress.ContainsI("Branch Registration", "Refer To", "Parent Registry"))
            //                    {
            //                        row["AddressLine1"] = addressLine1;
            //                        row["AddressLine2"] = addressLine2;
            //                        row["AddressLine3"] = addressLine3;
            //                        row["City"] = city;
            //                        row["County"] = county;
            //                        if (!string.IsNullOrWhiteSpace(country))
            //                        {
            //                            row["Country"] = country;
            //                        }

            //                        row["PostalCode"] = postalCode;
            //                        row["PoBox"] = poBox;
            //                        row["OldAddress"] = oldAddress;
            //                        row["OldAddressSource"] = addressSource;
            //                        row["AddressSource"] = "CoHo";
            //                        row["AddressChanged"] = VirtualDateTime.Now;
            //                    }
            //                }
            //            }

            //            //Dont overwrite user specified SIC codes
            //            if (SourceComparer.CanReplace("CoHo", sicSource))
            //            {
            //                //Get the new SicCodes
            //                var codes = new HashSet<string>();
            //                if (company != null && company.sic_codes != null)
            //                {
            //                    foreach (dynamic code in company.sic_codes)
            //                    {
            //                        codes.Add(code.Value);
            //                    }
            //                }

            //                //Only replace if at least one SIC code
            //                if (codes.Count > 0)
            //                {
            //                    //Get the old SicCodes
            //                    var oldCodes = new HashSet<string>();
            //                    oldCodes.AddRange(row["SicCode"].ToString().SplitI());

            //                    //Only replace if SIC codes changed
            //                    if (!codes.SequenceEqual(oldCodes))
            //                    {
            //                        row["OldSicCode"] = row["SicCode"];
            //                        row["SicCode"] = codes.ToDelimitedString(";");
            //                        row["OldSicSource"] = sicSource;
            //                        row["SicSource"] = "CoHo";
            //                        row["SicChanged"] = VirtualDateTime.Now;
            //                    }
            //                }
            //            }

            //            //Dont overwrite user specified organisation name
            //            if (SourceComparer.CanReplace("CoHo", nameSource))
            //            {
            //                //Get the new name
            //                var name = (string) company.company_name;

            //                //Only replace if name is not empty
            //                if (!string.IsNullOrWhiteSpace(name))
            //                {
            //                    //Get the old name
            //                    string oldName = row["OrganisationName"].ToString();

            //                    //Only replace if name has changed
            //                    if (!name.EqualsI(oldName))
            //                    {
            //                        row["OldName"] = oldName;
            //                        row["OrganisationName"] = name;
            //                        row["OldNameSource"] = nameSource;
            //                        row["NameSource"] = "CoHo";
            //                        row["NameChanged"] = VirtualDateTime.Now;
            //                    }
            //                }
            //            }

            //            c++;
            //            updates++;
            //        }
            //        catch (Exception ex)
            //        {
            //            if (!(ex.InnerException is HttpException httpEx))
            //            {
            //                throw;
            //            }

            //            if (httpEx.StatusCode == 429)
            //            {
            //                if (c > 0)
            //                {
            //                    await datatable.SaveAsync(Global.FileRepository, filePath);
            //                    c = 0;
            //                }

            //                Thread.Sleep(60000);
            //                goto retry;
            //            }

            //            if (httpEx.StatusCode.IsAny(500, 502))
            //            {
            //                if (c > 0)
            //                {
            //                    await datatable.SaveAsync(Global.FileRepository, filePath);
            //                    c = 0;
            //                }

            //                Thread.Sleep(20000);
            //                goto retry;
            //            }

            //            if (httpEx.StatusCode == (int) HttpStatusCode.NotFound)
            //            {
            //                row["StatusCheckedDate"] = VirtualDateTime.Now.ToString();
            //                Interlocked.Increment(ref c);
            //            }
            //            else
            //            {
            //                throw;
            //            }
            //        }
            //    }

            //    //Save the results
            //    if (datatable != null && c > 0)
            //    {
            //        await datatable.SaveAsync(Global.FileRepository, filePath);
            //    }
            //}
            //finally
            //{
            //    if (updates > 0 && all)
            //    {
            //        //Update the team that the updates are complete
            //        List<DnBOrgsModel> AllDnBOrgs = await Global.FileRepository.ReadCSVAsync<DnBOrgsModel>(filePath);
            //        int count = AllDnBOrgs.Count(
            //            o => !string.IsNullOrWhiteSpace(o.CompanyNumber)
            //                 && !o.GetIsDissolved()
            //                 && (o.StatusCheckedDate == null || o.StatusCheckedDate.Value.AddMonths(1) < VirtualDateTime.Now));
            //        if (count == 0)
            //        {
            //            try
            //            {
            //                await _Messenger.SendGeoMessageAsync(
            //                    "Companies House Check Complete",
            //                    $"The Companies house check complete at {VirtualDateTime.Now} for all records in the D&B file and is now ready for import.");
            //            }
            //            catch (Exception ex)
            //            {
            //                log.LogError(ex, ex.Message);
            //            }
            //        }
            //    }

            //    RunningJobs.Remove(nameof(CompaniesHouseCheck));
            // }
        }

    }
}
