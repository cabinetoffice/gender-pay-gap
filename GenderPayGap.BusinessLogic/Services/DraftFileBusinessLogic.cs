using System;
using System.Collections.Generic;
using System.Linq;
using GenderPayGap.BusinessLogic.Classes;
using GenderPayGap.BusinessLogic.Models.Submit;
using GenderPayGap.Core;
using GenderPayGap.Core.Interfaces;
using GenderPayGap.Database.Models;
using GenderPayGap.Extensions;

namespace GenderPayGap.BusinessLogic.Services
{
    public interface IDraftFileBusinessLogic
    {

        Draft GetDraftIfAvailable(long organisationId, int snapshotYear);
        Draft GetExistingOrNew(long organisationId, int snapshotYear, long userIdRequestingAccess);
        Draft Update(ReturnViewModel postedReturnViewModel, Draft draft, long userIdRequestingAccess);
        void KeepDraftFileLockedToUser(Draft draftExpectedToBeLocked, long userIdRequestingLock);
        void DiscardDraft(Draft draftToDiscard);
        bool RollbackDraft(Draft draftToRevert);
        void RestartDraft(long organisationId, int snapshotYear, long userIdRequestingRollback);
        void CommitDraft(Draft draftToKeep);

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

            if (DraftExists(result, DraftReturnStatus.Original))
            {
                if (DraftExists(result, DraftReturnStatus.Backup))
                {
                    result.ReturnViewModelContent = LoadDraftReturnAsReturnViewModel(result, DraftReturnStatus.Backup);
                    return result;
                }

                result.ReturnViewModelContent = LoadDraftReturnAsReturnViewModel(result, DraftReturnStatus.Original);
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
            return GetDraftOrCreateAsync(resultingDraft, userIdRequestingAccess);
        }

        public Draft Update(ReturnViewModel postedReturnViewModel, Draft draft, long userIdRequestingAccess)
        {
            Draft draftFile = GetDraftOrCreateAsync(draft, userIdRequestingAccess);

            if (!draftFile.IsUserAllowedAccess)
            {
                return draftFile;
            }

            draft.HasDraftBeenModifiedDuringThisSession = default; // front end flag, reset before saving.

            draftFile.ReturnViewModelContent = postedReturnViewModel;
            WriteInDbAndTimestamp(draftFile, userIdRequestingAccess);

            return draftFile;
        }

        public void KeepDraftFileLockedToUser(Draft draftExpectedToBeLocked, long userIdRequestingLock)
        {
            (bool IsAllowedAccess, long UserId) result = GetIsUserAllowedAccessAsync(userIdRequestingLock, draftExpectedToBeLocked);
            draftExpectedToBeLocked.IsUserAllowedAccess = result.IsAllowedAccess;

            if (!draftExpectedToBeLocked.IsUserAllowedAccess)
            {
                return;
            }

            SetMetadataAsync(draftExpectedToBeLocked, userIdRequestingLock);

            SetDraftAccessInformationAsync(userIdRequestingLock, draftExpectedToBeLocked);
        }

        public void DiscardDraft(Draft draftToDiscard)
        {
            DeleteDraft(draftToDiscard, DraftReturnStatus.Original);
            DeleteDraft(draftToDiscard, DraftReturnStatus.Backup);
        }

        public void RestartDraft(long organisationId, int snapshotYear, long userIdRequestingRollback)
        {
            var draftToRollback = new Draft(organisationId, snapshotYear);
            SetDraftAccessInformationAsync(userIdRequestingRollback, draftToRollback);
            if (!draftToRollback.IsUserAllowedAccess)
            {
                return;
            }

            bool hasRollbackSucceeded = RollbackDraft(draftToRollback);

            if (hasRollbackSucceeded)
            {
                CopyDraft(draftToRollback, DraftReturnStatus.Original, DraftReturnStatus.Backup);
            }
        }

        public bool RollbackDraft(Draft draftToDiscard)
        {
            if (!DraftExists(draftToDiscard, DraftReturnStatus.Backup))
            {
                return false;
            }

            DeleteDraft(draftToDiscard, DraftReturnStatus.Original);
            CopyDraft(draftToDiscard, DraftReturnStatus.Backup, DraftReturnStatus.Original);
            
            CommitDraft(draftToDiscard);

            return true;
        }

        public void CommitDraft(Draft draftToDiscard)
        {
            SetMetadataAsync(draftToDiscard, 0);
            DeleteDraft(draftToDiscard, DraftReturnStatus.Backup);
        }

        #endregion

        #region private methods

