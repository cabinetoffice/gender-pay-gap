using GenderPayGap.Core;
using GenderPayGap.Database;

namespace GenderPayGap.WebUI.BusinessLogic.Abstractions
{

    public interface IUserRepository
    {

        bool CheckPassword(User user, string password, bool isReset = false);

        User FindByEmail(string email, params UserStatuses[] filterStatuses);

        void UpdateEmail(User userToUpdate, string newEmailAddress);

        void UpdatePassword(User userToUpdate, string newPassword);

        void RetireUser(User userToRetire);
    }

}
