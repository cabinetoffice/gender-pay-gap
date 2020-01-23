using GenderPayGap.Core.Abstractions;

namespace GenderPayGap.Core.Models
{

    public class OrganisationRegistrationRemovedTemplate : AEmailTemplate
    {

        public string CurrentUser { get; set; }

        public string OrganisationName { get; set; }

    }

}
