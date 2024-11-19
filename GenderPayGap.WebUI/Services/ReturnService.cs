using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using GenderPayGap.Core;
using GenderPayGap.Core.Classes;
using GenderPayGap.Core.Interfaces;
using GenderPayGap.Database;
using GenderPayGap.Database.Models;
using GenderPayGap.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace GenderPayGap.WebUI.Services
{
    public class ReturnService
    {

        private readonly IDataRepository dataRepository;
        private readonly EmailSendingService emailSendingService;

        public ReturnService(
            IDataRepository dataRepository,
            EmailSendingService emailSendingService)
        {
            this.dataRepository = dataRepository;
            this.emailSendingService = emailSendingService;
        }


        public Return CreateAndSaveReturnFromDraftReturn(DraftReturn draftReturn, User user, IUrlHelper urlHelper)
        {
            // Whilst this Return isn't itself late, it might be a non-figures edit to an already-late Return
            Organisation organisation = dataRepository.Get<Organisation>(draftReturn.OrganisationId);
            Return existingReturn = organisation.GetReturn(draftReturn.SnapshotYear);

            if (existingReturn != null && existingReturn.IsLateSubmission)
            {
                return CreateAndSaveReturnFromDraftReturn(draftReturn, user, urlHelper, isLateSubmission: true, lateReason: existingReturn.LateReason, receivedLetterFromEhrc: existingReturn.EHRCResponse);
            }

            return CreateAndSaveReturnFromDraftReturn(draftReturn, user, urlHelper, isLateSubmission: false, lateReason: null, receivedLetterFromEhrc: false);
        }

        public Return CreateAndSaveLateReturnFromDraftReturn(
            DraftReturn draftReturn,
            User user,
            IUrlHelper urlHelper,
            string lateReason,
            bool receivedLetterFromEhrc)
        {
            return CreateAndSaveReturnFromDraftReturn(draftReturn, user, urlHelper, true, lateReason, receivedLetterFromEhrc);
        }

        private Return CreateAndSaveReturnFromDraftReturn(
            DraftReturn draftReturn,
            User user,
            IUrlHelper urlHelper,
            bool isLateSubmission,
            string lateReason,
            bool receivedLetterFromEhrc)
        {
            // Don't save if no changes (to do later - when we get to editing returns)
            // Construct "Modifications" field (to do later - when we get to editing returns)

            // Create the Return
            Return newReturn = CreateReturnFromDraftReturn(draftReturn, isLateSubmission, lateReason, receivedLetterFromEhrc);
            dataRepository.Insert(newReturn);
            dataRepository.SaveChanges();

            // Set the Return Status using SetStatus (creates a ReturnStatus object) (might need to do this after initial save?)
            newReturn.SetStatus(ReturnStatuses.Submitted, user.UserId);

            // Retire the old Returns
            RetireOldReturnsForSameYear(newReturn, user);
            // Discard draft return
            dataRepository.Delete(draftReturn);
            dataRepository.SaveChanges();

            // Send emails
            SendGeoFirstTimeSubmissionEmail(newReturn, urlHelper);
            SendSuccessfulSubmissionEmailToRegisteredUsers(newReturn, urlHelper);

            return newReturn;
        }

        private Return CreateReturnFromDraftReturn(
            DraftReturn draftReturn,
            bool isLateSubmission,
            string lateReason,
            bool receivedLetterFromEhrc)
        {
            Organisation organisation = dataRepository.Get<Organisation>(draftReturn.OrganisationId);
            Return existingReturn = organisation.GetReturn(draftReturn.SnapshotYear);
            var organisationSizeRange = draftReturn.OrganisationSize?.GetAttribute<RangeAttribute>();

            var newReturn = new Return
            {
                OrganisationId = draftReturn.OrganisationId,
                Organisation = organisation,

                AccountingDate = organisation.SectorType.GetAccountingStartDate(draftReturn.SnapshotYear),

                DiffMeanHourlyPayPercent = draftReturn.DiffMeanHourlyPayPercent.Value,
                DiffMedianHourlyPercent = draftReturn.DiffMedianHourlyPercent.Value,

                MaleMedianBonusPayPercent = draftReturn.MaleMedianBonusPayPercent.Value,
                FemaleMedianBonusPayPercent = draftReturn.FemaleMedianBonusPayPercent.Value,
                DiffMeanBonusPercent = draftReturn.DiffMeanBonusPercent,
                DiffMedianBonusPercent = draftReturn.DiffMedianBonusPercent,
                
                OptedOutOfReportingPayQuarters = draftReturn.OptedOutOfReportingPayQuarters,

                MaleUpperQuartilePayBand = draftReturn.MaleUpperQuartilePayBand,
                FemaleUpperQuartilePayBand = draftReturn.FemaleUpperQuartilePayBand,
                MaleUpperPayBand = draftReturn.MaleUpperPayBand,
                FemaleUpperPayBand = draftReturn.FemaleUpperPayBand,
                MaleMiddlePayBand = draftReturn.MaleMiddlePayBand,
                FemaleMiddlePayBand = draftReturn.FemaleMiddlePayBand,
                MaleLowerPayBand = draftReturn.MaleLowerPayBand,
                FemaleLowerPayBand = draftReturn.FemaleLowerPayBand,

                FirstName = draftReturn.FirstName,
                LastName = draftReturn.LastName,
                JobTitle = draftReturn.JobTitle,

                CompanyLinkToGPGInfo = draftReturn.CompanyLinkToGPGInfo,

                IsLateSubmission = isLateSubmission,
                LateReason = lateReason,
                EHRCResponse = receivedLetterFromEhrc,

                // We will set the status to Submitted soon
                // But, we want to use the Return.SetStatus method (which required the Return to already be saved
                // But, to save the Return, we need an initial status
                // Maybe we should see if we can reduce this complexity!
                Status = ReturnStatuses.Draft
            };

            if (organisationSizeRange != null)
            {
                newReturn.MinEmployees = (int) organisationSizeRange.Minimum;
                newReturn.MaxEmployees = (int) organisationSizeRange.Maximum;
            }

            newReturn.Modifications = CalculateModifications(newReturn, existingReturn);

            return newReturn;
        }

        private string CalculateModifications(Return newReturn, Return existingReturn)
        {
            if (existingReturn == null)
            {
                return null;
            }

            var modifications = new List<string>();

            if (newReturn.DiffMeanHourlyPayPercent != existingReturn.DiffMeanHourlyPayPercent
                || newReturn.DiffMedianHourlyPercent != existingReturn.DiffMedianHourlyPercent
                
                || newReturn.MaleMedianBonusPayPercent != existingReturn.MaleMedianBonusPayPercent
                || newReturn.FemaleMedianBonusPayPercent != existingReturn.FemaleMedianBonusPayPercent
                || newReturn.DiffMeanBonusPercent != existingReturn.DiffMeanBonusPercent
                || newReturn.DiffMedianBonusPercent != existingReturn.DiffMedianBonusPercent

                || newReturn.MaleUpperQuartilePayBand != existingReturn.MaleUpperQuartilePayBand
                || newReturn.FemaleUpperQuartilePayBand != existingReturn.FemaleUpperQuartilePayBand
                || newReturn.MaleUpperPayBand != existingReturn.MaleUpperPayBand
                || newReturn.FemaleUpperPayBand != existingReturn.FemaleUpperPayBand
                || newReturn.MaleMiddlePayBand != existingReturn.MaleMiddlePayBand
                || newReturn.FemaleMiddlePayBand != existingReturn.FemaleMiddlePayBand
                || newReturn.MaleLowerPayBand != existingReturn.MaleLowerPayBand
                || newReturn.FemaleLowerPayBand != existingReturn.FemaleLowerPayBand)
            {
                modifications.Add("Figures");
            }

            if (newReturn.FirstName != existingReturn.FirstName
                || newReturn.LastName != existingReturn.LastName
                || newReturn.JobTitle != existingReturn.JobTitle)
            {
                modifications.Add("PersonResponsible");
            }

            if (newReturn.MinEmployees != existingReturn.MinEmployees
                || newReturn.MaxEmployees != existingReturn.MaxEmployees)
            {
                modifications.Add("OrganisationSize");
            }

            if (newReturn.CompanyLinkToGPGInfo != existingReturn.CompanyLinkToGPGInfo)
            {
                modifications.Add("WebsiteURL");
            }

            return string.Join(", ", modifications);
        }

        private static void RetireOldReturnsForSameYear(Return newReturn, User user)
        {
            List<Return> oldActiveReturnsForSameYear =
                newReturn.Organisation.Returns
                    .Where(r => r.AccountingDate == newReturn.AccountingDate) // Get only the Returns for this year
                    .Where(r => r.Status == ReturnStatuses.Submitted) // Get only the Submitted(Active) Returns - normally only one
                    .Except(new[] { newReturn }) // Except this new Return
                    .ToList();

            foreach (Return oldReturn in oldActiveReturnsForSameYear)
            {
                oldReturn.SetStatus(ReturnStatuses.Retired, user.UserId);
            }
        }

        private void SendGeoFirstTimeSubmissionEmail(Return newReturn, IUrlHelper urlHelper)
        {
            if (Global.EnableSubmitAlerts
                && newReturn.Organisation.Returns.Count(r => r.AccountingDate == newReturn.AccountingDate) == 1)
            {
                string urlToPublicViewingPage = urlHelper.Action(
                    "ReportForYear",
                    "ViewReports",
                    new { organisationId = newReturn.OrganisationId, year = newReturn.AccountingDate.Year },
                    "https");

                emailSendingService.SendGeoFirstTimeDataSubmissionEmail(
                    newReturn.AccountingDate.Year.ToString(),
                    newReturn.Organisation.OrganisationName,
                    newReturn.StatusDate.ToShortDateString(),
                    urlToPublicViewingPage);
            }
        }

        private void SendSuccessfulSubmissionEmailToRegisteredUsers(Return newReturn, IUrlHelper urlHelper)
        {
            string urlToPublicViewingPage = urlHelper.Action(
                "ReportForYear",
                "ViewReports",
                new { organisationId = newReturn.OrganisationId, year = newReturn.AccountingDate.Year },
                "https");

            List<Return> otherReturns =
                newReturn.Organisation.Returns
                    .Except(new[] { newReturn })
                    .Where(r => r.AccountingDate == newReturn.AccountingDate)
                    .ToList();
            string submittedOrUpdated = otherReturns.Count > 0 ? "updated" : "submitted";

            EmailSendingServiceHelpers.SendSuccessfulSubmissionEmailToRegisteredUsers(
                newReturn,
                urlToPublicViewingPage,
                submittedOrUpdated,
                emailSendingService);
        }

    }
}
