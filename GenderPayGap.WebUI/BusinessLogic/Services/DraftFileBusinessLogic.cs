using System;
using System.Collections.Generic;
using System.Linq;
using GenderPayGap.Core;
using GenderPayGap.Core.Interfaces;
using GenderPayGap.Database.Models;
using GenderPayGap.Extensions;
using GenderPayGap.WebUI.BusinessLogic.Classes;
using GenderPayGap.WebUI.BusinessLogic.Models.Organisation;
using GenderPayGap.WebUI.BusinessLogic.Models.Submit;

namespace GenderPayGap.WebUI.BusinessLogic.Services
{
    public interface IDraftFileBusinessLogic
    {

        Draft GetDraftIfAvailable(long organisationId, int snapshotYear);
        Draft GetExistingOrNew(long organisationId, int snapshotYear, long userIdRequestingAccess);
        Draft UpdateAndCommit(ReturnViewModel postedReturnViewModel, Draft draft, long userIdRequestingAccess);
        void KeepDraftFileLockedToUser(Draft draftExpectedToBeLocked, long userIdRequestingLock);
        void DiscardDraft(Draft draftToDiscard);

    }

    public class DraftFileBusinessLogic : IDraftFileBusinessLogic
    {

        private readonly IDataRepository dataRepository;

        public DraftFileBusinessLogic(IDataRepository dataRepository)
        {
            this.dataRepository = dataRepository;
        }


        #region public methods

        public Draft GetDraftIfAvailable(long organisationId, int snapshotYear)
        {
            var result = new Draft(organisationId, snapshotYear);

            if (DraftExistsAndHasData(result))
            {
                result.ReturnViewModelContent = LoadDraftReturnAsReturnViewModel(result);
                return result;
            }

            return null;
        }

        /// <summary>
        ///     Gets a Draft object with information about the status of the draft.
        ///     It returns draft object locked to the current user, or flagged as locked by someone else via the variables
        ///     'IsUserAllowedAccess' and 'LastWrittenByUserId'.
        /// </summary>
        /// <param name="organisationId"></param>
        /// <param name="snapshotYear"></param>
        /// <param name="userIdRequestingAccess"></param>
        /// <returns></returns>
        public Draft GetExistingOrNew(long organisationId, int snapshotYear, long userIdRequestingAccess)
        {
            var resultingDraft = new Draft(organisationId, snapshotYear);
            return GetDraftOrCreate(resultingDraft, userIdRequestingAccess);
        }

        public Draft UpdateAndCommit(ReturnViewModel postedReturnViewModel, Draft draft, long userIdRequestingAccess)
        {
            Draft draftFile = GetDraftOrCreate(draft, userIdRequestingAccess);

            if (!draftFile.IsUserAllowedAccess)
            {
                return draftFile;
            }

            draft.HasDraftBeenModifiedDuringThisSession = default; // front end flag, reset before saving.

            draftFile.ReturnViewModelContent = postedReturnViewModel;
            WriteInDbAndTimestamp(draftFile);

            SetMetadata(draftFile, 0);

            return draftFile;
        }

        public void KeepDraftFileLockedToUser(Draft draftExpectedToBeLocked, long userIdRequestingLock)
        {
            (bool IsAllowedAccess, long UserId) result = GetIsUserAllowedAccess(userIdRequestingLock, draftExpectedToBeLocked);
            draftExpectedToBeLocked.IsUserAllowedAccess = result.IsAllowedAccess;

            if (!draftExpectedToBeLocked.IsUserAllowedAccess)
            {
                return;
            }

            SetMetadata(draftExpectedToBeLocked, userIdRequestingLock);

            SetDraftAccessInformation(userIdRequestingLock, draftExpectedToBeLocked);
        }

        public void DiscardDraft(Draft draftToDiscard)
        {
            List<DraftReturn> matchingReturns = dataRepository.GetAll<DraftReturn>()
                .Where(dr => dr.OrganisationId == draftToDiscard.OrganisationId)
                .Where(dr => dr.SnapshotYear == draftToDiscard.SnapshotYear)
                .ToList();

            foreach (DraftReturn matchingReturn in matchingReturns)
            {
                dataRepository.Delete(matchingReturn);
            }

            dataRepository.SaveChangesAsync().Wait();
        }

        #endregion

        #region private methods

        private Draft GetDraftOrCreate(Draft resultingDraft, long userIdRequestingAccess)
        {
            if (!DraftExistsAndHasData(resultingDraft))
            {
                SaveNewEmptyDraftReturn(resultingDraft, userIdRequestingAccess);
                SetDraftAccessInformation(userIdRequestingAccess, resultingDraft);
                return resultingDraft;
            }

            SetDraftAccessInformation(userIdRequestingAccess, resultingDraft);

            if (!resultingDraft.IsUserAllowedAccess)
            {
                return resultingDraft;
            }

            SetMetadata(resultingDraft, userIdRequestingAccess);
            resultingDraft.ReturnViewModelContent = LoadDraftReturnAsReturnViewModel(resultingDraft);

            return resultingDraft;
        }

