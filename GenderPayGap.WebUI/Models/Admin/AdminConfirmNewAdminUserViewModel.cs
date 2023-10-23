using GenderPayGap.Database;

namespace GenderPayGap.WebUI.Models.Admin
{
    public class AdminConfirmNewAdminUserViewModel 
    {

        public User User { get; set; }

        public bool ReadOnly { get; set; }

    }
}
