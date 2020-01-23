using System;
using System.Collections.Concurrent;
using System.Text;
using System.Threading.Tasks;
using GenderPayGap.BusinessLogic.Classes;
using GenderPayGap.BusinessLogic.Models.Submit;
using GenderPayGap.Core.Interfaces;
using GenderPayGap.Extensions;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

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

        public DraftFileBusinessLogic(IFileRepository fileRepository)
        {
            _fileRepository = fileRepository;
        }

        #endregion

        #region attributes

        private readonly IFileRepository _fileRepository;
        private const string metaDataLastWrittenByUserId = "lastWrittenByUserId";
        private const string metaDataLastWrittenTimeStamp = "lastWrittenTimeStamp";

        #endregion

        #region public methods

        public async Task<Draft> GetDraftIfAvailableAsync(long organisationId, int snapshotYear)
        {
            var result = new Draft(organisationId, snapshotYear);

            // When_Not_Json_Return_Null() // 1
            if (!await _fileRepository.GetFileExistsAsync(result.DraftPath))
            {
                return null;
            }

            ReturnViewModel deserialisedJsonReturnViewModel = await DeserialiseDraftContentAsync(result.DraftPath);

            // When_Json_Has_Data_And_Not_Bak_File_Return_Draft() // 2
            if (deserialisedJsonReturnViewModel != null && !await _fileRepository.GetFileExistsAsync(result.BackupDraftPath))
            {
                result.ReturnViewModelContent = deserialisedJsonReturnViewModel;
                return result;
            }

            // When_Json_Empty_And_Not_Bak_File_Return_Null() // 3
            if (deserialisedJsonReturnViewModel == null && !await _fileRepository.GetFileExistsAsync(result.BackupDraftPath))
            {
                return null;
            }

            // When_Json_Has_Data_And_Bak_File_Has_Data_Return_Bak_Draft_Info() // 4
            ReturnViewModel deserialisedBakReturnViewModel = await DeserialiseDraftContentAsync(result.BackupDraftPath);

            if (deserialisedJsonReturnViewModel != null && deserialisedBakReturnViewModel != null)
            {
                result.ReturnViewModelContent = deserialisedBakReturnViewModel;
                return result;
            }

            // When_Json_Has_Data_And_Bak_File_Empty_Return_Null() // 5
            if (deserialisedJsonReturnViewModel != null && deserialisedBakReturnViewModel == null)
            {
                return null;
            }

            // When_Json_Empty_And_Bak_File_Empty_Return_Null() // 6
            if (deserialisedJsonReturnViewModel == null && deserialisedBakReturnViewModel == null)
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

            draft.HasDraftBeenModifiedDuringThisSession = default; // front end flag, reset before saving.

            draftFile.ReturnViewModelContent = postedReturnViewModel;
            byte[] objectConvertedToBytesSequence = SerialiseReturnViewModel(draftFile);
            await WriteAndTimestampAsync(draftFile, objectConvertedToBytesSequence, userIdRequestingAccess);

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

            await GetDraftAccessInformationAsync(userIdRequestingLock, draftExpectedToBeLocked);
        }

        //private bool HasReturnViewModelChanged(Draft draftToBackup, ReturnViewModel postedReturnViewModel)
        //{
        //    if (!_fileRepository.GetFileExists(draftToBackup.DraftPath))
        //        return false;

        //    var storedReturnViewModel = DeserialiseDraftContent(draftToBackup.DraftPath);

        //    if (!postedReturnViewModel.HasUserData())
        //        return false;

        //    return !postedReturnViewModel.AreDraftFieldsEqual(storedReturnViewModel);
        //}

        public async Task DiscardDraftAsync(Draft draftToDiscard)
        {
            await CheckBeforeDeleteAsync(draftToDiscard.DraftPath);
            await CheckBeforeDeleteAsync(draftToDiscard.BackupDraftPath);
        }

        public async Task RestartDraftAsync(long organisationId, int snapshotYear, long userIdRequestingRollback)
        {
            var draftToRollback = new Draft(organisationId, snapshotYear);
            await GetDraftAccessInformationAsync(userIdRequestingRollback, draftToRollback);
            if (!draftToRollback.IsUserAllowedAccess)
            {
                return;
            }

            bool hasRollbackSucceeded = await RollbackDraftAsync(draftToRollback);

            if (hasRollbackSucceeded)
            {
                await _fileRepository.CopyFileAsync(draftToRollback.DraftPath, draftToRollback.BackupDraftPath, true);
            }
        }

        public async Task<bool> RollbackDraftAsync(Draft draftToDiscard)
        {
            if (!await _fileRepository.GetFileExistsAsync(draftToDiscard.BackupDraftPath))
            {
                return false;
            }

            await CheckBeforeDeleteAsync(draftToDiscard.DraftPath);
            await _fileRepository.CopyFileAsync(draftToDiscard.BackupDraftPath, draftToDiscard.DraftPath, true);
            await CommitDraftAsync(draftToDiscard);

            return true;
        }

        public async Task CommitDraftAsync(Draft draftToDiscard)
        {
            await SetMetadataAsync(draftToDiscard, 0);
            await CheckBeforeDeleteAsync(draftToDiscard.BackupDraftPath);
        }

        #endregion

        #region private methods

        private async Task<Draft> GetDraftOrCreateAsync(Draft resultingDraft, long userIdRequestingAccess)
        {
            if (!await _fileRepository.GetFileExistsAsync(resultingDraft.DraftPath))
            {
                await WriteAndTimestampAsync(resultingDraft, new byte[] { }, userIdRequestingAccess);
                await _fileRepository.CopyFileAsync(resultingDraft.DraftPath, resultingDraft.BackupDraftPath, true);
                await GetDraftAccessInformationAsync(userIdRequestingAccess, resultingDraft);
                return resultingDraft;
            }

            await GetDraftAccessInformationAsync(userIdRequestingAccess, resultingDraft);

            if (!resultingDraft.IsUserAllowedAccess)
            {
                return resultingDraft;
            }

            if (!await _fileRepository.GetFileExistsAsync(resultingDraft.BackupDraftPath))
            {
                await _fileRepository.CopyFileAsync(resultingDraft.DraftPath, resultingDraft.BackupDraftPath, true);
            }

            await SetMetadataAsync(resultingDraft, userIdRequestingAccess);
            resultingDraft.ReturnViewModelContent = await DeserialiseDraftContentAsync(resultingDraft.DraftPath);

            return resultingDraft;
        }

        private async Task CheckBeforeDeleteAsync(string draftFilePath)
        {
            if (await _fileRepository.GetFileExistsAsync(draftFilePath))
            {
                await _fileRepository.DeleteFileAsync(draftFilePath);
            }
        }

        private async Task<string> CheckBeforeReadAsync(string draftFilePath)
        {
            return await _fileRepository.GetFileExistsAsync(draftFilePath)
                ? await _fileRepository.ReadAsync(draftFilePath)
                : string.Empty;
        }

        private async Task<DateTime?> CheckBeforeGetLastWriteTimeAsync(string draftFilePath)
        {
            if (await _fileRepository.GetFileExistsAsync(draftFilePath))
            {
                string metaData = await _fileRepository.GetMetaDataAsync(draftFilePath, metaDataLastWrittenTimeStamp);
                return metaData.ToDateTime();
            }

            return null;
        }

        private async Task SetMetadataAsync(Draft draft, long userIdRequestingAccess)
        {
            await _fileRepository.SetMetaDataAsync(draft.DraftPath, metaDataLastWrittenByUserId, userIdRequestingAccess.ToString());
            await _fileRepository.SetMetaDataAsync(draft.DraftPath, metaDataLastWrittenTimeStamp, VirtualDateTime.Now.ToString());
        }

        private async Task GetDraftAccessInformationAsync(long userRequestingAccessToDraft, Draft draft)
        {
            draft.LastWrittenDateTime = await CheckBeforeGetLastWriteTimeAsync(draft.DraftPath);

            (bool IsAllowedAccess, long UserId) result = await GetIsUserAllowedAccessAsync(userRequestingAccessToDraft, draft);
            draft.IsUserAllowedAccess = result.IsAllowedAccess;
            draft.LastWrittenByUserId = result.UserId;
        }

        private async Task<(bool IsAllowedAccess, long UserId)> GetIsUserAllowedAccessAsync(long userRequestingAccessToDraft, Draft draft)
        {
            if (!await _fileRepository.GetFileExistsAsync(draft.DraftPath))
            {
                return (true, 0);
            }

            (bool IsAllowedAccess, long UserId) result =
                await GetIsUserTheLastPersonThatWroteOnTheFileAsync(draft.DraftPath, userRequestingAccessToDraft);

            return (result.IsAllowedAccess || !IsInUse(draft.LastWrittenDateTime), result.UserId);
        }

        private async Task<(bool IsAllowedAccess, long UserId)> GetIsUserTheLastPersonThatWroteOnTheFileAsync(string draftFilePath,
            long userId)
        {
            string metaData = await _fileRepository.GetMetaDataAsync(draftFilePath, metaDataLastWrittenByUserId);
            int lastAccessedByUserId = metaData.ToInt32();
            return (lastAccessedByUserId == 0 || lastAccessedByUserId == userId, lastAccessedByUserId);
        }

        private bool IsInUse(DateTime? lastWrittenDateTime)
        {
            return lastWrittenDateTime != null // System was able to read the file's time-stamp
                   && VirtualDateTime.Now.AddMinutes(-20) <= lastWrittenDateTime;
        }

        private async Task<ReturnViewModel> DeserialiseDraftContentAsync(string draftFilePath)
        {
            string jsonDraftFileContents = await CheckBeforeReadAsync(draftFilePath);

            #region json serialiser settings

            var exceptions = new ConcurrentBag<Exception>();

            var jsonSerializerSettings = new JsonSerializerSettings {
                Error = delegate(object sender, ErrorEventArgs args) {
                    exceptions.Add(new Exception(args.ErrorContext.Error.Message));
                    args.ErrorContext.Handled = true;
                }
            };
            if (exceptions.Count > 0)
            {
                throw new AggregateException($"Error De-serializing {draftFilePath}", exceptions);
            }

            #endregion

            return JsonConvert.DeserializeObject<ReturnViewModel>(jsonDraftFileContents, jsonSerializerSettings);
        }

        private byte[] SerialiseReturnViewModel(Draft draft)
        {
            #region serialiser settings

            var exceptions = new ConcurrentBag<Exception>();

            var jsonSerializerSettings = new JsonSerializerSettings {
                NullValueHandling = NullValueHandling.Ignore,
                DefaultValueHandling = DefaultValueHandling.Ignore,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                Error = delegate(object sender, ErrorEventArgs args) {
                    exceptions.Add(new Exception(args.ErrorContext.Error.Message));
                    args.ErrorContext.Handled = true;
                }
            };
            if (exceptions.Count > 0)
            {
                throw new AggregateException($"Error serializing {draft.DraftPath}", exceptions);
            }

            #endregion


            string serializedObject = JsonConvert.SerializeObject(
                draft.ReturnViewModelContent,
                Formatting.Indented,
                jsonSerializerSettings);

            return Encoding.UTF8.GetBytes(serializedObject);
        }

        private async Task WriteAndTimestampAsync(Draft resultingDraft, byte[] contentToWrite, long userIdRequestingAccess)
        {
            await _fileRepository.WriteAsync(resultingDraft.DraftPath, contentToWrite);
            await SetMetadataAsync(resultingDraft, userIdRequestingAccess);
        }

        #endregion

    }
}
