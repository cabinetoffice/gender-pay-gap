﻿using System.ComponentModel;
using GenderPayGap.Core;
using GenderPayGap.Core.Interfaces;
using GenderPayGap.Database;
using GenderPayGap.Extensions;
using GenderPayGap.WebUI.Helpers;
using GenderPayGap.WebUI.Services;

namespace GenderPayGap.WebUI.Repositories
{
    public class UserRepository
    {
        private readonly IDataRepository dataRepository;
        private readonly AuditLogger auditLogger;

        public UserRepository(IDataRepository dataRepository, AuditLogger auditLogger)
        {
            this.dataRepository = dataRepository ?? throw new ArgumentNullException(nameof(dataRepository));
            this.auditLogger = auditLogger;
        }

        public User FindByEmail(string email, params UserStatuses[] filterStatuses)
        {
            return dataRepository.GetAll<User>()
                .Where(user => filterStatuses.Length == 0 || filterStatuses.ToList().Contains(user.Status))
                .FirstOrDefault(user => user.EmailAddressDB == User.EncryptEmailAddress(email));
        }

        public bool CheckPassword(User user, string password, bool isReset = false)
        {
            try
            {
                user.LoginDate = VirtualDateTime.Now;
                if (CheckPasswordBasedOnHashingAlgorithm(user, password))
                {
                    user.LoginAttempts = 0;
                    if (isReset)
                    {
                        user.ResetAttempts = 0;
                    }

                    if (user.HashingAlgorithm != HashingAlgorithm.PBKDF2)
                    {
                        UpdateUserPasswordUsingPBKDF2(user, password);
                    }

                    return true;
                }
                user.LoginAttempts++;
                if (isReset)
                {
                    user.ResetAttempts++;
                }

                return false;
            }
            finally
            {
                //Save the changes
                dataRepository.SaveChanges();
            }
        }

        public void UpdateEmail(User userToUpdate, string newEmailAddress)
        {
            if (userToUpdate is null)
            {
                throw new ArgumentNullException(nameof(userToUpdate));
            }

            if (string.IsNullOrWhiteSpace(newEmailAddress))
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
            dataRepository.SaveChanges();

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

            if (string.IsNullOrWhiteSpace(newPassword))
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
            dataRepository.SaveChanges();

            // log password changed
            auditLogger.AuditChangeToUser(
                AuditedAction.UserChangePassword,
                userToUpdate,
                new { }, // We don't want to save the passwords(!) so there's not really anything else to save here
                userToUpdate);
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
            dataRepository.SaveChanges();

            // log status changed
            auditLogger.AuditChangeToUser(
                AuditedAction.UserRetiredThemselves,
                userToRetire,
                new { }, // There's no interesting details to include here
                userToRetire);
        }

        private bool CheckPasswordBasedOnHashingAlgorithm(User user, string password)
        {
            switch (user.HashingAlgorithm)
            {
                case HashingAlgorithm.SHA512:
                    return user.PasswordHash == PasswordHelper.GetSHA512Checksum(password);
                case HashingAlgorithm.PBKDF2:
                    return user.PasswordHash == PasswordHelper.GetPBKDF2(password, Convert.FromBase64String(user.Salt));
                case HashingAlgorithm.PBKDF2AppliedToSHA512:
                    return user.PasswordHash == PasswordHelper.GetPBKDF2(PasswordHelper.GetSHA512Checksum(password), Convert.FromBase64String(user.Salt));
                case HashingAlgorithm.Unknown:
                    throw new InvalidOperationException($"Hashing algorithm should not be unknown {user.HashingAlgorithm}");
                default:
                    throw new InvalidEnumArgumentException($"Invalid enum argument: {user.HashingAlgorithm}");
            }
        }

        private void UpdateUserPasswordUsingPBKDF2(User user, string password)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            byte[] salt = PasswordHelper.GetSalt();
            user.Salt = Convert.ToBase64String(salt);
            user.PasswordHash = PasswordHelper.GetPBKDF2(password, salt);
            user.HashingAlgorithm = HashingAlgorithm.PBKDF2;

            dataRepository.SaveChanges();
        }

    }
}
