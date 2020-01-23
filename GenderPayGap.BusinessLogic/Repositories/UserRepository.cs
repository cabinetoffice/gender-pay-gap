using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using GenderPayGap.BusinessLogic.Account.Abstractions;
using GenderPayGap.BusinessLogic.Account.Models;
using GenderPayGap.BusinessLogic.LogRecords;
using GenderPayGap.Core;
using GenderPayGap.Core.Interfaces;
using GenderPayGap.Database;
using GenderPayGap.Extensions;
using Microsoft.EntityFrameworkCore;

namespace GenderPayGap.BusinessLogic.Account.Repositories
{

    public class UserRepository : IUserRepository
    {

        public UserRepository(IDataRepository dataRepository, IUserLogRecord userRecordLog)
        {
            DataRepository = dataRepository ?? throw new ArgumentNullException(nameof(dataRepository));
            UserRecordLog = userRecordLog ?? throw new ArgumentNullException(nameof(userRecordLog));
        }

        public async Task<User> FindBySubjectIdAsync(string subjectId, params UserStatuses[] filterStatuses)
        {
            return await FindBySubjectIdAsync(subjectId.ToInt64(), filterStatuses);
        }

        public async Task<User> FindBySubjectIdAsync(long userId, params UserStatuses[] filterStatuses)
        {
            return await DataRepository.GetAll<User>()
                // filter by user id
                .Where(x => x.UserId == userId)
                // skip or filter by user status
                .Where(user => filterStatuses.Length == 0 || filterStatuses.Contains(user.Status))
                // return first match otherwise null
                .FirstOrDefaultAsync();
        }

        public async Task<User> FindByEmailAsync(string email, params UserStatuses[] filterStatuses)
        {
            return await DataRepository.GetAll<User>()
                // filter by email address
                .Where(user => user.EmailAddress.ToLower() == email.ToLower())
                // skip or filter by user status
                .Where(user => filterStatuses.Length == 0 || filterStatuses.Contains(user.Status))
                // return first match otherwise null
                .FirstOrDefaultAsync();
        }

        public async Task<List<User>> FindAllUsersByNameAsync(string name)
        {
            string nameForSearch = name?.ToLower();

            IQueryable<User> foundUsers = DataRepository
                .GetAll<User>()
                .Where(x => x.Fullname.ToLower().Contains(nameForSearch) || x.ContactFullname.ToLower().Contains(nameForSearch));

            return await foundUsers.ToListAsync();
        }

        public async Task<bool> CheckPasswordAsync(User user, string password)
        {
            try
            {
                user.LoginDate = VirtualDateTime.Now;
                if (CheckPasswordBasedOnHashingAlgorithm(user, password))
                {
                    user.LoginAttempts = 0;

                    if (user.HashingAlgorithm != HashingAlgorithm.PBKDF2)
                    {
                        UpdateUserPasswordUsingPBKDF2(user, password);
                    }

                    return true;
                }

                user.LoginAttempts++;
                return false;
            }
            finally
            {
                //Save the changes
                await DataRepository.SaveChangesAsync();
            }
        }

        public async Task UpdateEmailAsync(User userToUpdate, string newEmailAddress)
        {
            if (userToUpdate is null)
            {
                throw new ArgumentNullException(nameof(userToUpdate));
            }

            if (newEmailAddress.IsNullOrWhiteSpace())
            {
                throw new ArgumentNullException(nameof(newEmailAddress));
            }

            if (userToUpdate.Status != UserStatuses.Active)
            {
                throw new ArgumentException($"Can only update emails for active users. UserId={userToUpdate.UserId}");
            }

            string oldEmailAddress = userToUpdate.EmailAddress;

            // update email
            DateTime now = VirtualDateTime.Now;
            userToUpdate.EmailAddress = newEmailAddress;
            userToUpdate.EmailVerifiedDate = now;
            userToUpdate.Modified = now;

            // save
            await DataRepository.SaveChangesAsync();

            // log email change
            await UserRecordLog.LogEmailChangedAsync(oldEmailAddress, newEmailAddress, userToUpdate, userToUpdate.EmailAddress);
        }

        public async Task UpdatePasswordAsync(User userToUpdate, string newPassword)
        {
            if (userToUpdate is null)
            {
                throw new ArgumentNullException(nameof(userToUpdate));
            }

            if (newPassword.IsNullOrWhiteSpace())
            {
                throw new ArgumentNullException(nameof(newPassword));
            }

            if (userToUpdate.Status != UserStatuses.Active)
            {
                throw new ArgumentException($"Can only update passwords for active users. UserId={userToUpdate.UserId}");
            }

            UpdateUserPasswordUsingPBKDF2(userToUpdate, newPassword);

            userToUpdate.Modified = VirtualDateTime.Now;

            // save
            await DataRepository.SaveChangesAsync();

            // log password changed
            await UserRecordLog.LogPasswordChangedAsync(userToUpdate, userToUpdate.EmailAddress);
        }

