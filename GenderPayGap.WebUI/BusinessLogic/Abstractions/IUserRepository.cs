using System.Collections.Generic;
using System.Threading.Tasks;
using GenderPayGap.Core;
using GenderPayGap.Core.Interfaces;
using GenderPayGap.Database;
using GenderPayGap.WebUI.BusinessLogic.Models.Account;

namespace GenderPayGap.WebUI.BusinessLogic.Abstractions
{

    public interface IUserRepository : IDataTransaction
    {

        Task<bool> CheckPasswordAsync(User user, string password);

        Task<User> FindBySubjectIdAsync(long subjectId, params UserStatuses[] filterStatuses);

        Task<User> FindByEmailAsync(string email, params UserStatuses[] filterStatuses);

        Task<List<User>> FindAllUsersByNameAsync(string name);

        void UpdateEmail(User userToUpdate, string newEmailAddress);

        void UpdatePassword(User userToUpdate, string newPassword);

        bool UpdateDetails(User userToUpdate, UpdateDetailsModel changeDetails);

        void RetireUser(User userToRetire);

        void UpdateUserPasswordUsingPBKDF2(User currentUser, string password);
    }

}
