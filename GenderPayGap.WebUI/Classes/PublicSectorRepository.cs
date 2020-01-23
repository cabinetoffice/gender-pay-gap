using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GenderPayGap.Core;
using GenderPayGap.Core.API;
using GenderPayGap.Core.Classes;
using GenderPayGap.Core.Interfaces;
using GenderPayGap.Core.Models;
using GenderPayGap.Database;
using GenderPayGap.Extensions;
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

        public async Task<PagedResult<EmployerRecord>> SearchAsync(string searchText, int page, int pageSize, bool test = false)
        {
            var result = new PagedResult<EmployerRecord>();
            if (test)
            {
                var employers = new List<EmployerRecord>();

                int min = await _DataRepository.GetAll<Organisation>().CountAsync();

                int id = Numeric.Rand(min, int.MaxValue - 1);
                var employer = new EmployerRecord {
                    OrganisationName = Global.TestPrefix + "_GovDept_" + id,
                    CompanyNumber = ("_" + id).Left(10),
                    Address1 = "Test Address 1",
                    Address2 = "Test Address 2",
                    City = "Test Address 3",
                    Country = "Test Country",
                    PostCode = "Test Post Code",
                    EmailDomains = "*@*",
                    PoBox = null,
                    SicCodeIds = "1"
                };
                employers.Add(employer);

                result.ActualRecordTotal = employers.Count;
                result.VirtualRecordTotal = employers.Count;
                result.CurrentPage = page;
                result.PageSize = pageSize;
                result.Results = employers;
                return result;
            }

            List<Organisation> searchResults = await _DataRepository.GetAll<Organisation>()
                .Where(o => o.SectorType == SectorTypes.Public && o.Status == OrganisationStatuses.Active)
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
