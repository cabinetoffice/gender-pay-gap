using System;

namespace GenderPayGap.WebUI.BusinessLogic.Models.Account
{

    [Serializable]
    public class UpdateDetailsModel
    {

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string JobTitle { get; set; }

        public string ContactPhoneNumber { get; set; }

        public bool SendUpdates { get; set; }

        public bool AllowContact { get; set; }

    }

}
