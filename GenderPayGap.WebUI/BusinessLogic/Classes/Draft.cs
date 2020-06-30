using System;
using GenderPayGap.WebUI.BusinessLogic.Models.Submit;

namespace GenderPayGap.WebUI.BusinessLogic.Classes
{
    [Serializable]
    public class Draft
    {

        #region Constructor

        public Draft() { }

        public Draft(long organisationId, int snapshotYear)
        {
            SnapshotYear = snapshotYear;
            OrganisationId = organisationId;
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
        public int SnapshotYear { get; set; }
        public long OrganisationId { get; set; }
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

    }
}
