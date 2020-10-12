using GenderPayGap.Database;
using GovUkDesignSystem;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace GenderPayGap.WebUI.Models.Account
{
    public class ManageAccountViewModel : GovUkViewModel
    {

        [BindNever]
        public User User { get; set; }

    }
}
