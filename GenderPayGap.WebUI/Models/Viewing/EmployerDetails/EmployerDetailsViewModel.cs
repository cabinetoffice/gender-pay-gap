using GenderPayGap.Core.Classes;

namespace GenderPayGap.WebUI.Models
{
    public class EmployerDetailsViewModel
    {

        public Database.Organisation Organisation { get; set; }

        public string LastSearchUrl { get; set; }

        public string EmployerBackUrl { get; set; }

        public SessionList<string> ComparedEmployers { get; set; }

    }
}
