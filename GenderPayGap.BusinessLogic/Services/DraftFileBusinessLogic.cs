using System;
using System.Threading.Tasks;
using GenderPayGap.BusinessLogic.Classes;
using GenderPayGap.BusinessLogic.Models.Submit;
using GenderPayGap.Core;
using GenderPayGap.Core.Interfaces;
using GenderPayGap.Database.Models;
using GenderPayGap.Extensions;
using Microsoft.EntityFrameworkCore;

namespace GenderPayGap.BusinessLogic.Services
{
    public interface IDraftFileBusinessLogic
    {

        Task<Draft> GetDraftIfAvailableAsync(long organisationId, int snapshotYear);
        Task<Draft> GetExistingOrNewAsync(long organisationId, int snapshotYear, long userIdRequestingAccess);
        Task<Draft> UpdateAsync(ReturnViewModel postedReturnViewModel, Draft draft, long userIdRequestingAccess);
        Task KeepDraftFileLockedToUserAsync(Draft draftExpectedToBeLocked, long userIdRequestingLock);
        Task DiscardDraftAsync(Draft draftToDiscard);
        Task<bool> RollbackDraftAsync(Draft draftToRevert);
        Task RestartDraftAsync(long organisationId, int snapshotYear, long userIdRequestingRollback);
        Task CommitDraftAsync(Draft draftToKeep);

    }

    public class DraftFileBusinessLogic : IDraftFileBusinessLogic
    {

        #region Constructor

        public DraftFileBusinessLogic(IDataRepository dataRepository)
        {
            this.dataRepository = dataRepository;
        }

        #endregion

        #region attributes

        private readonly IDataRepository dataRepository;

        #endregion

        #region public methods

