using System.Collections.Generic;
using GenderPayGap.Database;
using GovUkDesignSystem;

namespace GenderPayGap.WebUI.Models.Admin
{
    public class AdminSicCodeUploadCheckViewModel : GovUkViewModel
    {

        public FileUploadType FileUploadType { get; set; }

        public List<SicCode> RecordsToCreate { get; set; }

        public List<SicCodeToUpdate> RecordsToUpdate { get; set; }

        public List<SicCode> RecordsToDelete { get; set; }

        public bool AbleToProceed { get; set; }

        public string SerializedNewRecords { get; set; }

        public string Reason { get; set; }

    }

    public class SicCodeToUpdate
    {
        public int SicCodeId { get; set; }

        public string PreviousSicSectionId { get; set; }
        
        public string NewSicSectionId { get; set; }

        public string PreviousDescription { get; set; }

        public string NewDescription { get; set; }

    }

}
