using System.Threading.Tasks;
using GenderPayGap.Core;
using GenderPayGap.Core.Interfaces;
using GenderPayGap.Database;

namespace GenderPayGap.WebUI.BusinessLogic.Abstractions
{

    public interface IUserRepository : IDataTransaction
    {

        bool CheckPassword(User user, string password);

        Task<User> FindByEmailAsync(string email, params UserStatuses[] filterStatuses);

        void UpdateEmail(User userToUpdate, string newEmailAddress);

        void UpdatePassword(User userToUpdate, string newPassword);

        void RetireUser(User userToRetire);

        void UpdateUserPasswordUsingPBKDF2(User currentUser, string password);
    }

}