        private void SaveNewEmptyDraftReturn(Draft resultingDraft, long userIdRequestingAccess)
        {
            var newEmptyDraftReturn = new DraftReturn
            {
                OrganisationId = resultingDraft.OrganisationId,
                SnapshotYear = resultingDraft.SnapshotYear,
                LastWrittenByUserId = userIdRequestingAccess
            };

            InsertOrUpdate(newEmptyDraftReturn);
            dataRepository.SaveChangesAsync().Wait();
        }

        private bool DraftExists(Draft draft)
        {
            DraftReturn draftReturn = GetDraftReturnFromDatabase(draft.OrganisationId, draft.SnapshotYear);

            return draftReturn != null;
        }

        private bool DraftExistsAndHasData(Draft draft)
        {
            DraftReturn draftReturn = GetDraftReturnFromDatabase(draft.OrganisationId, draft.SnapshotYear);

            return draftReturn != null && draftReturn.SectorType.HasValue;
        }

        private DateTime? GetLastWriteTime(Draft draft)
        {
            if (DraftExists(draft))
            {
                DraftReturn originalDraftReturn = GetDraftReturnFromDatabase(draft.OrganisationId, draft.SnapshotYear);
                return originalDraftReturn.LastWrittenDateTime;
            }

            return null;
        }

        private void SetMetadata(Draft draft, long userIdRequestingAccess)
        {
            DraftReturn originalDraftReturn = GetDraftReturnFromDatabase(draft.OrganisationId, draft.SnapshotYear);

            if (originalDraftReturn != null)
            {
                originalDraftReturn.LastWrittenByUserId = userIdRequestingAccess;
                originalDraftReturn.LastWrittenDateTime = VirtualDateTime.Now;

                dataRepository.SaveChangesAsync().Wait();
            }
        }

        private void SetDraftAccessInformation(long userRequestingAccessToDraft, Draft draft)
        {
            draft.LastWrittenDateTime = GetLastWriteTime(draft);

            (bool IsAllowedAccess, long UserId) result = GetIsUserAllowedAccess(userRequestingAccessToDraft, draft);
            draft.IsUserAllowedAccess = result.IsAllowedAccess;
            draft.LastWrittenByUserId = result.UserId;
        }

        private (bool IsAllowedAccess, long UserId) GetIsUserAllowedAccess(long userRequestingAccessToDraft, Draft draft)
        {
            if (!DraftExists(draft))
            {
                return (true, 0);
            }

            (bool IsAllowedAccess, long UserId) result =
                GetIsUserTheLastPersonThatWroteOnTheFile(draft.OrganisationId, draft.SnapshotYear, userRequestingAccessToDraft);

            return (result.IsAllowedAccess || !IsInUse(draft.LastWrittenDateTime), result.UserId);
        }

        private (bool IsAllowedAccess, long UserId) GetIsUserTheLastPersonThatWroteOnTheFile(long organisationId,
            int snapshotYear,
            long userId)
        {
            DraftReturn draftReturn = GetDraftReturnFromDatabase(organisationId, snapshotYear);

            long lastAccessedByUserId = draftReturn.LastWrittenByUserId ?? 0;
            return (lastAccessedByUserId == 0 || lastAccessedByUserId == userId, lastAccessedByUserId);
        }

        private bool IsInUse(DateTime? lastWrittenDateTime)
        {
            return lastWrittenDateTime != null // System was able to read the file's time-stamp
                   && VirtualDateTime.Now.AddMinutes(-20) <= lastWrittenDateTime;
        }

        private ReturnViewModel LoadDraftReturnAsReturnViewModel(Draft resultingDraft)
        {
            DraftReturn draftReturnFromDatabase = GetDraftReturnFromDatabase(resultingDraft.OrganisationId, resultingDraft.SnapshotYear);
            ReturnViewModel returnViewModel = CastDatabaseDraftReturnToReturnViewModel(draftReturnFromDatabase);
            return returnViewModel;
        }

        private DraftReturn GetDraftReturnFromDatabase(long organisationId, int snapshotYear)
        {
            return dataRepository.GetAll<DraftReturn>()
                .Where(
                    d => d.OrganisationId == organisationId
                         && d.SnapshotYear == snapshotYear)
                .OrderByDescending(dr => dr.DraftReturnId)
                .FirstOrDefault();
        }

