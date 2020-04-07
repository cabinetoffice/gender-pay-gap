using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using GenderPayGap.Core;
using GenderPayGap.Core.Classes;
using GenderPayGap.Core.Classes.ErrorMessages;
using GenderPayGap.Core.Models;
using GenderPayGap.Core.Models.HttpResultModels;
using GenderPayGap.Database;
using GenderPayGap.Extensions;
using GenderPayGap.WebUI.Classes;
using GenderPayGap.WebUI.Models.Admin;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace GenderPayGap.WebUI.Controllers.Administration
{
    public partial class AdminController : BaseController
    {

        private async Task<long> UpdateSearchIndexesAsync(string parameters, string comment, StringWriter writer, bool test)
        {
            if (!string.IsNullOrWhiteSpace(parameters))
            {
                throw new ArgumentException("ERROR: parameters must be empty");
            }

            long count = await AdminService.GetSearchDocumentCountAsync();
            if (!test)
            {
                await Program.MvcApplication.ExecuteWebjobQueue.AddMessageAsync(
                    new QueueWrapper($"command=UpdateSearch&userEmail={CurrentUser.EmailAddress}&comment={comment}"));
                writer.WriteLine(
                    $"An email will be sent to '{CurrentUser.EmailAddress}' when the background task '{nameof(UpdateSearchIndexesAsync)}' has completed");
            }

            return count;
        }

        private async Task<long> UpdateDownloadFilesAsync(string parameters, string comment, StringWriter writer, bool test)
        {
            if (!string.IsNullOrWhiteSpace(parameters))
            {
                throw new ArgumentException("ERROR: parameters must be empty");
            }

            long count = await AdminService.GetSearchDocumentCountAsync();
            if (!test)
            {
                await Program.MvcApplication.ExecuteWebjobQueue.AddMessageAsync(
                    new QueueWrapper($"command=UpdateDownloadFiles&userEmail={CurrentUser.EmailAddress}&comment={comment}"));
                writer.WriteLine(
                    $"An email will be sent to '{CurrentUser.EmailAddress}' when the background task '{nameof(UpdateDownloadFilesAsync)}' has completed");
            }

            return count;
        }
        
        public class BulkResult
        {

            public int Count { get; set; }
            public int TotalRecords { get; set; }

        }

        #region Manual Changes

        [HttpGet("manual-changes")]
        public IActionResult ManualChanges()
        {
            //Throw error if the user is not a super administrator
            if (!IsDatabaseAdministrator)
            {
                return new HttpUnauthorizedResult($"User {CurrentUser?.EmailAddress} is not a database administrator");
            }

            return View(new ManualChangesViewModel());
        }

        [PreventDuplicatePost]
        [ValidateAntiForgeryToken]
        [HttpPost("manual-changes")]
        public async Task<IActionResult> ManualChanges(ManualChangesViewModel model)
        {
            //Throw error if the user is not a super administrator
            if (!IsDatabaseAdministrator)
            {
                return new HttpUnauthorizedResult($"User {CurrentUser?.EmailAddress} is not a database administrator");
            }

            model.Results = null;
            ModelState.Clear();
            bool test = model.LastTestedCommand != model.Command
                        || model.LastTestedInput != model.Parameters.ReplaceI(Environment.NewLine, ";");
            model.Tested = false;
            BulkResult result = null;
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
                        case "Add organisations latest name":
                            count = await SetOrganisationNameAsync(model.Parameters, model.Comment, writer, test);
                            break;
                        case "Reset organisation to only original name":
                            count = await ResetOrganisationNameAsync(model.Parameters, model.Comment, writer, test);
                            break;
                        case "Convert public to private":
                            count = await ConvertPublicOrganisationsToPrivateAsync(model.Parameters, model.Comment, writer, test);
                            break;
                        case "Convert private to public":
                            count = await ConvertPrivateOrganisationsToPublicAsync(model.Parameters, model.Comment, writer, test);
                            break;
                        case "Delete submissions":
                            count = await DeleteSubmissionsAsync(model.Parameters, model.Comment, writer, test);
                            break;
                        case "Set organisation company number":
                            count = await SetOrganisationCompanyNumberAsync(model.Parameters, model.Comment, writer, test);
                            break;
                        case "Set organisation SIC codes":
                            count = await SetOrganisationSicCodesAsync(model.Parameters, model.Comment, writer, test);
                            break;
                        case "Set organisation addresses":
                            count = await SetOrganisationAddressesAsync(model.Parameters, model.Comment, writer, test);
                            break;
                        case "Set public sector type":
                            count = await SetPublicSectorTypeAsync(model.Parameters, model.Comment, writer, test);
                            break;
                        case "Set organisation as out of scope":
                            count = await SetOrganisationScopeAsync(
                                model.Parameters,
                                model.Comment,
                                writer,
                                ScopeStatuses.OutOfScope,
                                test);
                            break;
                        case "Set organisation as in scope":
                            count = await SetOrganisationScopeAsync(model.Parameters, model.Comment, writer, ScopeStatuses.InScope, test);
                            break;
                        case "Update search indexes":
                            count = await UpdateSearchIndexesAsync(model.Parameters, model.Comment, writer, test);
                            break;
                        case "Update GPG download data files":
                            count = await UpdateDownloadFilesAsync(model.Parameters, model.Comment, writer, test);
                            break;
                        case "Create security code":
                            count = await SecurityCodeWorkAsync(
                                model.Parameters,
                                model.Comment,
                                writer,
                                test,
                                ManualActions.Create,
                                OrganisationBusinessLogic.CreateSecurityCodeAsync);
                            break;
                        case "Create security codes for all active and pending orgs":
                            result = await SecurityCodeBulkWorkAsync(
                                model.Parameters,
                                model.Comment,
                                writer,
                                test,
                                ManualActions.Create,
                                OrganisationBusinessLogic.CreateSecurityCodesInBulkAsync);
                            count = result.Count;
                            total = result.TotalRecords;
                            break;
                        case "Extend security code":
                            count = await SecurityCodeWorkAsync(
                                model.Parameters,
                                model.Comment,
                                writer,
                                test,
                                ManualActions.Extend,
                                OrganisationBusinessLogic.ExtendSecurityCodeAsync);
                            break;
                        case "Extend security codes for all active and pending orgs":
                            result = await SecurityCodeBulkWorkAsync(
                                model.Parameters,
                                model.Comment,
                                writer,
                                test,
                                ManualActions.Create,
                                OrganisationBusinessLogic.ExtendSecurityCodesInBulkAsync);
                            count = result.Count;
                            total = result.TotalRecords;
                            break;
                        case "Expire security code":
                            count = await ExpireSecurityCodeAsync(model.Parameters, model.Comment, writer, test, ManualActions.Expire);
                            break;
                        case "Expire security codes for all active and pending orgs":
                            result = await ExpireSecurityCodeBulkWorkAsync(
                                model.Parameters,
                                model.Comment,
                                writer,
                                test,
                                ManualActions.Create);
                            count = result.Count;
                            total = result.TotalRecords;
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

        /// <summary>
        ///     Contains the logic to extract an employer reference from a manual changes parameter line.
        /// </summary>
        /// <param name="lineToReview">line containing the employer reference</param>
        /// <param name="employerReference">the found employer reference or null if unable to extract</param>
        /// <returns>true if the employer reference was found, false otherwise</returns>
        private static bool HasEmployerReference(ref string lineToReview, out string employerReference)
        {
            employerReference = lineToReview.BeforeFirst("=")?.ToUpper();

            // if found, remove from line to review
            if (!string.IsNullOrEmpty(employerReference))
            {
                lineToReview = lineToReview.Replace(employerReference, "");
            }

            return !string.IsNullOrEmpty(employerReference);
        }

        private static bool HasDateTimeInfo(ref string lineToReview, out DateTime extractedDateTime)
        {
            string parameterDateTimeSection = lineToReview.AfterFirst("=")?.ToUpper();
            extractedDateTime = parameterDateTimeSection.ToDateTime();

            return extractedDateTime != DateTime.MinValue;
        }

        /// <summary>
        ///     Contains the logic to extract an snapshotYear from a manual changes parameter line.
        /// </summary>
        /// <param name="lineToReview">line containing the snapshot year</param>
        /// <param name="snapshotYear">the snapshot year or zero if unable to extract</param>
        private static void GetSnapshotYear(ref string lineToReview, out int snapshotYear)
        {
            string parameterDetailSection = lineToReview.AfterFirst("=")?.ToUpper();

            bool containsComma = parameterDetailSection.ContainsI(",");

            string candidateSnapshotYear = parameterDetailSection;

            if (containsComma)
            {
                candidateSnapshotYear = parameterDetailSection.BeforeFirst(",")?.ToUpper();
            }

            snapshotYear = candidateSnapshotYear.ToInt32();

            // if found, remove from line to review
            if (snapshotYear != default)
            {
                lineToReview = lineToReview.Replace(snapshotYear.ToString(), "");
            }
        }

        /// <summary>
        ///     Contains the logic to extract a comment from the 'lineToReview' parameter.
        ///     <para>
        ///         if the comment was found this method RETURNS 'true' and the extracted comment is available on the 'out'
        ///         parameter 'changeScopeToComment'.
        ///     </para>
        /// </summary>
        /// <param name="lineToReview">line containing the comment to extract</param>
        /// <param name="comment">the found comment or null if unable to extract</param>
        /// <returns>
        ///     FALSE (unable to find) if the comment was NOT found, or TRUE if it was indeed able to read a comment from the
        ///     given line.
        /// </returns>
        private static bool GetComment(ref string lineToReview, out string comment)
        {
            string parameterDetailSection = lineToReview.AfterFirst("=");

            bool containsComma = parameterDetailSection.ContainsI(",");

            comment = parameterDetailSection;

            if (containsComma)
            {
                comment = parameterDetailSection.AfterFirst(",");
            }

            bool found = !string.IsNullOrEmpty(comment);

            // if the comment was found - not empty -, then remove it from line under review
            if (found)
            {
                lineToReview = lineToReview.Replace(comment, "");
            }

            return found;
        }

        private async Task<long> SetOrganisationScopeAsync(string input,
            string comment,
            StringWriter writer,
            ScopeStatuses scopeStatus,
            bool test)
        {
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
            var listOfModifiedOrgs = new HashSet<Organisation>();
            foreach (string line in lines)
            {
                string outcome = line;

                i++;

                if (!HasEmployerReference(ref outcome, out string employerRef))
                {
                    writer.WriteLine(
                        Color.Red,
                        $"{i}: ERROR: '{line}' expected an employer reference before the '=' character (i.e. EmployerReference=SnapshotYear,Comment to add to the scope change for this particular employer)");
                    continue;
                }

                GetSnapshotYear(ref outcome, out int changeScopeToSnapshotYear);
                bool wasCommentFoundInLine = GetComment(ref outcome, out string changeScopeToComment);
                bool commentBeginsWithNumber = Regex.IsMatch(changeScopeToComment, "^(\\s*\\d{1}|\\d{1})");

                if (commentBeginsWithNumber && changeScopeToSnapshotYear == default)
                {
                    writer.WriteLine(
                        Color.Red,
                        $"{i}: ERROR: '{employerRef}' the in-line comment appears to begin with a number, if having a number as part of the comment is intended please reenter this line with the format '{employerRef}=SnapshotYear,{changeScopeToComment}', alternatively add a comma after the number.");
                    continue;
                }

                if (!wasCommentFoundInLine)
                {
                    if (string.IsNullOrEmpty(comment))
                    {
                        writer.WriteLine(
                            Color.Red,
                            $"{i}: ERROR: '{employerRef}' please enter a comment in the comments section of this page");
                        continue;
                    }

                    changeScopeToComment = comment;
                }

                try
                {
                    if (processed.Contains(employerRef))
                    {
                        writer.WriteLine(Color.Red, $"{i}: ERROR: '{employerRef}' duplicate organisation");
                        continue;
                    }

                    processed.Add(employerRef);

                    CustomResult<OrganisationScope> outOfScopeOutcome = await OrganisationBusinessLogic.SetAsScopeAsync(
                        employerRef,
                        changeScopeToSnapshotYear,
                        changeScopeToComment,
                        CurrentUser,
                        scopeStatus,
                        false);

                    if (outOfScopeOutcome.Failed)
                    {
                        writer.WriteLine(Color.Red, $"{i}: ERROR: '{employerRef}' {outOfScopeOutcome.ErrorMessage}");
                        continue;
                    }

                    if (outOfScopeOutcome.Result.Organisation != null)
                    {
                        listOfModifiedOrgs.Add(outOfScopeOutcome.Result.Organisation);
                    }

                    string hasBeenWillBe = test ? "will be" : "has been";
                    writer.WriteLine(
                        $"{i}: {employerRef}: {hasBeenWillBe} set as '{outOfScopeOutcome.Result.ScopeStatus}' for snapshotYear '{outOfScopeOutcome.Result.SnapshotDate.Year}' with comment '{outOfScopeOutcome.Result.Reason}'");
                    if (!test)
                    {
                        await Global.ManualChangeLog.WriteAsync(
                            new ManualChangeLogModel {
                                MethodName = $"SetOrg{scopeStatus.ToString()}",
                                Action = ManualActions.Update,
                                Source = CurrentUser.EmailAddress,
                                Comment = comment,
                                ReferenceName = nameof(Organisation.EmployerReference),
                                ReferenceValue = employerRef,
                                TargetName = nameof(Organisation.OrganisationScopes)
                            });
                    }
                }
                catch (Exception ex)
                {
                    writer.WriteLine(Color.Red, $"{i}: ERROR: '{employerRef}' {ex.Message}");
                    continue;
                }

                count++;
            }

            if (!test && listOfModifiedOrgs.Count > 0)
            {
                await DataRepository.SaveChangesAsync();
                //todo: writer.WriteLine(Color.Green, $"INFO: Changes saved to database, attempting to update search index.");

                await SearchBusinessLogic.UpdateSearchIndexAsync(listOfModifiedOrgs.ToArray());
                //todo: writer.WriteLine(Color.Green, $"INFO: Search index updated successfully.");
            }

            return count;
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
                    writer.WriteLine(Color.Red, $"{i}: ERROR: '{line}' does not contain '=' character");
                    continue;
                }

                string employerRef = line.BeforeFirst("=")?.ToUpper();
                if (string.IsNullOrWhiteSpace(employerRef))
                {
                    continue;
                }

                if (processed.Contains(employerRef))
                {
                    writer.WriteLine(Color.Red, $"{i}: ERROR: '{employerRef}' duplicate organisation");
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
                        writer.WriteLine(Color.Red, $"{i}: ERROR: '{employerRef}' Invalid company number '{newValue}'");
                        continue;
                    }

                    if (processedCoNos.Contains(newValue))
                    {
                        writer.WriteLine(Color.Red, $"{i}: ERROR: '{employerRef}' duplicate company number '{newValue}'");
                        continue;
                    }

                    processedCoNos.Add(newValue);
                }

                Organisation org = await DataRepository.GetAll<Organisation>()
                    .FirstOrDefaultAsync(o => o.EmployerReference.ToUpper() == employerRef);
                if (org == null)
                {
                    writer.WriteLine(Color.Red, $"{i}: ERROR: '{employerRef}' Cannot find organisation with this employer reference");
                    continue;
                }

                string oldValue = org.CompanyNumber;

                if (oldValue == newValue)
                {
                    writer.WriteLine(
                        Color.Orange,
                        $"{i}: WARNING '{employerRef}' '{org.OrganisationName}' Company Number='{org.CompanyNumber}' already set to '{oldValue}'");
                }
                else if (!string.IsNullOrWhiteSpace(newValue)
                         && await DataRepository.GetAll<Organisation>()
                             .AnyAsync(o => o.CompanyNumber == newValue && o.OrganisationId != org.OrganisationId))
                {
                    writer.WriteLine(Color.Red, $"{i}: ERROR '{employerRef}' Another organisation exists with this company number");
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
                        await Global.ManualChangeLog.WriteAsync(
                            new ManualChangeLogModel(
                                methodName,
                                ManualActions.Update,
                                CurrentUser.EmailAddress,
                                nameof(Organisation.EmployerReference),
                                employerRef,
                                nameof(Organisation.CompanyNumber),
                                oldValue,
                                newValue,
                                comment));
                        await DataRepository.SaveChangesAsync();
                    }
                }

                count++;
            }

            return count;
        }

        private async Task<int> SetOrganisationSicCodesAsync(string input, string comment, StringWriter writer, bool test)
        {
            string methodName = nameof(SetOrganisationSicCodesAsync);

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
                if (!line.Contains('='))
                {
                    writer.WriteLine(Color.Red, $"{i}: ERROR: '{line}' does not contain '=' character");
                    continue;
                }

                // ensure we have value BEFORE the = sign
                string employerRef = line.BeforeFirst("=")?.ToUpper();
                if (string.IsNullOrWhiteSpace(employerRef))
                {
                    continue;
                }

                if (processed.Contains(employerRef))
                {
                    writer.WriteLine(Color.Red, $"{i}: ERROR: '{employerRef}' duplicate organisation");
                    continue;
                }

                processed.Add(employerRef);

                // ensure the employer ref exists
                Organisation org = DataRepository.GetAll<Organisation>()
                    .FirstOrDefault(o => o.EmployerReference.ToLower() == employerRef.ToLower());
                if (org == null)
                {
                    writer.WriteLine(Color.Red, $"{i}: ERROR: '{employerRef}' Cannot find organisation with this employer reference");
                    continue;
                }

                // ensure the org is active
                if (org.Status != OrganisationStatuses.Active)
                {
                    writer.WriteLine(
                        Color.Red,
                        $"{i}: ERROR: '{employerRef}:{org.OrganisationName}' is not an active organisation so you cannot change its SIC codes");
                    continue;
                }

                // ensure the org does not have a company number
                if (string.IsNullOrEmpty(org.CompanyNumber) == false)
                {
                    writer.WriteLine(
                        Color.Red,
                        $"{i}: ERROR: '{employerRef}:{org.OrganisationName}' has a company number so you cannot change this organisation");
                    continue;
                }

                // ensure we have value AFTER the = sign
                string newSicCodes = line.AfterFirst("=", includeWhenNoSeparator: false).TrimI()?.ToUpper();
                if (string.IsNullOrWhiteSpace(newSicCodes))
                {
                    writer.WriteLine(Color.Red, $"{i}: ERROR: '{employerRef}:{org.OrganisationName}' must contain at least one SIC code");
                    continue;
                }

                // ensure all sic codes are integers
                string[] sicCodes = newSicCodes.Trim(' ').Split(',');
                if (sicCodes.Any(x => int.TryParse(x, out int o) == false))
                {
                    writer.WriteLine(Color.Red, $"{i}: ERROR: '{employerRef}:{org.OrganisationName}' you can only input numeric SIC codes");
                    continue;
                }

                // ensure all sic codes exist in db
                var invalidSicCodes = new List<string>();
                var parsedSicCodes = new List<int>();
                foreach (string sc in sicCodes)
                {
                    int parsedSc = int.Parse(sc);
                    if (DataRepository.GetAll<SicCode>().Any(x => x.SicCodeId == parsedSc) == false)
                    {
                        invalidSicCodes.Add(sc);
                    }
                    else
                    {
                        parsedSicCodes.Add(parsedSc);
                    }
                }

                if (invalidSicCodes.Count > 0)
                {
                    writer.WriteLine(
                        Color.Red,
                        $"{i}: ERROR: '{employerRef}:{org.OrganisationName}' the following SIC codes do not exist in the database: {string.Join(",", invalidSicCodes)}'");
                    continue;
                }

                // get all existing sic codes
                string prevSicCodes = org.GetSicCodeIdsString();

                // remove all existing sic codes
                org.GetSicCodes().ForEach(ent => ent.Retired = VirtualDateTime.Now);

                // set new sic codes
                parsedSicCodes.ForEach(
                    x => {
                        var sic = new OrganisationSicCode {Organisation = org, SicCodeId = x, Source = "Manual"};
                        DataRepository.Insert(sic);
                        org.OrganisationSicCodes.Add(sic);
                    });

                //Output the actual execution result
                string oldValue = string.Join(",", prevSicCodes);
                string newValue = string.Join(",", parsedSicCodes);
                string hasBeenWillBe = test ? "will be" : "has been";
                writer.WriteLine($"{i}: {employerRef}:{org.OrganisationName} SIC codes={oldValue} {hasBeenWillBe} set to {newValue}");
                if (!test)
                {
                    await Global.ManualChangeLog.WriteAsync(
                        new ManualChangeLogModel(
                            methodName,
                            ManualActions.Update,
                            CurrentUser.EmailAddress,
                            nameof(Organisation.EmployerReference),
                            employerRef,
                            nameof(Organisation.OrganisationSicCodes),
                            oldValue,
                            newValue,
                            comment));
                    await DataRepository.SaveChangesAsync();
                }

                count++;
            }

            return count;
        }

        private async Task<int> SetOrganisationAddressesAsync(string input, string comment, StringWriter writer, bool test)
        {
            string methodName = nameof(SetOrganisationAddressesAsync);

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
                if (!line.Contains('='))
                {
                    writer.WriteLine(Color.Red, $"{i}: ERROR: '{line}' does not contain '=' character");
                    continue;
                }

                // ensure we have value BEFORE the = sign
                string employerRef = line.BeforeFirst("=")?.ToUpper();
                if (string.IsNullOrWhiteSpace(employerRef))
                {
                    continue;
                }

                if (processed.Contains(employerRef))
                {
                    writer.WriteLine(Color.Red, $"{i}: ERROR: '{employerRef}' duplicate organisation");
                    continue;
                }

                processed.Add(employerRef);

                // ensure the employer ref exists
                Organisation org = DataRepository.GetAll<Organisation>()
                    .FirstOrDefault(o => o.EmployerReference.ToLower() == employerRef.ToLower());
                if (org == null)
                {
                    writer.WriteLine(Color.Red, $"{i}: ERROR: '{employerRef}' Cannot find organisation with this employer reference");
                    continue;
                }

                // ensure the org is active
                if (org.Status != OrganisationStatuses.Active)
                {
                    writer.WriteLine(
                        Color.Red,
                        $"{i}: ERROR: '{employerRef}:{org.OrganisationName}' is not an active organisation so you cannot change its address");
                    continue;
                }

                // ensure the org does not have a company number
                if (string.IsNullOrEmpty(org.CompanyNumber) == false)
                {
                    writer.WriteLine(
                        Color.Red,
                        $"{i}: ERROR: '{employerRef}:{org.OrganisationName}' has a company number so you cannot change this organisation");
                    continue;
                }

                // ensure we have value AFTER the = sign
                string addressEntries = line.AfterFirst("=", includeWhenNoSeparator: false).TrimI();
                if (string.IsNullOrWhiteSpace(addressEntries))
                {
                    writer.WriteLine(Color.Red, $"{i}: ERROR: '{employerRef}:{org.OrganisationName}' must contain an address entry");
                    continue;
                }

                // ensure all address fields are present
                // [Address1],[Address2],[Address3],[TownCity],[County],[Country],[PostCode]
                string[] addressFields = addressEntries.Split(',');
                if (addressFields.Length != 7)
                {
                    writer.WriteLine(
                        Color.Red,
                        $"{i}: ERROR: '{employerRef}:{org.OrganisationName}' doesnt have the correct number of address fields. Expected 7 fields but saw {addressFields.Length} fields");
                    continue;
                }

                // extract fields
                string address1 = addressFields[0];
                string address2 = addressFields[1];
                string address3 = addressFields[2];
                string townCity = addressFields[3];
                string county = addressFields[4];
                string country = addressFields[5];
                string postCode = addressFields[6];

                // ensure mandatory fields are set
                var requiredState = new List<string>();

                if (string.IsNullOrWhiteSpace(address1))
                {
                    requiredState.Add("Address1 is required");
                }

                if (address1.Length > 100)
                {
                    requiredState.Add("Address1 is greater than 100 chars");
                }

                if (string.IsNullOrWhiteSpace(address2) == false && address2.Length > 100)
                {
                    requiredState.Add("Address2 is greater than 100 chars");
                }

                if (string.IsNullOrWhiteSpace(address3) == false && address3.Length > 100)
                {
                    requiredState.Add("Address3 is greater than 100 chars");
                }

                if (string.IsNullOrWhiteSpace(townCity))
                {
                    requiredState.Add("Town\\City is required");
                }

                if (townCity.Length > 100)
                {
                    requiredState.Add("Town\\City is greater than 100 chars");
                }

                if (string.IsNullOrWhiteSpace(county) == false && county.Length > 100)
                {
                    requiredState.Add("County is greater than 100 chars");
                }

                if (string.IsNullOrWhiteSpace(country) == false && country.Length > 100)
                {
                    requiredState.Add("Country is greater than 100 chars");
                }

                if (string.IsNullOrWhiteSpace(postCode))
                {
                    requiredState.Add("Postcode is required");
                }

                if (postCode.Length < 3)
                {
                    requiredState.Add("Postcode is less than 3 chars");
                }

                if (postCode.Length > 100)
                {
                    requiredState.Add("Postcode is greater than 100 chars");
                }

                if (requiredState.Count > 0)
                {
                    writer.WriteLine(Color.Red, $"{i}: ERROR: '{employerRef}:{org.OrganisationName}' {string.Join(",", requiredState)}");
                    continue;
                }

                // add new address
                OrganisationAddress prevAddress = org.LatestAddress;
                var newAddress = new OrganisationAddress {
                    OrganisationId = org.OrganisationId,
                    Address1 = address1,
                    Address2 = address2,
                    Address3 = address3,
                    TownCity = townCity,
                    County = county,
                    Country = country,
                    PostCode = postCode,
                    Source = "Manual"
                };

                newAddress.SetStatus(AddressStatuses.Active, CurrentUser.UserId, $"Inserted by {newAddress.Source}");
                if (prevAddress != null)
                {
                    prevAddress.SetStatus(AddressStatuses.Retired, CurrentUser.UserId, $"Replaced by {newAddress.Source}");
                }

                org.LatestAddress = newAddress;

                DataRepository.Insert(newAddress);
                org.OrganisationAddresses.Add(newAddress);

                //Output the actual execution result
                string oldValue = prevAddress == null ? "No previous address" : prevAddress.GetAddressString();
                string newValue = string.Join(",", addressFields);
                string hasBeenWillBe = test ? "will be" : "has been";
                writer.WriteLine($"{i}: {employerRef}:{org.OrganisationName} Address={oldValue} {hasBeenWillBe} set to {newValue}");
                if (!test)
                {
                    await Global.ManualChangeLog.WriteAsync(
                        new ManualChangeLogModel(
                            methodName,
                            ManualActions.Update,
                            CurrentUser.EmailAddress,
                            nameof(Organisation.EmployerReference),
                            employerRef,
                            nameof(Organisation.LatestAddress),
                            oldValue,
                            newValue,
                            comment));
                    await DataRepository.SaveChangesAsync();
                }

                count++;
            }

            return count;
        }

        private async Task<int> SetPublicSectorTypeAsync(string input, string comment, StringWriter writer, bool test)
        {
            string methodName = nameof(SetPublicSectorTypeAsync);

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
                if (!line.Contains('='))
                {
                    writer.WriteLine(Color.Red, $"{i}: ERROR: '{line}' does not contain '=' character");
                    continue;
                }

                // ensure we have value BEFORE the = sign
                string employerRef = line.BeforeFirst("=")?.ToUpper();
                if (string.IsNullOrWhiteSpace(employerRef))
                {
                    continue;
                }

                if (processed.Contains(employerRef))
                {
                    writer.WriteLine(Color.Red, $"{i}: ERROR: '{employerRef}' duplicate organisation");
                    continue;
                }

                processed.Add(employerRef);

                // ensure the employer ref exists
                Organisation org = DataRepository.GetAll<Organisation>()
                    .FirstOrDefault(o => o.EmployerReference.ToLower() == employerRef.ToLower());
                if (org == null)
                {
                    writer.WriteLine(Color.Red, $"{i}: ERROR: '{employerRef}' Cannot find organisation with this employer reference");
                    continue;
                }

                // ensure the org is active
                if (org.Status != OrganisationStatuses.Active)
                {
                    writer.WriteLine(
                        Color.Red,
                        $"{i}: ERROR: '{employerRef}:{org.OrganisationName}' is not an active organisation so you cannot change its public sector type");
                    continue;
                }

                // ensure the org is public sector
                if (org.SectorType != SectorTypes.Public)
                {
                    writer.WriteLine(Color.Red, $"{i}: ERROR: '{employerRef}:{org.OrganisationName}' is not a public sector organisation");
                    continue;
                }

                // ensure we have value AFTER the = sign
                string enteredClassification = line.AfterFirst("=", includeWhenNoSeparator: false).TrimI();
                if (string.IsNullOrWhiteSpace(enteredClassification))
                {
                    writer.WriteLine(Color.Red, $"{i}: ERROR: '{employerRef}:{org.OrganisationName}' must contain a public sector type");
                    continue;
                }

                // ensure only one public sector type can be entered
                if (enteredClassification.Contains(","))
                {
                    writer.WriteLine(
                        Color.Red,
                        $"{i}: ERROR: '{employerRef}:{org.OrganisationName}' you can only assign one public sector type per organisation'");
                    continue;
                }

                // ensure the public sector type is an integer
                if (int.TryParse(enteredClassification, out int parsedPublicSectorTypeId) == false)
                {
                    writer.WriteLine(
                        Color.Red,
                        $"{i}: ERROR: '{employerRef}:{org.OrganisationName}' you can only input a numeric public sector type");
                    continue;
                }

                // ensure the public sector type exists
                var newSectorType = DataRepository.Get<PublicSectorType>(parsedPublicSectorTypeId);
                if (newSectorType == null)
                {
                    writer.WriteLine(
                        Color.Red,
                        $"{i}: ERROR: '{employerRef}:{org.OrganisationName}' public sector type {parsedPublicSectorTypeId} does not exist");
                    continue;
                }

                // ensure the organisation isn't already set to the specified public sector type
                if (org.LatestPublicSectorType?.PublicSectorTypeId == parsedPublicSectorTypeId)
                {
                    writer.WriteLine(
                        Color.Red,
                        $"{i}: ERROR: '{employerRef}:{org.OrganisationName}' is already set to {parsedPublicSectorTypeId}:{newSectorType.Description}");
                    continue;
                }

                // retire current public sector type
                OrganisationPublicSectorType prevClassification = org.LatestPublicSectorType;
                if (prevClassification != null)
                {
                    prevClassification.Retired = VirtualDateTime.Now;
                }

                // create new public sector type mapping to the org
                var newOrgSectorClass = new OrganisationPublicSectorType {
                    OrganisationId = org.OrganisationId,
                    PublicSectorTypeId = parsedPublicSectorTypeId,
                    PublicSectorType = newSectorType,
                    Source = "Manual"
                };
                DataRepository.Insert(newOrgSectorClass);
                org.LatestPublicSectorType = newOrgSectorClass;

                //Output the actual execution result
                string oldValue = prevClassification == null
                    ? "No previous public sector type"
                    : prevClassification.PublicSectorType.Description;
                string newValue = newSectorType.Description;
                string hasBeenWillBe = test ? "will be" : "has been";
                writer.WriteLine(
                    $"{i}: {employerRef}:{org.OrganisationName} public sector type={oldValue} {hasBeenWillBe} set to {newValue}");
                if (!test)
                {
                    await Global.ManualChangeLog.WriteAsync(
                        new ManualChangeLogModel(
                            methodName,
                            ManualActions.Update,
                            CurrentUser.EmailAddress,
                            nameof(Organisation.EmployerReference),
                            employerRef,
                            nameof(Organisation.LatestPublicSectorType),
                            oldValue,
                            newValue,
                            comment));
                    await DataRepository.SaveChangesAsync();
                }

                count++;
            }

            return count;
        }

        private async Task<int> DeleteSubmissionsAsync(string input, string comment, StringWriter writer, bool test)
        {
            string methodName = nameof(DeleteSubmissionsAsync);

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
                if (!line.Contains('='))
                {
                    writer.WriteLine(Color.Red, $"{i}: ERROR: '{line}' does not contain '=' character");
                    continue;
                }

                string employerRef = line.BeforeFirst("=")?.ToUpper();
                if (string.IsNullOrWhiteSpace(employerRef))
                {
                    continue;
                }

                if (processed.Contains(employerRef))
                {
                    writer.WriteLine(Color.Red, $"{i}: ERROR '{employerRef}' duplicate organisation");
                    continue;
                }

                processed.Add(employerRef);

                Organisation org = await DataRepository.GetAll<Organisation>()
                    .FirstOrDefaultAsync(o => o.EmployerReference.ToUpper() == employerRef);
                if (org == null)
                {
                    writer.WriteLine(Color.Red, $"{i}: ERROR: '{employerRef}' Cannot find organisation with this employer reference");
                    continue;
                }

                string number = line.AfterFirst("=", includeWhenNoSeparator: false).TrimI();
                if (number == null || !string.IsNullOrWhiteSpace(number) && !number.IsNumber())
                {
                    writer.WriteLine(Color.Red, $"{i}: ERROR: '{employerRef}' invalid year '{number}'");
                    continue;
                }

                int year = number.ToInt32(org.SectorType.GetAccountingStartDate().Year);
                Return @return = org.Returns.OrderByDescending(o => o.StatusDate)
                    .FirstOrDefault(r => r.Status == ReturnStatuses.Submitted && r.AccountingDate.Year == year);
                if (@return == null)
                {
                    writer.WriteLine(Color.Orange, $"{i}: WARNING: '{employerRef}' Cannot find submitted data for year {year}");
                    continue;
                }

                var newValue = ReturnStatuses.Deleted;
                ReturnStatuses oldValue = @return.Status;

                //Output the actual execution result
                @return.SetStatus(newValue, CurrentUser.UserId, comment);

                writer.WriteLine($"{i}: {employerRef}: {org.OrganisationName} Year='{year}' Status='{oldValue}' set to '{newValue}'");
                if (!test)
                {
                    await Global.ManualChangeLog.WriteAsync(
                        new ManualChangeLogModel(
                            methodName,
                            ManualActions.Update,
                            CurrentUser.EmailAddress,
                            nameof(Return.ReturnId),
                            @return.ReturnId.ToString(),
                            nameof(Return.Status),
                            oldValue.ToString(),
                            newValue.ToString(),
                            comment,
                            $"Year={year} Employer='{employerRef}'"));
                    await DataRepository.SaveChangesAsync();
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
                    writer.WriteLine(Color.Red, $"{i}: ERROR: '{line}' contains '=' character");
                    continue;
                }

                string employerRef = line?.ToUpper();
                if (string.IsNullOrWhiteSpace(employerRef))
                {
                    continue;
                }

                if (processed.Contains(employerRef))
                {
                    writer.WriteLine(Color.Red, $"{i}: ERROR: '{employerRef}' duplicate organisation");
                    continue;
                }

                processed.Add(employerRef);

                var newSector = SectorTypes.Public;

                Organisation org = await DataRepository.GetAll<Organisation>()
                    .FirstOrDefaultAsync(o => o.EmployerReference.ToUpper() == employerRef);
                if (org == null)
                {
                    writer.WriteLine(Color.Red, $"{i}: ERROR: '{employerRef}' Cannot find organisation with this employer reference");
                    continue;
                }

                SectorTypes oldSector = org.SectorType;

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
                        Color.Orange,
                        $"{i}: WARNING: '{employerRef}' '{org.OrganisationName}' sector already set to '{oldSector}'");
                }
                else
                {
                    if (oldSector != newSector)
                    {
                        //Change the sector type
                        org.SectorType = newSector;
                        if (!test)
                        {
                            await Global.ManualChangeLog.WriteAsync(
                                new ManualChangeLogModel(
                                    methodName,
                                    ManualActions.Update,
                                    CurrentUser.EmailAddress,
                                    nameof(Organisation.EmployerReference),
                                    employerRef,
                                    nameof(Organisation.SectorType),
                                    oldSector.ToString(),
                                    newSector.ToString(),
                                    comment));
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
                            await Global.ManualChangeLog.WriteAsync(
                                new ManualChangeLogModel(
                                    methodName,
                                    ManualActions.Create,
                                    CurrentUser.EmailAddress,
                                    nameof(Organisation.EmployerReference),
                                    employerRef,
                                    nameof(OrganisationSicCode),
                                    null,
                                    "1",
                                    comment));
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
                                await Global.ManualChangeLog.WriteAsync(
                                    new ManualChangeLogModel(
                                        methodName,
                                        ManualActions.Update,
                                        CurrentUser.EmailAddress,
                                        nameof(@return.ReturnId),
                                        @return.ReturnId.ToString(),
                                        nameof(Return.AccountingDate),
                                        oldDate.ToString(),
                                        newDate.ToString(),
                                        comment));
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
                                await Global.ManualChangeLog.WriteAsync(
                                    new ManualChangeLogModel(
                                        methodName,
                                        ManualActions.Update,
                                        CurrentUser.EmailAddress,
                                        nameof(scope.OrganisationScopeId),
                                        scope.OrganisationScopeId.ToString(),
                                        nameof(scope.SnapshotDate),
                                        oldDate.ToString(),
                                        newDate.ToString(),
                                        comment));
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
                    writer.WriteLine(Color.Red, $"{i}: ERROR: '{line}' contains '=' character");
                    continue;
                }

                string employerRef = line?.ToUpper();
                if (string.IsNullOrWhiteSpace(employerRef))
                {
                    continue;
                }

                if (processed.Contains(employerRef))
                {
                    writer.WriteLine(Color.Red, $"{i}: '{employerRef}' duplicate organisation");
                    continue;
                }

                processed.Add(employerRef);

                var newSector = SectorTypes.Private;

                Organisation org = await DataRepository.GetAll<Organisation>()
                    .FirstOrDefaultAsync(o => o.EmployerReference.ToUpper() == employerRef);
                if (org == null)
                {
                    writer.WriteLine(Color.Red, $"{i}: ERROR: '{employerRef}' Cannot find organisation with this employer reference");
                    continue;
                }

                SectorTypes oldSector = org.SectorType;

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
                        Color.Orange,
                        $"{i}: WARNING: '{employerRef}' '{org.OrganisationName}' sector already set to '{oldSector}'");
                }
                else
                {
                    //Change the sector type
                    if (oldSector != newSector)
                    {
                        org.SectorType = newSector;
                        if (!test)
                        {
                            await Global.ManualChangeLog.WriteAsync(
                                new ManualChangeLogModel(
                                    methodName,
                                    ManualActions.Update,
                                    CurrentUser.EmailAddress,
                                    nameof(Organisation.EmployerReference),
                                    employerRef,
                                    nameof(Organisation.SectorType),
                                    oldSector.ToString(),
                                    newSector.ToString(),
                                    comment));
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
                                await Global.ManualChangeLog.WriteAsync(
                                    new ManualChangeLogModel(
                                        methodName,
                                        ManualActions.Delete,
                                        CurrentUser.EmailAddress,
                                        nameof(Organisation.EmployerReference),
                                        employerRef,
                                        nameof(OrganisationSicCode),
                                        JsonConvert.SerializeObject(
                                            new {
                                                sic.OrganisationSicCodeId,
                                                sic.SicCodeId,
                                                sic.OrganisationId,
                                                sic.Source,
                                                sic.Created,
                                                sic.Retired
                                            }),
                                        null,
                                        comment));
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
                                await Global.ManualChangeLog.WriteAsync(
                                    new ManualChangeLogModel(
                                        methodName,
                                        ManualActions.Update,
                                        CurrentUser.EmailAddress,
                                        nameof(@return.ReturnId),
                                        @return.ReturnId.ToString(),
                                        nameof(Return.AccountingDate),
                                        oldDate.ToString(),
                                        newDate.ToString(),
                                        comment));
                            }

                            if (string.IsNullOrWhiteSpace(@return.ResponsiblePerson))
                            {
                                writer.WriteLine(
                                    Color.Orange,
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
                                await Global.ManualChangeLog.WriteAsync(
                                    new ManualChangeLogModel(
                                        methodName,
                                        ManualActions.Update,
                                        CurrentUser.EmailAddress,
                                        nameof(scope.OrganisationScopeId),
                                        scope.OrganisationScopeId.ToString(),
                                        nameof(scope.SnapshotDate),
                                        oldDate.ToString(),
                                        newDate.ToString(),
                                        comment));
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

        /// <summary>
        ///     Sets the latest company name
        /// </summary>
        private async Task<int> SetOrganisationNameAsync(string input, string comment, StringWriter writer, bool test)
        {
            string methodName = nameof(SetOrganisationNameAsync);

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
                if (!line.Contains('='))
                {
                    writer.WriteLine(Color.Red, $"{i}: ERROR: '{line}' does not contain '=' character");
                    continue;
                }

                string employerRef = line.BeforeFirst("=")?.ToUpper();
                if (string.IsNullOrWhiteSpace(employerRef))
                {
                    continue;
                }

                if (processed.Contains(employerRef))
                {
                    writer.WriteLine(Color.Red, $"{i}: ERROR: '{employerRef}' duplicate organisation");
                    continue;
                }

                processed.Add(employerRef);
                string newValue = line.AfterFirst("=", includeWhenNoSeparator: false).TrimI();
                if (string.IsNullOrWhiteSpace(newValue))
                {
                    writer.WriteLine(Color.Red, $"{i}: ERROR: '{employerRef}' No organisation name specified");
                    continue;
                }

                Organisation org = await DataRepository.GetAll<Organisation>()
                    .FirstOrDefaultAsync(o => o.EmployerReference.ToUpper() == employerRef);
                if (org == null)
                {
                    writer.WriteLine(Color.Red, $"{i}: ERROR: '{employerRef}' Cannot find organisation with this employer reference");
                    continue;
                }

                string oldValue = org.OrganisationName;
                if (oldValue == newValue)
                {
                    writer.WriteLine(Color.Orange, $"{i}: WARNING: '{employerRef}' '{org.OrganisationName}' already set to '{newValue}'");
                }
                else if (await DataRepository.GetAll<Organisation>()
                    .AnyAsync(o => o.OrganisationName.ToLower() == newValue.ToLower() && o.OrganisationId != org.OrganisationId))
                {
                    writer.WriteLine(Color.Red, $"{i}: ERROR: '{employerRef}' Another organisation exists with this company name");
                    continue;
                }
                else
                {
                    //Output the actual execution result
                    org.OrganisationName = newValue;
                    if (!test)
                    {
                        await Global.ManualChangeLog.WriteAsync(
                            new ManualChangeLogModel(
                                methodName,
                                ManualActions.Update,
                                CurrentUser.EmailAddress,
                                nameof(Organisation.EmployerReference),
                                employerRef,
                                nameof(Organisation.OrganisationName),
                                oldValue,
                                newValue,
                                comment));
                    }

                    org.OrganisationNames.Add(new OrganisationName {Organisation = org, Source = "Manual", Name = newValue});
                    if (!test)
                    {
                        await Global.ManualChangeLog.WriteAsync(
                            new ManualChangeLogModel(
                                methodName,
                                ManualActions.Create,
                                CurrentUser.EmailAddress,
                                nameof(Organisation.EmployerReference),
                                employerRef,
                                nameof(Organisation.OrganisationName),
                                oldValue,
                                newValue,
                                comment));
                    }

                    writer.WriteLine($"{i}: {employerRef}: '{oldValue}' set to '{newValue}'");
                    if (!test)
                    {
                        await DataRepository.SaveChangesAsync();
                    }
                }

                count++;
            }

            return count;
        }

        /// <summary>
        ///     Removes all previous names and sets the first company name
        /// </summary>
        private async Task<int> ResetOrganisationNameAsync(string input, string comment, StringWriter writer, bool test)
        {
            string methodName = nameof(ResetOrganisationNameAsync);

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
                if (!line.Contains('='))
                {
                    writer.WriteLine(Color.Red, $"{i}: ERROR: '{line}' does not contain '=' character");
                    continue;
                }

                string employerRef = line.BeforeFirst("=")?.ToUpper();
                if (string.IsNullOrWhiteSpace(employerRef))
                {
                    continue;
                }

                if (processed.Contains(employerRef))
                {
                    writer.WriteLine(Color.Red, $"{i}: ERROR: '{employerRef}' duplicate organisation");
                    continue;
                }

                processed.Add(employerRef);
                string newValue = line.AfterFirst("=", includeWhenNoSeparator: false).TrimI();
                if (string.IsNullOrWhiteSpace(newValue))
                {
                    writer.WriteLine(Color.Red, $"{i}: ERROR: '{employerRef}' No organisation name specified");
                    continue;
                }

                Organisation org = await DataRepository.GetAll<Organisation>()
                    .FirstOrDefaultAsync(o => o.EmployerReference.ToUpper() == employerRef);
                if (org == null)
                {
                    writer.WriteLine(Color.Red, $"{i}: ERROR: '{employerRef}' Cannot find organisation with this employer reference");
                    continue;
                }

                string oldValue = org.OrganisationName;
                if (oldValue == newValue && org.OrganisationNames.Count() == 1)
                {
                    writer.WriteLine(Color.Orange, $"{i}: WARNING: '{employerRef}' '{org.OrganisationName}' already set to '{newValue}'");
                }
                else if (await DataRepository.GetAll<Organisation>()
                    .AnyAsync(o => o.OrganisationName.ToLower() == newValue.ToLower() && o.OrganisationId != org.OrganisationId))
                {
                    writer.WriteLine(Color.Red, $"{i}: ERROR: '{employerRef}' Another organisation exists with this company name");
                    continue;
                }
                else
                {
                    if (oldValue != newValue)
                    {
                        //Output the actual execution result
                        org.OrganisationName = newValue;
                        if (!test)
                        {
                            await Global.ManualChangeLog.WriteAsync(
                                new ManualChangeLogModel(
                                    methodName,
                                    ManualActions.Update,
                                    CurrentUser.EmailAddress,
                                    nameof(Organisation.EmployerReference),
                                    employerRef,
                                    nameof(Organisation.OrganisationName),
                                    oldValue,
                                    newValue,
                                    comment));
                        }
                    }

                    if (org.OrganisationName.Count() != 1)
                    {
                        foreach (OrganisationName name in org.OrganisationNames.ToList())
                        {
                            if (!test)
                            {
                                await Global.ManualChangeLog.WriteAsync(
                                    new ManualChangeLogModel(
                                        methodName,
                                        ManualActions.Delete,
                                        CurrentUser.EmailAddress,
                                        nameof(Organisation.EmployerReference),
                                        employerRef,
                                        nameof(OrganisationName),
                                        JsonConvert.SerializeObject(
                                            new {
                                                name.OrganisationNameId,
                                                name.OrganisationId,
                                                name.Name,
                                                name.Created,
                                                name.Source
                                            }),
                                        null,
                                        comment));
                            }

                            DataRepository.Delete(name);
                        }

                        org.OrganisationNames.Add(
                            new OrganisationName {Organisation = org, Source = "Manual", Name = newValue, Created = org.Created});
                        if (!test)
                        {
                            await Global.ManualChangeLog.WriteAsync(
                                new ManualChangeLogModel(
                                    methodName,
                                    ManualActions.Create,
                                    CurrentUser.EmailAddress,
                                    nameof(Organisation.EmployerReference),
                                    employerRef,
                                    nameof(Organisation.OrganisationName),
                                    oldValue,
                                    newValue,
                                    comment));
                        }
                    }

                    writer.WriteLine($"{i}: {employerRef}: '{oldValue}' set to '{newValue}'");
                    if (!test)
                    {
                        await DataRepository.SaveChangesAsync();
                    }
                }

                count++;
            }

            return count;
        }

        public delegate Task<CustomResult<Organisation>> SecurityCodeDelegate(string employerRef, DateTime securityCodeExpiryDateTime);

        private async Task<int> SecurityCodeWorkAsync(string input,
            string comment,
            StringWriter writer,
            bool test,
            ManualActions manualAction,
            SecurityCodeDelegate callBackDelegatedMethod)
        {
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
            var listOfModifiedOrgs = new HashSet<Organisation>();
            foreach (string line in lines)
            {
                string outcome = line;

                i++;

                if (!line.Contains('='))
                {
                    writer.WriteLine(Color.Red, $"{i}: ERROR: '{line}' does not contain '=' character");
                    continue;
                }

                if (!HasEmployerReference(ref outcome, out string employerRef))
                {
                    writer.WriteLine(
                        Color.Red,
                        $"{i}: ERROR: '{line}' expected an employer reference before the '=' character (i.e. EmployerReference=dd/mm/yyyy to know which employer to modify)");
                    continue;
                }

                if (!HasDateTimeInfo(ref outcome, out DateTime extractedDateTime))
                {
                    writer.WriteLine(
                        Color.Red,
                        $"{i}: ERROR: '{line}' expected a valid (dd/mm/yyyy) date value after the '=' character (i.e. EmployerReference=dd/mm/yyyy to know when to expire the security codes created for this employer)");
                    continue;
                }

                if (processed.Contains(employerRef))
                {
                    writer.WriteLine(Color.Red, $"{i}: ERROR: '{employerRef}' duplicate organisation");
                    continue;
                }

                processed.Add(employerRef);

                try
                {
                    CustomResult<Organisation> securityCodeWorksOutcome = await callBackDelegatedMethod(employerRef, extractedDateTime);

                    if (securityCodeWorksOutcome.Failed)
                    {
                        writer.WriteLine(
                            Color.Red,
                            $"{i}: ERROR: '{securityCodeWorksOutcome.ErrorRelatedObject}' {securityCodeWorksOutcome.ErrorMessage}");
                        continue;
                    }

                    if (securityCodeWorksOutcome.Result != null)
                    {
                        listOfModifiedOrgs.Add(securityCodeWorksOutcome.Result);
                    }

                    string hasBeenWillBe = test ? "will be" : "has been";
                    string createdOrExtended = manualAction == ManualActions.Extend ? "extended" : "created";
                    string securityCodeHiddenOrShow =
                        test ? new string('*', Global.PINLength) : $"{securityCodeWorksOutcome.Result.SecurityCode}";
                    writer.WriteLine(
                        $"{i}: {securityCodeWorksOutcome.Result}: {hasBeenWillBe} {createdOrExtended} to be '{securityCodeHiddenOrShow}' and will expire on '{securityCodeWorksOutcome.Result.SecurityCodeExpiryDateTime:dd/MMMM/yyyy}'");

                    if (!test)
                    {
                        string serialisedInfo = JsonConvert.SerializeObject(
                            new {
                                securityCodeWorksOutcome.Result.SecurityCode,
                                securityCodeWorksOutcome.Result.SecurityCodeExpiryDateTime,
                                securityCodeWorksOutcome.Result.SecurityCodeCreatedDateTime,
                                CurrentUser.EmailAddress
                            });

                        await Global.ManualChangeLog.WriteAsync(
                            new ManualChangeLogModel {
                                MethodName = $"{manualAction.ToString()}SecurityCode",
                                Action = manualAction,
                                Source = CurrentUser.EmailAddress,
                                Comment = comment,
                                ReferenceName = nameof(Organisation.EmployerReference),
                                ReferenceValue = employerRef,
                                TargetName = nameof(Organisation.SecurityCode),
                                TargetNewValue = serialisedInfo
                            });
                    }
                }
                catch (Exception ex)
                {
                    writer.WriteLine(Color.Red, $"{i}: ERROR: '{employerRef}' {ex.Message}");
                    continue;
                }

                count++;
            }

            if (!test && listOfModifiedOrgs.Count > 0)
            {
                await DataRepository.SaveChangesAsync();
                writer.WriteLine(Color.Green, "INFO: Changes saved to database");
            }

            return count;
        }

        private async Task<int> ExpireSecurityCodeAsync(string input,
            string comment,
            StringWriter writer,
            bool test,
            ManualActions manualAction)
        {
            string methodName = MethodBase.GetCurrentMethod().Name;

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
            var listOfModifiedOrgs = new HashSet<Organisation>();
            foreach (string line in lines)
            {
                string outcome = line;

                i++;

                if (line.Contains('='))
                {
                    writer.WriteLine(Color.Red, $"{i}: ERROR: '{line}' must not contain '=' character");
                    continue;
                }

                if (!HasEmployerReference(ref outcome, out string employerRef))
                {
                    writer.WriteLine(
                        Color.Red,
                        $"{i}: ERROR: '{line}' expected a valid employer reference to know which employer to modify");
                    continue;
                }

                if (processed.Contains(employerRef))
                {
                    writer.WriteLine(Color.Red, $"{i}: ERROR: '{employerRef}' duplicate organisation");
                    continue;
                }

                processed.Add(employerRef);

                try
                {
                    CustomResult<Organisation> securityCodeWorksOutcome =
                        await OrganisationBusinessLogic.ExpireSecurityCodeAsync(employerRef);

                    if (securityCodeWorksOutcome.Failed)
                    {
                        writer.WriteLine(
                            Color.Red,
                            $"{i}: ERROR: '{securityCodeWorksOutcome.ErrorRelatedObject}' {securityCodeWorksOutcome.ErrorMessage}");
                        continue;
                    }

                    if (securityCodeWorksOutcome.Result != null)
                    {
                        listOfModifiedOrgs.Add(securityCodeWorksOutcome.Result);
                    }

                    string hasBeenWillBe = test ? "will be" : "has been";
                    writer.WriteLine($"{i}: {securityCodeWorksOutcome.Result}: {hasBeenWillBe} expired.");

                    if (!test)
                    {
                        string serialisedInfo = JsonConvert.SerializeObject(
                            new {
                                securityCodeWorksOutcome.Result.SecurityCode,
                                securityCodeWorksOutcome.Result.SecurityCodeExpiryDateTime,
                                securityCodeWorksOutcome.Result.SecurityCodeCreatedDateTime,
                                CurrentUser.EmailAddress
                            });

                        if (!test)
                        {
                            await Global.ManualChangeLog.WriteAsync(
                                new ManualChangeLogModel {
                                    MethodName = methodName,
                                    Action = manualAction,
                                    Source = CurrentUser.EmailAddress,
                                    Comment = comment,
                                    ReferenceName = nameof(Organisation.EmployerReference),
                                    ReferenceValue = employerRef,
                                    TargetName = nameof(Organisation.SecurityCode),
                                    TargetNewValue = serialisedInfo
                                });
                        }
                    }
                }
                catch (Exception ex)
                {
                    writer.WriteLine(Color.Red, $"{i}: ERROR: '{employerRef}' {ex.Message}");
                    continue;
                }

                count++;
            }

            if (!test && listOfModifiedOrgs.Count > 0)
            {
                await DataRepository.SaveChangesAsync();
                writer.WriteLine(Color.Green, "INFO: Changes saved to database");
            }

            return count;
        }

        public delegate Task<CustomBulkResult<Organisation>> SecurityCodeBulkDelegate(DateTime securityCodeExpiryDateTime);

        private async Task<BulkResult> SecurityCodeBulkWorkAsync(string input,
            string comment,
            StringWriter writer,
            bool test,
            ManualActions manualAction,
            SecurityCodeBulkDelegate callBackBulkDelegatedMethod)
        {
            var result = new BulkResult();
            string methodName = MethodBase.GetCurrentMethod().Name;

            if (string.IsNullOrEmpty(input))
            {
                throw new ArgumentException("ERROR: You must supply the expiry date for the security codes");
            }

            if (input.Contains('='))
            {
                writer.WriteLine(Color.Red, $"ERROR: '{input}' must not contain '=' character");
                return result;
            }

            if (!HasDateTimeInfo(ref input, out DateTime extractedDateTime))
            {
                writer.WriteLine(
                    Color.Red,
                    $"ERROR: '{input}' expected a valid (dd/mm/yyyy) date value to know when to expire the security codes");
                return result;
            }

            try
            {
                CustomBulkResult<Organisation> securityCodeBulkWorkOutcome = await callBackBulkDelegatedMethod(extractedDateTime);

                if (securityCodeBulkWorkOutcome.Failed)
                {
                    var groupedErrors = securityCodeBulkWorkOutcome.ConcurrentBagOfErrors
                        .GroupBy(x => new {x.ErrorMessage.Code, x.ErrorMessage.Description})
                        .Select(
                            r => new {
                                Total = r.Count(), // count of similar errors
                                r.Key.Code,
                                r.Key.Description
                            });
                    foreach (var reportedError in groupedErrors)
                    {
                        writer.WriteLine(
                            Color.Red,
                            $"{reportedError.Total} ERROR(S) of type '{reportedError.Code}' {reportedError.Description}");
                    }
                }

                int numberOfSuccesses = securityCodeBulkWorkOutcome.ConcurrentBagOfSuccesses.Count;
                int numberOfFailures = securityCodeBulkWorkOutcome.ConcurrentBagOfErrors.Count;

                string hasBeenWillBe = test ? "will be" : "have been";
                string hasBeenWillBeSuccessfully = test ? hasBeenWillBe : $"{hasBeenWillBe} successfully";
                string failureMessage = numberOfFailures > 0
                    ? $"A Total of {numberOfFailures} records FAILED and {hasBeenWillBe} ignored. " // The space at the end of this message is required for presentation.
                    : string.Empty;
                string successMessage = numberOfSuccesses > 0
                    ? $"A total of {numberOfSuccesses} security codes {hasBeenWillBeSuccessfully} set to expire on '{extractedDateTime:dd/MMMM/yyyy}'"
                    : string.Empty;

                writer.WriteLine($"{failureMessage}{successMessage}");

                /* Completed List of all individual issues */
                if (securityCodeBulkWorkOutcome.Failed)
                {
                    foreach (CustomResult<Organisation> detailedError in securityCodeBulkWorkOutcome.ConcurrentBagOfErrors)
                    {
                        string orgDetails = detailedError.ErrorRelatedObject != null
                            ? detailedError.ErrorRelatedObject.ToString()
                            : "[null]";
                        writer.Write($"[{orgDetails} {detailedError.ErrorMessage.Code} '{detailedError.ErrorMessage.Description}'] ");
                    }
                }

                if (!test)
                {
                    await Global.ManualChangeLog.WriteAsync(
                        new ManualChangeLogModel {
                            MethodName = methodName,
                            Action = manualAction,
                            Source = CurrentUser.EmailAddress,
                            Comment = comment,
                            TargetName = nameof(Organisation.SecurityCode),
                            TargetNewValue = $"{failureMessage}{successMessage}"
                        });
                }

                if (!test && securityCodeBulkWorkOutcome.ConcurrentBagOfSuccesses.Count > 0)
                {
                    DataRepository.UpdateChangesInBulk(securityCodeBulkWorkOutcome.ConcurrentBagOfSuccesses);
                    writer.WriteLine(Color.Green, "INFO: Changes saved to database");
                }

                result.Count = numberOfSuccesses;
                result.TotalRecords = numberOfSuccesses + numberOfFailures;
            }
            catch (Exception ex)
            {
                writer.WriteLine(Color.Red, $"ERROR: {ex.Message}");
            }

            return result;
        }

        private async Task<BulkResult> ExpireSecurityCodeBulkWorkAsync(string input,
            string comment,
            StringWriter writer,
            bool test,
            ManualActions manualAction)
        {
            var result = new BulkResult();
            string methodName = MethodBase.GetCurrentMethod().Name;

            if (!string.IsNullOrWhiteSpace(input))
            {
                throw new ArgumentException("ERROR: parameters must be empty");
            }

            try
            {
                CustomBulkResult<Organisation> securityCodeBulkWorkOutcome =
                    await OrganisationBusinessLogic.ExpireSecurityCodesInBulkAsync();

                if (securityCodeBulkWorkOutcome.Failed)
                {
                    var groupedErrors = securityCodeBulkWorkOutcome.ConcurrentBagOfErrors
                        .GroupBy(x => new {x.ErrorMessage.Code, x.ErrorMessage.Description})
                        .Select(
                            r => new {
                                Total = r.Count(), // count of similar errors
                                r.Key.Code,
                                r.Key.Description
                            });
                    foreach (var reportedError in groupedErrors)
                    {
                        writer.WriteLine(
                            Color.Red,
                            $"{reportedError.Total} ERROR(S) of type '{reportedError.Code}' {reportedError.Description}");
                    }
                }

                int numberOfSuccesses = securityCodeBulkWorkOutcome.ConcurrentBagOfSuccesses.Count;
                int numberOfFailures = securityCodeBulkWorkOutcome.ConcurrentBagOfErrors.Count;

                string hasBeenWillBe = test ? "will be" : "have been";
                string hasBeenWillBeSuccessfully = test ? hasBeenWillBe : $"{hasBeenWillBe} successfully";
                string failureMessage = numberOfFailures > 0
                    ? $"A Total of {numberOfFailures} records FAILED and {hasBeenWillBe} ignored. "
                    : string.Empty;
                string successMessage = numberOfSuccesses > 0
                    ? $"A total of {numberOfSuccesses} security codes {hasBeenWillBeSuccessfully} expired."
                    : string.Empty;

                writer.WriteLine($"{failureMessage}{successMessage}");

                /* Completed List of all individual issues */
                if (securityCodeBulkWorkOutcome.Failed)
                {
                    foreach (CustomResult<Organisation> detailedError in securityCodeBulkWorkOutcome.ConcurrentBagOfErrors)
                    {
                        string orgDetails = detailedError.ErrorRelatedObject != null
                            ? detailedError.ErrorRelatedObject.ToString()
                            : "[null]";
                        writer.Write($"[{orgDetails} {detailedError.ErrorMessage.Code} '{detailedError.ErrorMessage.Description}'] ");
                    }
                }

                if (!test)
                {
                    await Global.ManualChangeLog.WriteAsync(
                        new ManualChangeLogModel {
                            MethodName = methodName,
                            Action = manualAction,
                            Source = CurrentUser.EmailAddress,
                            Comment = comment,
                            TargetName = nameof(Organisation.SecurityCode),
                            TargetNewValue = $"{failureMessage}{successMessage}"
                        });
                }

                if (!test && securityCodeBulkWorkOutcome.ConcurrentBagOfSuccesses.Count > 0)
                {
                    DataRepository.UpdateChangesInBulk(securityCodeBulkWorkOutcome.ConcurrentBagOfSuccesses);
                    writer.WriteLine(Color.Green, "INFO: Changes saved to database");
                }

                result.TotalRecords = numberOfSuccesses + numberOfFailures;
                result.Count = numberOfSuccesses;
            }
            catch (Exception ex)
            {
                writer.WriteLine(Color.Red, $"ERROR: {ex.Message}");
            }

            return result;
        }

        #endregion

    }
}
