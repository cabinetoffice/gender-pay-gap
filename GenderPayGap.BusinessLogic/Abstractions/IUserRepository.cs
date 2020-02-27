﻿using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using GenderPayGap.BusinessLogic.Account.Models;
using GenderPayGap.Core;
using GenderPayGap.Core.Interfaces;
using GenderPayGap.Database;

namespace GenderPayGap.BusinessLogic.Account.Abstractions
{

    public interface IUserRepository : IDataTransaction
    {

        Task<bool> CheckPasswordAsync(User user, string password);

        Task<User> FindBySubjectIdAsync(long subjectId, params UserStatuses[] filterStatuses);

        Task<User> FindBySubjectIdAsync(string subjectId, params UserStatuses[] filterStatuses);

        Task<User> FindByEmailAsync(string email, params UserStatuses[] filterStatuses);

        Task<List<User>> FindAllUsersByNameAsync(string name);

        Task<User> AutoProvisionUserAsync(string provider, string providerUserId, List<Claim> list);

        Task<User> FindByExternalProviderAsync(string provider, string providerUserId);

        void UpdateEmail(User userToUpdate, string newEmailAddress);

        void UpdatePassword(User userToUpdate, string newPassword);

        bool UpdateDetails(User userToUpdate, UpdateDetailsModel changeDetails);

        void RetireUser(User userToRetire);

        void UpdateUserPasswordUsingPBKDF2(User currentUser, string password);
    }

}
