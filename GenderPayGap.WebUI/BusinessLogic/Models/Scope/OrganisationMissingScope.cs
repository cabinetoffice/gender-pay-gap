﻿using System.Collections.Generic;

namespace GenderPayGap.BusinessLogic.Models.Scope
{
    public class OrganisationMissingScope
    {

        public Database.Organisation Organisation { get; set; }

        public List<int> MissingSnapshotYears { get; set; }

    }
}
