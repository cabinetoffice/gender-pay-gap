using GenderPayGap.Core.Interfaces;
using GenderPayGap.WebJob.Services;

namespace GenderPayGap.WebJob
{
    public partial class Functions
    {

        public Functions(
            IDataRepository dataRepository,
            EmailSendingService emailSendingService)
        {
            _DataRepository = dataRepository;
            this.emailSendingService = emailSendingService;
        }

        public readonly IDataRepository _DataRepository;
        private readonly EmailSendingService emailSendingService;

    }
}