        private static ReturnViewModel CastDatabaseDraftReturnToReturnViewModel(DraftReturn draftReturn)
        {
            if (draftReturn == null)
            {
                return null;
            }
            return new ReturnViewModel
            {
                AccountingDate = draftReturn.AccountingDate ?? VirtualDateTime.Now,
                Address = draftReturn.Address,
                CompanyLinkToGPGInfo = draftReturn.CompanyLinkToGPGInfo,
                DiffMeanBonusPercent = NormaliseDecimal(draftReturn.DiffMeanBonusPercent),
                DiffMeanHourlyPayPercent = NormaliseDecimal(draftReturn.DiffMeanHourlyPayPercent),
                DiffMedianBonusPercent = NormaliseDecimal(draftReturn.DiffMedianBonusPercent),
                DiffMedianHourlyPercent = NormaliseDecimal(draftReturn.DiffMedianHourlyPercent),
                EHRCResponse = draftReturn.EHRCResponse,
                EncryptedOrganisationId = draftReturn.EncryptedOrganisationId,
                FemaleLowerPayBand = NormaliseDecimal(draftReturn.FemaleLowerPayBand),
                FemaleMedianBonusPayPercent = NormaliseDecimal(draftReturn.FemaleMedianBonusPayPercent),
                FemaleMiddlePayBand = NormaliseDecimal(draftReturn.FemaleMiddlePayBand),
                FemaleUpperPayBand = NormaliseDecimal(draftReturn.FemaleUpperPayBand),
                FemaleUpperQuartilePayBand = NormaliseDecimal(draftReturn.FemaleUpperQuartilePayBand),
                FirstName = draftReturn.FirstName,
                IsDifferentFromDatabase = draftReturn.IsDifferentFromDatabase ?? false,
                IsInScopeForThisReportYear = draftReturn.IsInScopeForThisReportYear ?? false,
                IsLateSubmission = draftReturn.IsLateSubmission ?? false,
                IsVoluntarySubmission = draftReturn.IsVoluntarySubmission ?? false,
                JobTitle = draftReturn.JobTitle,
                LastName = draftReturn.LastName,
                LateReason = draftReturn.LateReason,
                LatestAddress = draftReturn.LatestAddress,
                LatestOrganisationName = draftReturn.LatestOrganisationName,
                LatestSector = draftReturn.LatestSector,
                MaleMedianBonusPayPercent = NormaliseDecimal(draftReturn.MaleMedianBonusPayPercent),
                MaleMiddlePayBand = NormaliseDecimal(draftReturn.MaleMiddlePayBand),
                MaleUpperQuartilePayBand = NormaliseDecimal(draftReturn.MaleUpperQuartilePayBand),
                MaleUpperPayBand = NormaliseDecimal(draftReturn.MaleUpperPayBand),
                MaleLowerPayBand = NormaliseDecimal(draftReturn.MaleLowerPayBand),
                Modified = draftReturn.Modified,
                OrganisationId = draftReturn.OrganisationId,
                OrganisationName = draftReturn.OrganisationName,
                OrganisationSize = draftReturn.OrganisationSize ?? OrganisationSizes.NotProvided,
                OriginatingAction = draftReturn.OriginatingAction,
                ReportInfo = new ReportInfoModel
                {
                    ReportingRequirement = draftReturn.ReportingRequirement ?? ScopeStatuses.Unknown,
                    ReportingStartDate = draftReturn.ReportingStartDate ?? VirtualDateTime.Now,
                    ReportModifiedDate = draftReturn.ReportModifiedDate ?? VirtualDateTime.Now,
                },
                ReturnId = draftReturn.ReturnId ?? 0,
                ReturnUrl = draftReturn.ReturnUrl,
                Sector = draftReturn.Sector,
                SectorType = draftReturn.SectorType ?? SectorTypes.Unknown,
                ShouldProvideLateReason = draftReturn.ShouldProvideLateReason ?? false
            };
        }

        private static decimal? NormaliseDecimal(decimal? input)
        {
            // In the drafts, some numbers are stored as decimals with exactly 2 decimal places (e.g. 5.00)
            // The controller does some validation to check that the numbers entered only have 1 decimal place (e.g. 5.0)
            // This calculation removes any trailing 0s, so 5.00 becomes 5.0, and 12.30 becomes 12.3
            return input / 1.000000000m;
        }

