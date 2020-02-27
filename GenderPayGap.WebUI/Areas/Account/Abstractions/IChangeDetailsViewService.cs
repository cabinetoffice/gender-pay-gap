using System.Threading.Tasks;
using GenderPayGap.Database;
using GenderPayGap.WebUI.Areas.Account.ViewModels;

namespace GenderPayGap.WebUI.Areas.Account.Abstractions
{

    public interface IChangeDetailsViewService
    {
        bool ChangeDetails(ChangeDetailsViewModel newDetails, User currentUser);
    }

}