        private Draft GetDraftOrCreateAsync(Draft resultingDraft, long userIdRequestingAccess)
        {
            if (!DraftExists(resultingDraft, DraftReturnStatus.Original))
            {
                SaveNewEmptyDraftReturn(resultingDraft, DraftReturnStatus.Original, userIdRequestingAccess);
                CopyDraft(resultingDraft, DraftReturnStatus.Original, DraftReturnStatus.Backup);
                SetDraftAccessInformationAsync(userIdRequestingAccess, resultingDraft);
                return resultingDraft;
            }

            SetDraftAccessInformationAsync(userIdRequestingAccess, resultingDraft);

            if (!resultingDraft.IsUserAllowedAccess)
            {
                return resultingDraft;
            }

            if (!DraftExists(resultingDraft, DraftReturnStatus.Backup))
            {
                CopyDraft(resultingDraft, DraftReturnStatus.Original, DraftReturnStatus.Backup);
            }

            SetMetadataAsync(resultingDraft, userIdRequestingAccess);
            resultingDraft.ReturnViewModelContent = LoadDraftReturnAsReturnViewModel(resultingDraft, DraftReturnStatus.Original);

            return resultingDraft;
        }

        private void DeleteDraft(Draft draft, DraftReturnStatus draftType)
        {
            dataRepository.Delete(GetDraftReturnFromDatabase(draft.OrganisationId, draft.SnapshotYear, draftType));
            dataRepository.SaveChangesAsync().Wait();
        }

        private void SaveNewEmptyDraftReturn(Draft resultingDraft, DraftReturnStatus draftType, long userIdRequestingAccess)
        {
            var newEmptyDraftReturn = new DraftReturn
            {
                DraftReturnStatus = draftType,
                OrganisationId = resultingDraft.OrganisationId,
                SnapshotYear = resultingDraft.SnapshotYear,
                LastWrittenByUserId = userIdRequestingAccess
            };

            dataRepository.Insert(newEmptyDraftReturn);
            dataRepository.SaveChangesAsync().Wait();
        }

        private void CopyDraft(Draft draft, DraftReturnStatus fromType, DraftReturnStatus toType)
        {
            DraftReturn draftToCopy = GetDraftReturnFromDatabase(draft.OrganisationId, draft.SnapshotYear, fromType);

            var newDraftReturn = new DraftReturn(draftToCopy, toType);

            InsertOrUpdate(newDraftReturn);
        }

        private bool DraftExists(Draft draft, DraftReturnStatus draftType)
        {
            DraftReturn draftReturn = GetDraftReturnFromDatabase(draft.OrganisationId, draft.SnapshotYear, draftType);

            return draftReturn != null;
        }

        private DateTime? GetLastWriteTime(Draft draft, DraftReturnStatus draftType)
        {
            if (DraftExists(draft, draftType))
            {
                DraftReturn originalDraftReturn = GetDraftReturnFromDatabase(draft.OrganisationId, draft.SnapshotYear, draftType);
                return originalDraftReturn.LastWrittenDateTime;
            }

            return null;
        }

        private void SetMetadataAsync(Draft draft, long userIdRequestingAccess)
        {
            DraftReturn originalDraftReturn = GetDraftReturnFromDatabase(draft.OrganisationId, draft.SnapshotYear, DraftReturnStatus.Original);

            originalDraftReturn.LastWrittenByUserId = userIdRequestingAccess;
            originalDraftReturn.LastWrittenDateTime = VirtualDateTime.Now;

            dataRepository.SaveChangesAsync().Wait();
        }

        private void SetDraftAccessInformationAsync(long userRequestingAccessToDraft, Draft draft)
        {
            draft.LastWrittenDateTime = GetLastWriteTime(draft, DraftReturnStatus.Original);

            (bool IsAllowedAccess, long UserId) result = GetIsUserAllowedAccessAsync(userRequestingAccessToDraft, draft);
            draft.IsUserAllowedAccess = result.IsAllowedAccess;
            draft.LastWrittenByUserId = result.UserId;
        }

        private (bool IsAllowedAccess, long UserId) GetIsUserAllowedAccessAsync(long userRequestingAccessToDraft, Draft draft)
        {
            if (!DraftExists(draft, DraftReturnStatus.Original))
            {
                return (true, 0);
            }

            (bool IsAllowedAccess, long UserId) result =
                GetIsUserTheLastPersonThatWroteOnTheFileAsync(draft.OrganisationId, draft.SnapshotYear, DraftReturnStatus.Original, userRequestingAccessToDraft);

            return (result.IsAllowedAccess || !IsInUse(draft.LastWrittenDateTime), result.UserId);
        }

