using System.Collections.Generic;
using GenderPayGap.Database;
using GovUkDesignSystem;

namespace GenderPayGap.WebUI.Models.Admin
{
    public class AdminSicSectionUploadCheckViewModel : GovUkViewModel
    {

        public FileUploadType FileUploadType { get; set; }

        public List<SicSection> RecordsToCreate { get; set; }

        public List<SicSectionToUpdate> RecordsToUpdate { get; set; }

        public List<SicSection> RecordsToDelete { get; set; }

        public bool AbleToProceed { get; set; }

        public string SerializedNewRecords { get; set; }

        public string Reason { get; set; }

    }

    public class SicSectionToUpdate
    {

        public string SicSectionId { get; set; }

        public string PreviousDescription { get; set; }

        public string NewDescription { get; set; }

    }

}
