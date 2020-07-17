using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GenderPayGap.Core;
using GenderPayGap.Core.Classes;
using GenderPayGap.Core.Interfaces;
using GenderPayGap.Core.Models;
using GenderPayGap.Database;
using GenderPayGap.Extensions;
using GenderPayGap.WebUI.ExternalServices.CompaniesHouse;
using Microsoft.EntityFrameworkCore;

namespace GenderPayGap
{
    public class PublicSectorRepository : IPagedRepository<EmployerRecord>
    {

        private readonly ICompaniesHouseAPI _CompaniesHouseAPI;
        private readonly IDataRepository _DataRepository;

        public PublicSectorRepository(IDataRepository dataRepository, ICompaniesHouseAPI companiesHouseAPI)
        {
            _DataRepository = dataRepository;
            _CompaniesHouseAPI = companiesHouseAPI;
        }

        public async Task<PagedResult<EmployerRecord>> SearchAsync(string searchText, int page, int pageSize)
        {
            var result = new PagedResult<EmployerRecord>();
            List<Organisation> searchResults = await _DataRepository.GetAll<Organisation>()
                .Where(o => o.SectorType == SectorTypes.Public)
                .Where(o => o.Status == OrganisationStatuses.Active)
                .ToListAsync();
            List<Organisation> searchResultsList = searchResults.Where(o => o.OrganisationName.ContainsI(searchText))
                .OrderBy(o => o.OrganisationName)
                .ThenBy(o => o.OrganisationName)
                .ToList();
            result.ActualRecordTotal = searchResultsList.Count;
            result.VirtualRecordTotal = searchResultsList.Count;
            result.CurrentPage = page;
            result.PageSize = pageSize;
            result.Results = searchResultsList.Page(pageSize, page).Select(o => o.ToEmployerRecord()).ToList();
            return result;
        }

        public async Task<string> GetSicCodesAsync(string companyNumber)
        {
            string sics = null;
            if (!string.IsNullOrWhiteSpace(companyNumber))
            {
                sics = await _CompaniesHouseAPI.GetSicCodesAsync(companyNumber);
            }

            if (!string.IsNullOrWhiteSpace(sics))
            {
                sics = "," + sics;
            }

            sics = "1" + sics;
            return sics;
        }

        #region Properties

        public void Delete(EmployerRecord entity)
        {
            throw new NotImplementedException();
        }

        public void Insert(EmployerRecord entity)
        {
            throw new NotImplementedException();
        }

        public void ClearSearch() { }

    }

    #endregion

}
