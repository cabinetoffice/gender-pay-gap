using System.Threading.Tasks;
using GenderPayGap.Database;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace GenderPayGap.WebUI.Areas.Account.Abstractions
{

    public interface ICloseAccountViewService
    {

        Task<ModelStateDictionary> CloseAccountAsync(User currentUser, string currentPassword, User actionByUser);

    }

}
