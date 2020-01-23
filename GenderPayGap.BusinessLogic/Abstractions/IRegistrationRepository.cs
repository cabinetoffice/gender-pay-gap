using System.Threading.Tasks;
using GenderPayGap.Core.Interfaces;
using GenderPayGap.Database;

namespace GenderPayGap.BusinessLogic.Account.Abstractions
{

    public interface IRegistrationRepository : IDataTransaction
    {

        Task RemoveRegistrationAsync(UserOrganisation userOrgToUnregister, User actionByUser);

        Task RemoveRetiredUserRegistrationsAsync(User userToRetire, User actionByUser);

    }

}
