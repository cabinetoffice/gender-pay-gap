using System.Threading.Tasks;
using GenderPayGap.Core.Interfaces;
using GenderPayGap.Core.Models;

namespace GenderPayGap.WebUI.Classes.Services
{
    public interface IAdminService
    {

        Task<long> GetSearchDocumentCountAsync();

    }

    public class AdminService : IAdminService
    {

        private readonly IDataRepository dataRepository;
        private readonly ISearchRepository<EmployerSearchModel> searchRepository;

        public AdminService(IDataRepository dataRepository, ISearchRepository<EmployerSearchModel> searchRepository)
        {
            this.dataRepository = dataRepository;
            this.searchRepository = searchRepository;
        }

        public async Task<long> GetSearchDocumentCountAsync()
        {
            return await searchRepository.GetDocumentCountAsync();
        }

    }
}
