﻿using GenderPayGap.Database;

namespace GenderPayGap.WebUI.Models.RemoveUserFromOrganisation
{
    public class RemoveUserFromOrganisationViewModel
    {

        public User LoggedInUser { get; set; }
        public User UserToRemove { get; set; }
        public Organisation Organisation { get; set; }

    }
}
