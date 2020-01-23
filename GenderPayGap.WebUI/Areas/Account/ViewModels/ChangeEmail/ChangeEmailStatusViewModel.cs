using System;

namespace GenderPayGap.WebUI.Areas.Account.ViewModels
{

    [Serializable]
    public class ChangeEmailStatusViewModel
    {

        public string OldEmail { get; set; }

        public string NewEmail { get; set; }

    }

}
