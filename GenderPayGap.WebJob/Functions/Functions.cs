using GenderPayGap.Core.Interfaces;
using GenderPayGap.WebJob.Services;

namespace GenderPayGap.WebJob
{
    public partial class Functions
    {

        public Functions(
            EmailSendingService emailSendingService)
        {
            this.emailSendingService = emailSendingService;
        }

        private readonly EmailSendingService emailSendingService;

    }
}
