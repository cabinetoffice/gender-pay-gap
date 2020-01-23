using System.Threading.Tasks;
using GenderPayGap.Database;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace GenderPayGap.WebUI.Areas.Account.Abstractions
{
    public interface IChangeEmailViewService
    {

        Task<ModelStateDictionary> InitiateChangeEmailAsync(string newEmailAddress, User currentUser);

        Task<ModelStateDictionary> CompleteChangeEmailAsync(string code, User currentUser);

    }

}
