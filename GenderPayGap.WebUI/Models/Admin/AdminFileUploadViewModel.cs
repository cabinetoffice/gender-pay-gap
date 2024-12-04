using System.ComponentModel.DataAnnotations;

namespace GenderPayGap.WebUI.Models.Admin
{
    public class AdminFileUploadViewModel 
    {

        public IFormFile File { get; set; }

    }

    public enum FileUploadType
    {

        [Display(Name = "SIC section")]
        SicSection = 0,

        [Display(Name = "SIC code")]
        SicCode = 1

    }
}