        public async Task<Draft> GetDraftIfAvailableAsync(long organisationId, int snapshotYear)
        {
            var result = new Draft(organisationId, snapshotYear);

            Draft originalDraftFromDb = CastDatabaseDraftReturnToDraft(await GetDraftReturnFromDatabase(organisationId, snapshotYear, DraftReturnStatus.Original));
            Draft backupDraftFromDb = CastDatabaseDraftReturnToDraft(await GetDraftReturnFromDatabase(organisationId, snapshotYear, DraftReturnStatus.Backup));

            if (originalDraftFromDb == null)
            {
                return null;
            }
            
            if (originalDraftFromDb.ReturnViewModelContent != null && backupDraftFromDb == null)
            {
                result.ReturnViewModelContent = originalDraftFromDb.ReturnViewModelContent;
                return result;
            }

            if (originalDraftFromDb.ReturnViewModelContent == null && backupDraftFromDb == null)
            {
                return null;
            }
            
            if (originalDraftFromDb.ReturnViewModelContent != null && backupDraftFromDb.ReturnViewModelContent != null)
            {
                result.ReturnViewModelContent = backupDraftFromDb.ReturnViewModelContent;
                return result;
            }

            if (originalDraftFromDb.ReturnViewModelContent != null && backupDraftFromDb.ReturnViewModelContent == null)
            {
                return null;
            }

            if (originalDraftFromDb.ReturnViewModelContent == null && backupDraftFromDb.ReturnViewModelContent == null)
            {
                return null;
            }

            throw new NotImplementedException();
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
        public async Task<Draft> GetExistingOrNewAsync(long organisationId, int snapshotYear, long userIdRequestingAccess)
        {
            var resultingDraft = new Draft(organisationId, snapshotYear);
            return await GetDraftOrCreateAsync(resultingDraft, userIdRequestingAccess);
        }

        public async Task<Draft> UpdateAsync(ReturnViewModel postedReturnViewModel, Draft draft, long userIdRequestingAccess)
        {
            Draft draftFile = await GetDraftOrCreateAsync(draft, userIdRequestingAccess);

            if (!draftFile.IsUserAllowedAccess)
            {
                return draftFile;
            }

            draftFile.HasDraftBeenModifiedDuringThisSession = default; // front end flag, reset before saving.

            draftFile.ReturnViewModelContent = postedReturnViewModel;
            await WriteInDbAndTimestampAsync(draftFile, userIdRequestingAccess);

            return draftFile;
        }

        public async Task KeepDraftFileLockedToUserAsync(Draft draftExpectedToBeLocked, long userIdRequestingLock)
        {
            (bool IsAllowedAccess, long UserId) result = await GetIsUserAllowedAccessAsync(userIdRequestingLock, draftExpectedToBeLocked);
            draftExpectedToBeLocked.IsUserAllowedAccess = result.IsAllowedAccess;

            if (!draftExpectedToBeLocked.IsUserAllowedAccess)
            {
                return;
            }

            await SetMetadataAsync(draftExpectedToBeLocked, userIdRequestingLock);

            await SetDraftAccessInformationAsync(userIdRequestingLock, draftExpectedToBeLocked);
        }

        public async Task DiscardDraftAsync(Draft draftToDiscard)
        {
            dataRepository.Delete(GetDraftReturnFromDatabase(draftToDiscard.OrganisationId, draftToDiscard.SnapshotYear, DraftReturnStatus.Original));
            dataRepository.Delete(GetDraftReturnFromDatabase(draftToDiscard.OrganisationId, draftToDiscard.SnapshotYear, DraftReturnStatus.Backup));
            await dataRepository.SaveChangesAsync();
        }

        public async Task RestartDraftAsync(long organisationId, int snapshotYear, long userIdRequestingRollback)
        {
            var draftToRollback = new Draft(organisationId, snapshotYear);
            await SetDraftAccessInformationAsync(userIdRequestingRollback, draftToRollback);
            if (!draftToRollback.IsUserAllowedAccess)
            {
                return;
            }

            bool hasRollbackSucceeded = await RollbackDraftAsync(draftToRollback);

            if (hasRollbackSucceeded)
            {
                DraftReturn newBackupDraftReturn = await GetDraftReturnFromDatabase(organisationId, snapshotYear, DraftReturnStatus.Original);
                newBackupDraftReturn.DraftReturnId = 0;
                newBackupDraftReturn.DraftReturnStatus = DraftReturnStatus.Backup;
                dataRepository.Insert(newBackupDraftReturn);
                await dataRepository.SaveChangesAsync();
            }
        }

        public async Task<bool> RollbackDraftAsync(Draft draftToDiscard)
        {
            DraftReturn originalDraftReturn = await GetDraftReturnFromDatabase(draftToDiscard.OrganisationId, draftToDiscard.SnapshotYear, DraftReturnStatus.Original);
            DraftReturn backupDraftReturn = await GetDraftReturnFromDatabase(draftToDiscard.OrganisationId, draftToDiscard.SnapshotYear, DraftReturnStatus.Backup);
            if (backupDraftReturn == null)
            {
                return false;
            }

            dataRepository.Delete(originalDraftReturn);
            
            DraftReturn newOriginalDraftReturn = new DraftReturn(backupDraftReturn);
            newOriginalDraftReturn.DraftReturnId = 0;
            newOriginalDraftReturn.DraftReturnStatus = DraftReturnStatus.Original;
            dataRepository.Insert(newOriginalDraftReturn);
            await dataRepository.SaveChangesAsync();

            await CommitDraftAsync(draftToDiscard);

            return true;
        }
        public async Task CommitDraftAsync(Draft draftToDiscard)
        {
            await SetMetadataAsync(draftToDiscard, 0);

            DraftReturn backupDraftReturn = await GetDraftReturnFromDatabase(draftToDiscard.OrganisationId, draftToDiscard.SnapshotYear, DraftReturnStatus.Backup);
            dataRepository.Delete(backupDraftReturn);
            await dataRepository.SaveChangesAsync();
        }

        #endregion

        #region private methods

        private async Task<Draft> GetDraftOrCreateAsync(Draft resultingDraft, long userIdRequestingAccess)
        {
            DraftReturn originalDraftReturn = await GetDraftReturnFromDatabase(resultingDraft.OrganisationId, resultingDraft.SnapshotYear, DraftReturnStatus.Original);
            DraftReturn backupDraftReturn = await GetDraftReturnFromDatabase(resultingDraft.OrganisationId, resultingDraft.SnapshotYear, DraftReturnStatus.Backup);

            if (originalDraftReturn == null)
            {
                dataRepository.Insert(new DraftReturn { DraftReturnStatus = DraftReturnStatus.Original, OrganisationId = resultingDraft.OrganisationId , SnapshotYear = resultingDraft.SnapshotYear, LastWrittenByUserId = userIdRequestingAccess });
                dataRepository.Insert(new DraftReturn { DraftReturnStatus = DraftReturnStatus.Backup, OrganisationId = resultingDraft.OrganisationId, SnapshotYear = resultingDraft.SnapshotYear, LastWrittenByUserId = userIdRequestingAccess });
                await SetDraftAccessInformationAsync(userIdRequestingAccess, resultingDraft);
                return resultingDraft;
            }

            await SetDraftAccessInformationAsync(userIdRequestingAccess, resultingDraft);

            if (!resultingDraft.IsUserAllowedAccess)
            {
                return resultingDraft;
            }

            if (backupDraftReturn == null)
            {
                DraftReturn newBackupDraftReturn = new DraftReturn(originalDraftReturn);
                newBackupDraftReturn.DraftReturnId = 0;
                newBackupDraftReturn.DraftReturnStatus = DraftReturnStatus.Backup;

                dataRepository.Insert(newBackupDraftReturn);
                await dataRepository.SaveChangesAsync();
            }

            resultingDraft = CastDatabaseDraftReturnToDraft(originalDraftReturn);

            await SetMetadataAsync(resultingDraft, userIdRequestingAccess);

            return resultingDraft;
        }

        private async Task SetMetadataAsync(Draft draft, long userIdRequestingAccess)
        {
            draft.LastWrittenByUserId = userIdRequestingAccess;
            draft.LastWrittenDateTime = VirtualDateTime.Now;

            DraftReturn originalDraftReturn = await GetDraftReturnFromDatabase(draft.OrganisationId, draft.SnapshotYear, DraftReturnStatus.Original);
            await SetDraftReturnFromDraft(originalDraftReturn, draft);
        }

        private async Task SetDraftAccessInformationAsync(long userRequestingAccessToDraft, Draft draft)
        {
            DraftReturn originalDraftReturn = await GetDraftReturnFromDatabase(draft.OrganisationId, draft.SnapshotYear, DraftReturnStatus.Original);

            if (originalDraftReturn == null)
            {
                draft.LastWrittenDateTime = VirtualDateTime.Now;
            }
            else
            {
                draft.LastWrittenDateTime = originalDraftReturn.LastWrittenDateTime;
            }

            (bool IsAllowedAccess, long UserId) result = await GetIsUserAllowedAccessAsync(userRequestingAccessToDraft, draft);
            draft.IsUserAllowedAccess = result.IsAllowedAccess;
            draft.LastWrittenByUserId = result.UserId;
        }

        private async Task<(bool IsAllowedAccess, long UserId)> GetIsUserAllowedAccessAsync(long userRequestingAccessToDraft, Draft draft)
        {
            DraftReturn draftReturn = await GetDraftReturnFromDatabase(draft.OrganisationId, draft.SnapshotYear, DraftReturnStatus.Original);
            if (draftReturn == null)
            {
                return (true, 0);
            }

            (bool IsAllowedAccess, long UserId) result =
                await GetIsUserTheLastPersonThatWroteOnTheFileAsync(draft.OrganisationId, draft.SnapshotYear, DraftReturnStatus.Original, userRequestingAccessToDraft);

            return (result.IsAllowedAccess || !IsInUse(draft.LastWrittenDateTime), result.UserId);
        }

        private async Task<(bool IsAllowedAccess, long UserId)> GetIsUserTheLastPersonThatWroteOnTheFileAsync(long organisationId,
            int snapshotYear,
            DraftReturnStatus draftReturnStatus,
            long userId)
        {
            DraftReturn draftReturn = await GetDraftReturnFromDatabase(organisationId, snapshotYear, draftReturnStatus);

            long lastAccessedByUserId = draftReturn.LastWrittenByUserId ?? 0;
            return (lastAccessedByUserId == 0 || lastAccessedByUserId == userId, lastAccessedByUserId);
        }

        private bool IsInUse(DateTime? lastWrittenDateTime)
        {
            return lastWrittenDateTime != null // System was able to read the file's time-stamp
                   && VirtualDateTime.Now.AddMinutes(-20) <= lastWrittenDateTime;
        }

        private async Task WriteInDbAndTimestampAsync(Draft resultingDraft, long userIdRequestingAccess)
        {
            await SetMetadataAsync(resultingDraft, userIdRequestingAccess);

            DraftReturn draftFromDb = await GetDraftReturnFromDatabase(resultingDraft.OrganisationId, resultingDraft.SnapshotYear, DraftReturnStatus.Original);
            await SetDraftReturnFromDraft(draftFromDb, resultingDraft);
        }

        private async Task<DraftReturn> GetDraftReturnFromDatabase(long organisationId, int snapshotYear, DraftReturnStatus draftReturnStatus)
        {
            return await dataRepository.GetAll<DraftReturn>()
                .FirstOrDefaultAsync(
                    d => d.OrganisationId == organisationId
                         && d.SnapshotYear == snapshotYear
                         && d.DraftReturnStatus == draftReturnStatus);
        }

        private Draft CastDatabaseDraftReturnToDraft(DraftReturn draftReturn)
        {
            if (draftReturn == null)
            {
                return null;
            }
            Draft result = new Draft
            {
                OrganisationId = draftReturn.OrganisationId,
                SnapshotYear = draftReturn.SnapshotYear,
                DraftReturnStatus = draftReturn.DraftReturnStatus,
                HasDraftBeenModifiedDuringThisSession = draftReturn.HasDraftBeenModifiedDuringThisSession ?? false,
                LastWrittenByUserId = draftReturn.LastWrittenByUserId ?? 0,
                LastWrittenDateTime = draftReturn.LastWrittenDateTime,
                ReturnViewModelContent = new ReturnViewModel
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
                }

            };
            return result;
        }