        public async Task<bool> UpdateDetailsAsync(User userToUpdate, UpdateDetailsModel changeDetails)
        {
            if (userToUpdate is null)
            {
                throw new ArgumentNullException(nameof(userToUpdate));
            }

            if (changeDetails is null)
            {
                throw new ArgumentNullException(nameof(changeDetails));
            }

            if (userToUpdate.Status != UserStatuses.Active)
            {
                throw new ArgumentException($"Can only update details for active users. UserId={userToUpdate.UserId}");
            }

            // check we have changes
            var originalDetails = Mapper.Map<UpdateDetailsModel>(userToUpdate);
            if (originalDetails.Equals(changeDetails))
            {
                return false;
            }

            // update current user with new details
            userToUpdate.Firstname = changeDetails.FirstName;
            userToUpdate.Lastname = changeDetails.LastName;
            userToUpdate.JobTitle = changeDetails.JobTitle;
            userToUpdate.ContactFirstName = changeDetails.FirstName;
            userToUpdate.ContactLastName = changeDetails.LastName;
            userToUpdate.ContactJobTitle = changeDetails.JobTitle;
            userToUpdate.ContactPhoneNumber = changeDetails.ContactPhoneNumber;
            userToUpdate.SendUpdates = changeDetails.SendUpdates;
            userToUpdate.AllowContact = changeDetails.AllowContact;
            userToUpdate.Modified = VirtualDateTime.Now;

            // save
            await DataRepository.SaveChangesAsync();

            // log details changed
            await UserRecordLog.LogDetailsChangedAsync(originalDetails, changeDetails, userToUpdate, userToUpdate.EmailAddress);

            // success
            return true;
        }

        public async Task RetireUserAsync(User userToRetire)
        {
            if (userToRetire is null)
            {
                throw new ArgumentNullException(nameof(userToRetire));
            }

            if (userToRetire.Status != UserStatuses.Active)
            {
                throw new ArgumentException($"Can only retire active users. UserId={userToRetire.UserId}");
            }

            // update status
            userToRetire.SetStatus(UserStatuses.Retired, userToRetire, "User retired");
            userToRetire.Modified = VirtualDateTime.Now;

            // save
            await DataRepository.SaveChangesAsync();

            // log status changed
            await UserRecordLog.LogUserRetiredAsync(userToRetire, userToRetire.EmailAddress);
        }

        public Task<User> AutoProvisionUserAsync(string provider, string providerUserId, List<Claim> list)
        {
            throw new NotImplementedException();
        }

        public Task<User> FindByExternalProviderAsync(string provider, string providerUserId)
        {
            throw new NotImplementedException();
        }

        private bool CheckPasswordBasedOnHashingAlgorithm(User user, string password)
        {
            switch (user.HashingAlgorithm)
            {
                case HashingAlgorithm.SHA512:
                    return user.PasswordHash == Crypto.GetSHA512Checksum(password);
                case HashingAlgorithm.PBKDF2:
                    return user.PasswordHash == Crypto.GetPBKDF2(password, Convert.FromBase64String(user.Salt));
                case HashingAlgorithm.PBKDF2AppliedToSHA512:
                    return user.PasswordHash == Crypto.GetPBKDF2(Crypto.GetSHA512Checksum(password), Convert.FromBase64String(user.Salt));
                case HashingAlgorithm.Unknown:
                    throw new InvalidOperationException($"Hashing algorithm should not be unknown {user.HashingAlgorithm}");
                default:
                    throw new InvalidEnumArgumentException($"Invalid enum argument: {user.HashingAlgorithm}");
            }
        }

        public void UpdateUserPasswordUsingPBKDF2(User user, string password)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            byte[] salt = Crypto.GetSalt();
            user.Salt = Convert.ToBase64String(salt);
            user.PasswordHash = Crypto.GetPBKDF2(password, salt);
            user.HashingAlgorithm = HashingAlgorithm.PBKDF2;

            DataRepository.SaveChangesAsync().Wait();
        }

        #region Dependencies

        public IDataRepository DataRepository { get; }

        public IUserLogRecord UserRecordLog { get; }

        #endregion

        #region IDataTransaction

        public async Task BeginTransactionAsync(Func<Task> delegateAction)
        {
            await DataRepository.BeginTransactionAsync(delegateAction);
        }

        public void CommitTransaction()
        {
            DataRepository.CommitTransaction();
        }

        public void RollbackTransaction()
        {
            DataRepository.RollbackTransaction();
        }

        #endregion

    }

}
