using System;
using System.Linq;
using System.Threading.Tasks;
using GenderPayGap.BusinessLogic.Account.Abstractions;
using GenderPayGap.Core;
using GenderPayGap.Core.Classes.Logger;
using GenderPayGap.Database;
using IdentityServer4.Extensions;
using IdentityServer4.Models;
using IdentityServer4.Services;

namespace GenderPayGap.IdentityServer4.Classes
{
    public class CustomProfileService : IProfileService
    {

        protected readonly IUserRepository _userRepository;

        public CustomProfileService(IUserRepository userRepository)
        {
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        }

        public Task GetProfileDataAsync(ProfileDataRequestContext context)
        {
            CustomLogger.Debug($"Get profile called for subject {context.Subject.GetSubjectId()} from client {context.Client.ClientName ?? context.Client.ClientId} with claim types {context.RequestedClaimTypes} via {context.Caller}");

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
