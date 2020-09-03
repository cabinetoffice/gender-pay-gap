using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using GenderPayGap.Core;
using GenderPayGap.Core.Classes;
using GenderPayGap.Database;
using GenderPayGap.Extensions;
using GenderPayGap.WebUI.Classes;
using GenderPayGap.WebUI.Models.Admin;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GenderPayGap.WebUI.Controllers.Administration
{
    public partial class AdminController : BaseController
    {

        [HttpGet("manual-changes")]
        public IActionResult ManualChanges()
        {
            return View(new ManualChangesViewModel());
        }

        [PreventDuplicatePost]
        [ValidateAntiForgeryToken]
        [HttpPost("manual-changes")]
        public async Task<IActionResult> ManualChanges(ManualChangesViewModel model)
        {
            model.Results = null;
            ModelState.Clear();
            bool test = model.LastTestedCommand != model.Command
                        || model.LastTestedInput != model.Parameters.ReplaceI(Environment.NewLine, ";");
            model.Tested = false;
            long count = 0;
            int? total = null;

            model.SuccessMessage = null;
            using (var writer = new StringWriter())
            {
                try
                {
                    switch (model.Command)
                    {
                        case "Please select..":
                            throw new ArgumentException("ERROR: You must first select a command");
                        case "Convert public to private":
                            count = await ConvertPublicOrganisationsToPrivateAsync(model.Parameters, model.Comment, writer, test);
                            break;
                        case "Convert private to public":
                            count = await ConvertPrivateOrganisationsToPublicAsync(model.Parameters, model.Comment, writer, test);
                            break;
                        case "Set organisation company number":
                            count = await SetOrganisationCompanyNumberAsync(model.Parameters, model.Comment, writer, test);
                            break;
                        default:
                            throw new NotImplementedException($"ERROR: The command '{model.Command}' has not yet been implemented");
                    }

                    if (test)
                    {
                        model.LastTestedCommand = model.Command;
                        model.LastTestedInput = model.Parameters.ReplaceI(Environment.NewLine, ";");
                        model.Tested = true;
                    }
                    else
                    {
                        model.LastTestedCommand = null;
                        model.LastTestedInput = null;
                        model.Comment = null;
                    }

                    //Add a summary to the output
                    if (!string.IsNullOrWhiteSpace(model.Parameters))
                    {
                        total = total ?? model.Parameters.LineCount();
                        model.SuccessMessage = $"SUCCESSFULLY {(test ? "TESTED" : "EXECUTED")} '{model.Command}': {count} of {total}";
                    }
                    else
                    {
                        model.SuccessMessage = $"SUCCESSFULLY {(test ? "TESTED" : "EXECUTED")} '{model.Command}': {count} items";
                    }
                }
                catch (AggregateException ex)
                {
                    foreach (Exception iex in ex.InnerExceptions)
                    {
                        Exception exception = iex;

                        while (exception.InnerException != null)
                        {
                            exception = exception.InnerException;
                        }

                        ModelState.AddModelError("", exception.Message);
                    }
                }
                catch (Exception ex)
                {
                    Exception exception = ex;

                    while (exception.InnerException != null)
                    {
                        exception = exception.InnerException;
                    }

                    ModelState.AddModelError("", exception.Message);
                }

                model.Results = writer.ToString();
            }

            return View(model);
        }

        private async Task<int> SetOrganisationCompanyNumberAsync(string input, string comment, StringWriter writer, bool test)
        {
            string methodName = nameof(SetOrganisationCompanyNumberAsync);

            //Split the input into separate action lines
            string[] lines = input.SplitI(Environment.NewLine);
            if (lines.Length == 0)
            {
                throw new ArgumentException("ERROR: You must supply 1 or more input parameters");
            }

            //Execute the command for each line
            var count = 0;
            var i = 0;
            var processed = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var processedCoNos = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (string line in lines)
            {
                i++;
                if (!line.Contains('='))
                {
                    writer.WriteLine($"{i}: ERROR: '{line}' does not contain '=' character");
                    continue;
                }

                string employerRef = line.BeforeFirst("=")?.ToUpper();
                if (string.IsNullOrWhiteSpace(employerRef))
                {
                    continue;
                }

                if (processed.Contains(employerRef))
                {
                    writer.WriteLine($"{i}: ERROR: '{employerRef}' duplicate organisation");
                    continue;
                }

                processed.Add(employerRef);

                string newValue = line.AfterFirst("=", includeWhenNoSeparator: false).TrimI()?.ToUpper();

                if (string.IsNullOrWhiteSpace(newValue))
                {
                    newValue = null;
                }
                else
                {
                    newValue = newValue.FixCompanyNumber();

                    if (!newValue.IsCompanyNumber())
                    {
                        writer.WriteLine($"{i}: ERROR: '{employerRef}' Invalid company number '{newValue}'");
                        continue;
                    }

                    if (processedCoNos.Contains(newValue))
                    {
                        writer.WriteLine($"{i}: ERROR: '{employerRef}' duplicate company number '{newValue}'");
                        continue;
                    }

                    processedCoNos.Add(newValue);
                }

                Organisation org = await DataRepository.GetAll<Organisation>()
                    .FirstOrDefaultAsync(o => o.EmployerReference.ToUpper() == employerRef);
                if (org == null)
                {
                    writer.WriteLine($"{i}: ERROR: '{employerRef}' Cannot find organisation with this employer reference");
                    continue;
                }

                string oldValue = org.CompanyNumber;

                if (oldValue == newValue)
                {
                    writer.WriteLine(
                        $"{i}: WARNING '{employerRef}' '{org.OrganisationName}' Company Number='{org.CompanyNumber}' already set to '{oldValue}'");
                }
                else if (!string.IsNullOrWhiteSpace(newValue)
                         && await DataRepository.GetAll<Organisation>()
                             .AnyAsync(o => o.CompanyNumber == newValue && o.OrganisationId != org.OrganisationId))
                {
                    writer.WriteLine($"{i}: ERROR '{employerRef}' Another organisation exists with this company number");
                    continue;
                }
                else
                {
                    //Output the actual execution result
                    org.CompanyNumber = newValue;
                    writer.WriteLine(
                        $"{i}: {employerRef}: {org.OrganisationName} Company Number='{org.CompanyNumber}' set to '{newValue}'");
                    if (!test)
                    {
                        await DataRepository.SaveChangesAsync();

                        auditLogger.AuditChangeToOrganisation(
                            AuditedAction.ExecuteManualChangeSetOrganisationCompanyNumber,
                            org,
                            new
                            {
                                OldCompanyNumber = oldValue,
                                NewCompanyNumber = newValue,
                                Reason = comment,
                            },
                            User);
                    }
                }

                count++;
            }

            return count;
        }

        private async Task<int> ConvertPrivateOrganisationsToPublicAsync(string input, string comment, StringWriter writer, bool test)
        {
            string methodName = nameof(ConvertPrivateOrganisationsToPublicAsync);

            //Split the input into separate action lines
            string[] lines = input.SplitI(Environment.NewLine);
            if (lines.Length == 0)
            {
                throw new ArgumentException("ERROR: You must supply 1 or more input parameters");
            }

            //Execute the command for each line
            var count = 0;
            var i = 0;
            var processed = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (string line in lines)
            {
                i++;
                if (line.Contains('='))
                {
                    writer.WriteLine($"{i}: ERROR: '{line}' contains '=' character");
                    continue;
                }

                string employerRef = line?.ToUpper();
                if (string.IsNullOrWhiteSpace(employerRef))
                {
                    continue;
                }

                if (processed.Contains(employerRef))
                {
                    writer.WriteLine($"{i}: ERROR: '{employerRef}' duplicate organisation");
                    continue;
                }

                processed.Add(employerRef);

                var newSector = OrganisationSectors.Public;

                Organisation org = await DataRepository.GetAll<Organisation>()
                    .FirstOrDefaultAsync(o => o.EmployerReference.ToUpper() == employerRef);
                if (org == null)
                {
                    writer.WriteLine($"{i}: ERROR: '{employerRef}' Cannot find organisation with this employer reference");
                    continue;
                }

                OrganisationSectors oldSector = org.Sector;

                var badReturnDates = false;
                foreach (Return @return in org.Returns)
                {
                    DateTime oldDate = @return.AccountingDate;
                    DateTime newDate = newSector.GetAccountingStartDate(oldDate.Year);
                    if (oldDate == newDate)
                    {
                        continue;
                    }

                    badReturnDates = true;
                    break;
                }

                var badScopeDates = false;
                foreach (OrganisationScope scope in org.OrganisationScopes)
                {
                    DateTime oldDate = scope.SnapshotDate;
                    DateTime newDate = newSector.GetAccountingStartDate(oldDate.Year);
                    if (oldDate == newDate)
                    {
                        continue;
                    }

                    badScopeDates = true;
                    break;
                }

                IEnumerable<OrganisationSicCode> sicCodes = org.GetSicCodes();

                if (oldSector == newSector && sicCodes.Any(s => s.SicCodeId == 1 && s.Retired == null) && !badReturnDates && !badScopeDates)
                {
                    writer.WriteLine(
                        $"{i}: WARNING: '{employerRef}' '{org.OrganisationName}' sector already set to '{oldSector}'");
                }
                else
                {
                    if (oldSector != newSector)
                    {
                        //Change the sector type
                        org.Sector = newSector;
                        if (!test)
                        {
                            auditLogger.AuditChangeToOrganisation(
                                AuditedAction.ExecuteManualChangeConvertPrivateToPublic,
                                org,
                                new
                                {
                                    Reason = comment,
                                },
                                User);
                        }
                    }

                    //Add SIC Code 1
                    if (!sicCodes.Any(sic => sic.SicCodeId == 1 && sic.Retired == null))
                    {
                        org.OrganisationSicCodes.Add(
                            new OrganisationSicCode {
                                OrganisationId = org.OrganisationId,
                                SicCodeId = 1,
                                Source = "Manual",
                                Created = sicCodes.Any() ? sicCodes.First().Created : VirtualDateTime.Now
                            });
                        if (!test)
                        {
                            auditLogger.AuditChangeToOrganisation(
                                AuditedAction.ExecuteManualChangeSetOrganisationSicCodes,
                                org,
                                new
                                {
                                    OldSicCodes = "(converted from private sector)",
                                    NewSicCodes = "1",
                                    Reason = comment,
                                },
                                User);
                        }
                    }

                    //Set accounting Date
                    if (badReturnDates)
                    {
                        foreach (Return @return in org.Returns)
                        {
                            DateTime oldDate = @return.AccountingDate;
                            DateTime newDate = newSector.GetAccountingStartDate(oldDate.Year);
                            if (oldDate == newDate)
                            {
                                continue;
                            }

                            @return.AccountingDate = newDate;
                            if (!test)
                            {
                                auditLogger.AuditChangeToOrganisation(
                                    AuditedAction.ExecuteManualChangeConvertSectorSetAccountingDate,
                                    org,
                                    new
                                    {
                                        ReturnId = @return.ReturnId,
                                        OldDate = oldDate,
                                        NewDate = newDate,
                                        Reason = comment,
                                    },
                                    User);
                            }
                        }
                    }

                    //Set snapshot Date
                    if (badScopeDates)
                    {
                        foreach (OrganisationScope scope in org.OrganisationScopes)
                        {
                            DateTime oldDate = scope.SnapshotDate;
                            DateTime newDate = newSector.GetAccountingStartDate(oldDate.Year);
                            if (oldDate == newDate)
                            {
                                continue;
                            }

                            scope.SnapshotDate = newDate;
                            if (!test)
                            {
                                auditLogger.AuditChangeToOrganisation(
                                    AuditedAction.ExecuteManualChangeConvertSectorSetAccountingDate,
                                    org,
                                    new
                                    {
                                        ScopeId = scope.OrganisationScopeId,
                                        OldDate = oldDate,
                                        NewDate = newDate,
                                        Reason = comment,
                                    },
                                    User);
                            }
                        }
                    }

                    writer.WriteLine($"{i}: {employerRef}: {org.OrganisationName} sector {oldSector} set to '{newSector}'");
                    if (!test)
                    {
                        await DataRepository.SaveChangesAsync();
                    }
                }

                count++;
            }

            return count;
        }

        private async Task<int> ConvertPublicOrganisationsToPrivateAsync(string input, string comment, StringWriter writer, bool test)
        {
            string methodName = nameof(ConvertPublicOrganisationsToPrivateAsync);

            //Split the input into separate action lines
            string[] lines = input.SplitI(Environment.NewLine);
            if (lines.Length == 0)
            {
                throw new ArgumentException("ERROR: You must supply 1 or more input parameters");
            }

            //Execute the command for each line
            var count = 0;
            var i = 0;
            var processed = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (string line in lines)
            {
                i++;
                if (line.Contains('='))
                {
                    writer.WriteLine($"{i}: ERROR: '{line}' contains '=' character");
                    continue;
                }

                string employerRef = line?.ToUpper();
                if (string.IsNullOrWhiteSpace(employerRef))
                {
                    continue;
                }

                if (processed.Contains(employerRef))
                {
                    writer.WriteLine($"{i}: '{employerRef}' duplicate organisation");
                    continue;
                }

                processed.Add(employerRef);

                var newSector = OrganisationSectors.Private;

                Organisation org = await DataRepository.GetAll<Organisation>()
                    .FirstOrDefaultAsync(o => o.EmployerReference.ToUpper() == employerRef);
                if (org == null)
                {
                    writer.WriteLine($"{i}: ERROR: '{employerRef}' Cannot find organisation with this employer reference");
                    continue;
                }

                OrganisationSectors oldSector = org.Sector;

                var badReturnDates = false;
                foreach (Return @return in org.Returns)
                {
                    DateTime oldDate = @return.AccountingDate;
                    DateTime newDate = newSector.GetAccountingStartDate(oldDate.Year);
                    if (oldDate == newDate)
                    {
                        continue;
                    }

                    badReturnDates = true;
                    break;
                }

                var badScopeDates = false;
                foreach (OrganisationScope scope in org.OrganisationScopes)
                {
                    DateTime oldDate = scope.SnapshotDate;
                    DateTime newDate = newSector.GetAccountingStartDate(oldDate.Year);
                    if (oldDate == newDate)
                    {
                        continue;
                    }

                    badScopeDates = true;
                    break;
                }

                IEnumerable<OrganisationSicCode> sicCodes = org.GetSicCodes();

                if (oldSector == newSector && !sicCodes.Any(s => s.SicCodeId == 1) && !badReturnDates && !badScopeDates)
                {
                    writer.WriteLine(
                        $"{i}: WARNING: '{employerRef}' '{org.OrganisationName}' sector already set to '{oldSector}'");
                }
                else
                {
                    //Change the sector type
                    if (oldSector != newSector)
                    {
                        org.Sector = newSector;
                        if (!test)
                        {
                            auditLogger.AuditChangeToOrganisation(
                                AuditedAction.ExecuteManualChangeConvertPublicToPrivate,
                                org,
                                new
                                {
                                    Reason = comment,
                                },
                                User);
                        }
                    }

                    //Remove SIC Code 1
                    if (sicCodes.Any(s => s.SicCodeId == 1))
                    {
                        foreach (OrganisationSicCode sic in org.OrganisationSicCodes.ToList())
                        {
                            if (sic.SicCodeId != 1)
                            {
                                continue;
                            }

                            if (!test)
                            {
                                auditLogger.AuditChangeToOrganisation(
                                    AuditedAction.ExecuteManualChangeSetOrganisationSicCodes,
                                    org,
                                    new
                                    {
                                        OldSicCodes = "(converted from public sector, so deleting SIC code 1)",
                                        OldSicCode_OrganisationSicCodeId = sic.OrganisationSicCodeId,
                                        OldSicCode_SicCodeId = sic.SicCodeId,
                                        OldSicCode_Source = sic.Source,
                                        OldSicCode_Created = sic.Created,
                                        OldSicCode_Retired = sic.Retired,
                                        Reason = comment,
                                    },
                                    User);
                            }

                            DataRepository.Delete(sic);
                        }
                    }

                    //Set accounting Date
                    if (badReturnDates)
                    {
                        foreach (Return @return in org.Returns)
                        {
                            DateTime oldDate = @return.AccountingDate;
                            DateTime newDate = newSector.GetAccountingStartDate(oldDate.Year);
                            if (oldDate == newDate)
                            {
                                continue;
                            }

                            @return.AccountingDate = newDate;
                            if (!test)
                            {
                                auditLogger.AuditChangeToOrganisation(
                                    AuditedAction.ExecuteManualChangeConvertSectorSetAccountingDate,
                                    org,
                                    new
                                    {
                                        ReturnId = @return.ReturnId,
                                        OldDate = oldDate,
                                        NewDate = newDate,
                                        Reason = comment,
                                    },
                                    User);
                            }

                            if (string.IsNullOrWhiteSpace(@return.ResponsiblePerson))
                            {
                                writer.WriteLine(
                                    $"    WARNING: No personal responsible for '{employerRef}' for data submited for year '{oldDate.Year}'");
                            }
                        }
                    }

                    //Set snapshot Date
                    if (badScopeDates)
                    {
                        foreach (OrganisationScope scope in org.OrganisationScopes)
                        {
                            DateTime oldDate = scope.SnapshotDate;
                            DateTime newDate = newSector.GetAccountingStartDate(oldDate.Year);
                            if (oldDate == newDate)
                            {
                                continue;
                            }

                            scope.SnapshotDate = newDate;
                            if (!test)
                            {
                                auditLogger.AuditChangeToOrganisation(
                                    AuditedAction.ExecuteManualChangeConvertSectorSetAccountingDate,
                                    org,
                                    new
                                    {
                                        ScopeId = scope.OrganisationScopeId,
                                        OldDate = oldDate,
                                        NewDate = newDate,
                                        Reason = comment,
                                    },
                                    User);
                            }
                        }
                    }

                    writer.WriteLine($"{i}: {employerRef}: {org.OrganisationName} sector {oldSector} set to '{newSector}'");
                    if (!test)
                    {
                        await DataRepository.SaveChangesAsync();
                    }
                }

                count++;
            }

            return count;
        }

    }
}