        private (bool IsAllowedAccess, long UserId) GetIsUserTheLastPersonThatWroteOnTheFileAsync(long organisationId,
            int snapshotYear,
            DraftReturnStatus draftReturnStatus,
            long userId)
        {
            DraftReturn draftReturn = GetDraftReturnFromDatabase(organisationId, snapshotYear, draftReturnStatus);

            long lastAccessedByUserId = draftReturn.LastWrittenByUserId ?? 0;
            return (lastAccessedByUserId == 0 || lastAccessedByUserId == userId, lastAccessedByUserId);
        }

        private bool IsInUse(DateTime? lastWrittenDateTime)
        {
            return lastWrittenDateTime != null // System was able to read the file's time-stamp
                   && VirtualDateTime.Now.AddMinutes(-20) <= lastWrittenDateTime;
        }

        private ReturnViewModel LoadDraftReturnAsReturnViewModel(Draft resultingDraft, DraftReturnStatus draftType)
        {
            DraftReturn draftReturnFromDatabase = GetDraftReturnFromDatabase(resultingDraft.OrganisationId, resultingDraft.SnapshotYear, draftType);
            ReturnViewModel returnViewModel = CastDatabaseDraftReturnToReturnViewModel(draftReturnFromDatabase);
            return returnViewModel;
        }

        private DraftReturn GetDraftReturnFromDatabase(long organisationId, int snapshotYear, DraftReturnStatus draftReturnStatus)
        {
            return dataRepository.GetAll<DraftReturn>()
                .Where(
                    d => d.OrganisationId == organisationId
                         && d.SnapshotYear == snapshotYear
                         && d.DraftReturnStatus == draftReturnStatus)
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
                DiffMeanBonusPercent = draftReturn.DiffMeanBonusPercent,
                DiffMeanHourlyPayPercent = draftReturn.DiffMeanHourlyPayPercent,
                DiffMedianBonusPercent = draftReturn.DiffMedianBonusPercent,
                DiffMedianHourlyPercent = draftReturn.DiffMedianHourlyPercent,
                EHRCResponse = draftReturn.EHRCResponse,
                EncryptedOrganisationId = draftReturn.EncryptedOrganisationId,
                FemaleLowerPayBand = draftReturn.FemaleLowerPayBand,
                FemaleMedianBonusPayPercent = draftReturn.FemaleMedianBonusPayPercent,
                FemaleMiddlePayBand = draftReturn.FemaleMiddlePayBand,
                FemaleUpperPayBand = draftReturn.FemaleUpperPayBand,
                FemaleUpperQuartilePayBand = draftReturn.FemaleUpperQuartilePayBand,
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
                MaleMedianBonusPayPercent = draftReturn.MaleMedianBonusPayPercent,
                MaleMiddlePayBand = draftReturn.MaleMiddlePayBand,
                MaleUpperQuartilePayBand = draftReturn.MaleUpperQuartilePayBand,
                MaleUpperPayBand = draftReturn.MaleUpperPayBand,
                MaleLowerPayBand = draftReturn.MaleLowerPayBand,
                Modified = draftReturn.Modified,
                OrganisationId = draftReturn.OrganisationId,
                OrganisationName = draftReturn.OrganisationName,
                OrganisationSize = draftReturn.OrganisationSize ?? OrganisationSizes.NotProvided,
                OriginatingAction = draftReturn.OriginatingAction,
                ReportInfo = new Models.Organisation.ReportInfoModel
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

        private DraftReturn SerialiseDraftAsDraftReturn(Draft draft, DraftReturnStatus draftType)
        {
            var draftReturn = new DraftReturn
            {
                OrganisationId = draft.OrganisationId,
                SnapshotYear = draft.SnapshotYear,

                DraftReturnStatus = draftType,

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

        private void WriteInDbAndTimestamp(Draft draftFile, long userIdRequestingAccess)
        {
            DraftReturn draftReturn = SerialiseDraftAsDraftReturn(draftFile, DraftReturnStatus.Original);
            InsertOrUpdate(draftReturn);
            SetMetadataAsync(draftFile, userIdRequestingAccess);
        }

        private void InsertOrUpdate(DraftReturn draftToSave)
        {
            List<DraftReturn> matchingReturns = dataRepository.GetAll<DraftReturn>()
                .Where(dr => dr.OrganisationId == draftToSave.OrganisationId)
                .Where(dr => dr.SnapshotYear == draftToSave.SnapshotYear)
                .Where(dr => dr.DraftReturnStatus == draftToSave.DraftReturnStatus)
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
