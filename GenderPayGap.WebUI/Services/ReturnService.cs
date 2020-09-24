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


        public Return CreateAndSaveOnTimeReturnFromDraftReturn(DraftReturn draftReturn, User user, IUrlHelper urlHelper)
        {
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

                MaleUpperQuartilePayBand = draftReturn.MaleUpperQuartilePayBand.Value,
                FemaleUpperQuartilePayBand = draftReturn.FemaleUpperQuartilePayBand.Value,
                MaleUpperPayBand = draftReturn.MaleUpperPayBand.Value,
                FemaleUpperPayBand = draftReturn.FemaleUpperPayBand.Value,
                MaleMiddlePayBand = draftReturn.MaleMiddlePayBand.Value,
                FemaleMiddlePayBand = draftReturn.FemaleMiddlePayBand.Value,
                MaleLowerPayBand = draftReturn.MaleLowerPayBand.Value,
                FemaleLowerPayBand = draftReturn.FemaleLowerPayBand.Value,

                FirstName = draftReturn.FirstName,
                LastName = draftReturn.LastName,
                JobTitle = draftReturn.JobTitle,

                MinEmployees = (int) organisationSizeRange.Minimum,
                MaxEmployees = (int) organisationSizeRange.Maximum,

                CompanyLinkToGPGInfo = draftReturn.CompanyLinkToGPGInfo,

                IsLateSubmission = isLateSubmission,
                LateReason = lateReason,
                EHRCResponse = receivedLetterFromEhrc,

                Modifications = null, // TODO we'll need to set this for edited returns

                // We will set the status to Submitted soon
                // But, we want to use the Return.SetStatus method (which required the Return to already be saved
                // But, to save the Return, we need an initial status
                // Maybe we should see if we can reduce this complexity!
                Status = ReturnStatuses.Draft
            };

            return newReturn;
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
                    "Report",
                    "Viewing",
                    new { employerIdentifier = newReturn.Organisation.GetEncryptedId(), year = newReturn.AccountingDate.Year },
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
                "Report",
                "Viewing",
                new { employerIdentifier = newReturn.Organisation.GetEncryptedId(), year = newReturn.AccountingDate.Year },
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
