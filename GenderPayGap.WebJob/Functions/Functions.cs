using System;
using GenderPayGap.BusinessLogic;
using GenderPayGap.BusinessLogic.Services;
using GenderPayGap.Core.Classes.Logger;
using GenderPayGap.Core.Interfaces;
using GenderPayGap.Extensions;
using GenderPayGap.Extensions.AspNetCore;
using GenderPayGap.WebJob.Services;

namespace GenderPayGap.WebJob
{
    public partial class Functions
    {

        public Functions(
            IDataRepository dataRepository,
            IScopeBusinessLogic scopeBL,
            ISubmissionBusinessLogic submissionBL,
            IOrganisationBusinessLogic orgBL,
            ISearchBusinessLogic searchBusinessLogic,
            EmailSendingService emailSendingService,
            UpdateFromCompaniesHouseService updateFromCompaniesHouseService)
        {
            _DataRepository = dataRepository;
            _ScopeBL = scopeBL;
            _SubmissionBL = submissionBL;
            _OrganisationBL = orgBL;
            _SearchBusinessLogic = searchBusinessLogic;
            _updateFromCompaniesHouseService = updateFromCompaniesHouseService;
            this.emailSendingService = emailSendingService;
        }

        #region Properties

        public readonly IDataRepository _DataRepository;
        private readonly IScopeBusinessLogic _ScopeBL;
        private readonly ISubmissionBusinessLogic _SubmissionBL;
        private readonly IOrganisationBusinessLogic _OrganisationBL;
        private readonly ISearchBusinessLogic _SearchBusinessLogic;
        private readonly EmailSendingService emailSendingService;
        private readonly UpdateFromCompaniesHouseService _updateFromCompaniesHouseService;

        private static readonly ConcurrentSet<string> RunningJobs = new ConcurrentSet<string>();
        private static readonly ConcurrentSet<string> StartedJobs = new ConcurrentSet<string>();

        #endregion

    }
}
