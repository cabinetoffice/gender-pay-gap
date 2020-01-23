using System;
using System.IO;
using GenderPayGap.BusinessLogic.Models.Submit;
using GenderPayGap.Core;

namespace GenderPayGap.BusinessLogic.Classes
{
    [Serializable]
    public class Draft
    {

        #region Constructor

        private Draft() { }

        public Draft(long organisationId, int snapshotYear)
        {
            DraftFilename = GetDraftFileName(organisationId, snapshotYear, "json");
            DraftPath = GetDraftFilePath(DraftFilename);

            BackupDraftFilename = GetDraftFileName(organisationId, snapshotYear, "bak");
            BackupDraftPath = GetDraftFilePath(BackupDraftFilename);
        }

        public Draft(long organisationId,
            int snapshotYear,
            bool isUserAllowedAccess,
            DateTime? lastWrittenDateTime,
            long lastWrittenByUserId) :
            this(organisationId, snapshotYear)
        {
            IsUserAllowedAccess = isUserAllowedAccess;
            LastWrittenDateTime = lastWrittenDateTime;
            LastWrittenByUserId = lastWrittenByUserId;
        }

        #endregion

        #region Public methods

        public string DraftFilename { get; set; }
        public string DraftPath { get; set; }
        public string BackupDraftFilename { get; set; }
        public string BackupDraftPath { get; set; }
        public bool IsUserAllowedAccess { get; set; }
        public DateTime? LastWrittenDateTime { get; set; }
        public long LastWrittenByUserId { get; set; }
        public ReturnViewModel ReturnViewModelContent { get; set; }
        public bool HasDraftBeenModifiedDuringThisSession { get; set; }

        public bool HasContent()
        {
            return ReturnViewModelContent != null;
        }

        #endregion

        #region Private methods

        private string GetDraftFileName(long organisationId, int snapshotYear, string fileExtension)
        {
            return $"{organisationId}_{snapshotYear}.{fileExtension}";
        }

        private string GetDraftFilePath(string draftFileName)
        {
            return Path.Combine(Global.DataPath, Global.SaveDraftPath, draftFileName);
        }

        #endregion

    }
}
