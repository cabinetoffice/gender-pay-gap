using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using GenderPayGap.BusinessLogic.Account.Abstractions;
using GenderPayGap.BusinessLogic.Account.Models;
using GenderPayGap.BusinessLogic.Services;
using GenderPayGap.Core;
using GenderPayGap.Core.Interfaces;
using GenderPayGap.Database;
using GenderPayGap.Extensions;
using Microsoft.EntityFrameworkCore;

namespace GenderPayGap.BusinessLogic.Account.Repositories
{

    public class UserRepository : IUserRepository
    {
        private readonly IDataRepository dataRepository;
        private readonly AuditLogger auditLogger;

        public UserRepository(IDataRepository dataRepository, AuditLogger auditLogger)
        {
            this.dataRepository = dataRepository ?? throw new ArgumentNullException(nameof(dataRepository));
            this.auditLogger = auditLogger;
        }

        public async Task<User> FindBySubjectIdAsync(string subjectId, params UserStatuses[] filterStatuses)
        {
            return await FindBySubjectIdAsync(subjectId.ToInt64(), filterStatuses);
        }

        public async Task<User> FindBySubjectIdAsync(long userId, params UserStatuses[] filterStatuses)
        {
            return await dataRepository.GetAll<User>()
                // filter by user id
                .Where(x => x.UserId == userId)
                // skip or filter by user status
                .Where(user => filterStatuses.Length == 0 || filterStatuses.Contains(user.Status))
                // return first match otherwise null
                .FirstOrDefaultAsync();
        }

        public async Task<User> FindByEmailAsync(string email, params UserStatuses[] filterStatuses)
        {
            return await dataRepository.GetAll<User>()
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

            IQueryable<User> foundUsers = dataRepository
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
                await dataRepository.SaveChangesAsync();
            }
        }

        public void UpdateEmail(User userToUpdate, string newEmailAddress)
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
            dataRepository.SaveChangesAsync().Wait();

            // log email change
            auditLogger.AuditChangeToUser(
                AuditedAction.UserChangeEmailAddress,
                userToUpdate,
                new
                {
                    OldEmailAddress = oldEmailAddress,
                    NewEmailAddress = newEmailAddress,
                },
                userToUpdate);
        }

        public void UpdatePassword(User userToUpdate, string newPassword)
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
            dataRepository.SaveChangesAsync().Wait();

            // log password changed
            auditLogger.AuditChangeToUser(
                AuditedAction.UserChangePassword,
                userToUpdate,
                new { }, // We don't want to save the passwords(!) so there's not really anything else to save here
                userToUpdate);
        }

        public bool UpdateDetails(User userToUpdate, UpdateDetailsModel changeDetails)
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

            // log details changed
            if (userToUpdate.Firstname != changeDetails.FirstName ||
                userToUpdate.Lastname != changeDetails.LastName ||
                userToUpdate.ContactFirstName != changeDetails.FirstName ||
                userToUpdate.ContactLastName != changeDetails.LastName)
            {
                auditLogger.AuditChangeToUser(
                    AuditedAction.UserChangeName,
                    userToUpdate,
                    new
                    {
                        OldFirstName = userToUpdate.Firstname,
                        OldLastName = userToUpdate.Lastname,
                        OldContactFirstName = userToUpdate.ContactFirstName,
                        OldContactLastName = userToUpdate.ContactLastName,
                        NewFirstName = changeDetails.FirstName,
                        NewLastName = changeDetails.LastName,
                    },
                    userToUpdate);
            }
            if (userToUpdate.JobTitle != changeDetails.JobTitle ||
                userToUpdate.ContactJobTitle != changeDetails.JobTitle)
            {
                auditLogger.AuditChangeToUser(
                    AuditedAction.UserChangeJobTitle,
                    userToUpdate,
                    new
                    {
                        OldJobTitle = userToUpdate.JobTitle,
                        OldContactJobTitle = userToUpdate.ContactJobTitle,
                        NewJobTitle = changeDetails.JobTitle,
                    },
                    userToUpdate);
            }
            if (userToUpdate.ContactPhoneNumber != changeDetails.ContactPhoneNumber)
            {
                auditLogger.AuditChangeToUser(
                    AuditedAction.UserChangePhoneNumber,
                    userToUpdate,
                    new
                    {
                        OldContactPhoneNumber = userToUpdate.ContactPhoneNumber,
                        NewContactPhoneNumber = changeDetails.ContactPhoneNumber,
                    },
                    userToUpdate);
            }
            if (userToUpdate.SendUpdates != changeDetails.SendUpdates ||
                userToUpdate.AllowContact != changeDetails.AllowContact)
            {
                auditLogger.AuditChangeToUser(
                    AuditedAction.UserChangeContactPreferences,
                    userToUpdate,
                    new
                    {
                        OldSendUpdates = userToUpdate.SendUpdates,
                        OldAllowContactForUserResearch = userToUpdate.AllowContact,
                        NewSendUpdates = changeDetails.SendUpdates,
                        NewAllowContactForUserResearch = changeDetails.AllowContact,
                    },
                    userToUpdate);
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
            dataRepository.SaveChangesAsync().Wait();

            // success
            return true;
        }

        public void RetireUser(User userToRetire)
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
            dataRepository.SaveChangesAsync().Wait();

            // log status changed
            auditLogger.AuditChangeToUser(
                AuditedAction.UserRetiredThemselves,
                userToRetire,
                new { }, // There's no interesting details to include here
                userToRetire);
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

            dataRepository.SaveChangesAsync().Wait();
        }

        #region IDataTransaction

        public async Task BeginTransactionAsync(Func<Task> delegateAction)
        {
            await dataRepository.BeginTransactionAsync(delegateAction);
        }

        public void CommitTransaction()
        {
            dataRepository.CommitTransaction();
        }

        public void RollbackTransaction()
        {
            dataRepository.RollbackTransaction();
        }

        #endregion

    }

}
