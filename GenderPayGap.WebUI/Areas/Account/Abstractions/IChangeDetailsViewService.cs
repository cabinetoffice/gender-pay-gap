using System.Threading.Tasks;
using GenderPayGap.Database;
using GenderPayGap.WebUI.Areas.Account.ViewModels;

namespace GenderPayGap.WebUI.Areas.Account.Abstractions
{

    public interface IChangeDetailsViewService
    {

        Task<bool> ChangeDetailsAsync(ChangeDetailsViewModel newDetails, User currentUser);

    }

}