        private async Task SetDraftReturnFromDraft(DraftReturn draftReturn, Draft draft)
        {
            if (draftReturn == null)
            {
                dataRepository.Insert(new DraftReturn { OrganisationId = draft.OrganisationId, SnapshotYear = draft.SnapshotYear, DraftReturnStatus = draft.DraftReturnStatus });
                await dataRepository.SaveChangesAsync();
                draftReturn = await GetDraftReturnFromDatabase(draft.OrganisationId, draft.SnapshotYear, draft.DraftReturnStatus);
            }

            draftReturn.AccountingDate = draft.ReturnViewModelContent?.AccountingDate;
            draftReturn.Address = draft.ReturnViewModelContent?.Address;
            draftReturn.CompanyLinkToGPGInfo = draft.ReturnViewModelContent?.CompanyLinkToGPGInfo;
            draftReturn.DiffMeanBonusPercent = draft.ReturnViewModelContent?.DiffMeanBonusPercent;
            draftReturn.DiffMeanHourlyPayPercent = draft.ReturnViewModelContent?.DiffMeanHourlyPayPercent;
            draftReturn.DiffMedianBonusPercent = draft.ReturnViewModelContent?.DiffMedianBonusPercent;
            draftReturn.DiffMedianHourlyPercent = draft.ReturnViewModelContent?.DiffMedianHourlyPercent;
            draftReturn.EHRCResponse = draft.ReturnViewModelContent?.EHRCResponse;
            draftReturn.EncryptedOrganisationId = draft.ReturnViewModelContent?.EncryptedOrganisationId;
            draftReturn.FemaleLowerPayBand = draft.ReturnViewModelContent?.FemaleLowerPayBand;
            draftReturn.FemaleMedianBonusPayPercent = draft.ReturnViewModelContent?.FemaleMedianBonusPayPercent;
            draftReturn.FemaleMiddlePayBand = draft.ReturnViewModelContent?.FemaleMiddlePayBand;
            draftReturn.FemaleUpperPayBand = draft.ReturnViewModelContent?.FemaleUpperPayBand;
            draftReturn.FemaleUpperQuartilePayBand = draft.ReturnViewModelContent?.FemaleUpperQuartilePayBand;
            draftReturn.FirstName = draft.ReturnViewModelContent?.FirstName;
            draftReturn.HasDraftBeenModifiedDuringThisSession = draft.HasDraftBeenModifiedDuringThisSession;
            draftReturn.IsDifferentFromDatabase = draft.ReturnViewModelContent?.IsDifferentFromDatabase;
            draftReturn.IsInScopeForThisReportYear = draft.ReturnViewModelContent?.IsInScopeForThisReportYear;
            draftReturn.IsLateSubmission = draft.ReturnViewModelContent?.IsLateSubmission;
            draftReturn.IsVoluntarySubmission = draft.ReturnViewModelContent?.IsVoluntarySubmission;
            draftReturn.JobTitle = draft.ReturnViewModelContent?.JobTitle;
            draftReturn.LastName = draft.ReturnViewModelContent?.LastName;
            draftReturn.LastWrittenByUserId = draft.LastWrittenByUserId;
            draftReturn.LastWrittenDateTime = draft.LastWrittenDateTime;
            draftReturn.LateReason = draft.ReturnViewModelContent?.LateReason;
            draftReturn.LatestAddress = draft.ReturnViewModelContent?.LatestAddress;
            draftReturn.LatestOrganisationName = draft.ReturnViewModelContent?.LatestOrganisationName;
            draftReturn.LatestSector = draft.ReturnViewModelContent?.LatestSector;
            draftReturn.MaleLowerPayBand = draft.ReturnViewModelContent?.MaleLowerPayBand;
            draftReturn.MaleMedianBonusPayPercent = draft.ReturnViewModelContent?.MaleMedianBonusPayPercent;
            draftReturn.MaleMiddlePayBand = draft.ReturnViewModelContent?.MaleMiddlePayBand;
            draftReturn.MaleUpperPayBand = draft.ReturnViewModelContent?.MaleUpperPayBand;
            draftReturn.MaleUpperQuartilePayBand = draft.ReturnViewModelContent?.MaleUpperQuartilePayBand;
            draftReturn.Modified = draft.ReturnViewModelContent?.Modified ?? VirtualDateTime.Now;
            draftReturn.OrganisationName = draft.ReturnViewModelContent?.OrganisationName;
            draftReturn.OrganisationSize = draft.ReturnViewModelContent?.OrganisationSize;
            draftReturn.OriginatingAction = draft.ReturnViewModelContent?.OriginatingAction;
            draftReturn.ReportingRequirement = draft.ReturnViewModelContent?.ReportInfo.ReportingRequirement;
            draftReturn.ReportingStartDate = draft.ReturnViewModelContent?.ReportInfo.ReportingStartDate;
            draftReturn.ReportModifiedDate = draft.ReturnViewModelContent?.ReportInfo.ReportModifiedDate;
            draftReturn.ReturnId = draft.ReturnViewModelContent?.ReturnId;
            draftReturn.ReturnUrl = draft.ReturnViewModelContent?.ReturnUrl;
            draftReturn.Sector = draft.ReturnViewModelContent?.Sector;
            draftReturn.SectorType = draft.ReturnViewModelContent?.SectorType;
            draftReturn.ShouldProvideLateReason = draft.ReturnViewModelContent?.ShouldProvideLateReason;

            await dataRepository.SaveChangesAsync();
        }
        #endregion

    }
}