        private DraftReturn SerialiseDraftAsDraftReturn(Draft draft)
        {
            var draftReturn = new DraftReturn
            {
                OrganisationId = draft.OrganisationId,
                SnapshotYear = draft.SnapshotYear,

                AccountingDate = draft.ReturnViewModelContent?.AccountingDate,
                Address = draft.ReturnViewModelContent?.Address,
                CompanyLinkToGPGInfo = draft.ReturnViewModelContent?.CompanyLinkToGPGInfo,
                DiffMeanBonusPercent = draft.ReturnViewModelContent?.DiffMeanBonusPercent,
                DiffMeanHourlyPayPercent = draft.ReturnViewModelContent?.DiffMeanHourlyPayPercent,
                DiffMedianBonusPercent = draft.ReturnViewModelContent?.DiffMedianBonusPercent,
                DiffMedianHourlyPercent = draft.ReturnViewModelContent?.DiffMedianHourlyPercent,
                EHRCResponse = draft.ReturnViewModelContent?.EHRCResponse,
                EncryptedOrganisationId = draft.ReturnViewModelContent?.EncryptedOrganisationId,
                FemaleLowerPayBand = draft.ReturnViewModelContent?.FemaleLowerPayBand,
                FemaleMedianBonusPayPercent = draft.ReturnViewModelContent?.FemaleMedianBonusPayPercent,
                FemaleMiddlePayBand = draft.ReturnViewModelContent?.FemaleMiddlePayBand,
                FemaleUpperPayBand = draft.ReturnViewModelContent?.FemaleUpperPayBand,
                FemaleUpperQuartilePayBand = draft.ReturnViewModelContent?.FemaleUpperQuartilePayBand,
                FirstName = draft.ReturnViewModelContent?.FirstName,
                HasDraftBeenModifiedDuringThisSession = draft.HasDraftBeenModifiedDuringThisSession,
                IsDifferentFromDatabase = draft.ReturnViewModelContent?.IsDifferentFromDatabase,
                IsInScopeForThisReportYear = draft.ReturnViewModelContent?.IsInScopeForThisReportYear,
                IsLateSubmission = draft.ReturnViewModelContent?.IsLateSubmission,
                IsVoluntarySubmission = draft.ReturnViewModelContent?.IsVoluntarySubmission,
                JobTitle = draft.ReturnViewModelContent?.JobTitle,
                LastName = draft.ReturnViewModelContent?.LastName,
                LastWrittenByUserId = draft.LastWrittenByUserId,
                LastWrittenDateTime = draft.LastWrittenDateTime,
                LateReason = draft.ReturnViewModelContent?.LateReason,
                LatestAddress = draft.ReturnViewModelContent?.LatestAddress,
                LatestOrganisationName = draft.ReturnViewModelContent?.LatestOrganisationName,
                LatestSector = draft.ReturnViewModelContent?.LatestSector,
                MaleLowerPayBand = draft.ReturnViewModelContent?.MaleLowerPayBand,
                MaleMedianBonusPayPercent = draft.ReturnViewModelContent?.MaleMedianBonusPayPercent,
                MaleMiddlePayBand = draft.ReturnViewModelContent?.MaleMiddlePayBand,
                MaleUpperPayBand = draft.ReturnViewModelContent?.MaleUpperPayBand,
                MaleUpperQuartilePayBand = draft.ReturnViewModelContent?.MaleUpperQuartilePayBand,
                Modified = draft.ReturnViewModelContent?.Modified ?? VirtualDateTime.Now,
                OrganisationName = draft.ReturnViewModelContent?.OrganisationName,
                OrganisationSize = draft.ReturnViewModelContent?.OrganisationSize,
                OriginatingAction = draft.ReturnViewModelContent?.OriginatingAction,
                ReportingRequirement = draft.ReturnViewModelContent?.ReportInfo.ReportingRequirement,
                ReportingStartDate = draft.ReturnViewModelContent?.ReportInfo.ReportingStartDate,
                ReportModifiedDate = draft.ReturnViewModelContent?.ReportInfo.ReportModifiedDate,
                ReturnId = draft.ReturnViewModelContent?.ReturnId,
                ReturnUrl = draft.ReturnViewModelContent?.ReturnUrl,
                Sector = draft.ReturnViewModelContent?.Sector,
                SectorType = draft.ReturnViewModelContent?.SectorType,
                ShouldProvideLateReason = draft.ReturnViewModelContent?.ShouldProvideLateReason
            };
            
            return draftReturn;
        }

        private void WriteInDbAndTimestamp(Draft draftFile)
        {
            DraftReturn draftReturn = SerialiseDraftAsDraftReturn(draftFile);
            InsertOrUpdate(draftReturn);
        }

        private void InsertOrUpdate(DraftReturn draftToSave)
        {
            List<DraftReturn> matchingReturns = dataRepository.GetAll<DraftReturn>()
                .Where(dr => dr.OrganisationId == draftToSave.OrganisationId)
                .Where(dr => dr.SnapshotYear == draftToSave.SnapshotYear)
                .ToList();

            foreach (DraftReturn matchingReturn in matchingReturns)
            {
                dataRepository.Delete(matchingReturn);
            }

            dataRepository.Insert(draftToSave);
            dataRepository.SaveChangesAsync().Wait();
        }

        #endregion

    }
}
