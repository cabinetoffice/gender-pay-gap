using System;
using System.Linq;
using System.Threading.Tasks;
using GenderPayGap.BusinessLogic.Account.Abstractions;
using GenderPayGap.Core;
using GenderPayGap.Database;
using IdentityServer4.Extensions;
using IdentityServer4.Models;
using IdentityServer4.Services;
using Microsoft.Extensions.Logging;

namespace GenderPayGap.IdentityServer4.Classes
{
    public class CustomProfileService : IProfileService
    {

        protected readonly IUserRepository _userRepository;
        protected readonly ILogger Logger;

        public CustomProfileService(IUserRepository userRepository, ILogger<CustomProfileService> logger)
        {
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public Task GetProfileDataAsync(ProfileDataRequestContext context)
        {
            Logger.LogDebug(
                "Get profile called for subject {subject} from client {client} with claim types {claimTypes} via {caller}",
                context.Subject.GetSubjectId(),
                context.Client.ClientName ?? context.Client.ClientId,
                context.RequestedClaimTypes,
                context.Caller);

            //Issue the requested claims for the user
            context.IssuedClaims = context.Subject.Claims.Where(x => context.RequestedClaimTypes.Contains(x.Type)).ToList();

            return Task.CompletedTask;
        }

        public async Task IsActiveAsync(IsActiveContext context)
        {
            //Ensure the user is new, active or retired (retired needed for logout redirect otherwise will fail)
            User user = await _userRepository.FindBySubjectIdAsync(
                context.Subject.GetSubjectId(),
                UserStatuses.New,
                UserStatuses.Active,
                UserStatuses.Retired);
            context.IsActive = user != null;
        }

    }
}
