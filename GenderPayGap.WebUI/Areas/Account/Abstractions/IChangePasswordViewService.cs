using System.Threading.Tasks;
using GenderPayGap.Database;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace GenderPayGap.WebUI.Areas.Account.Abstractions
{

    public interface IChangePasswordViewService
    {

        Task<ModelStateDictionary> ChangePasswordAsync(User currentUser, string currentPassword, string newPassword);

    }

}
