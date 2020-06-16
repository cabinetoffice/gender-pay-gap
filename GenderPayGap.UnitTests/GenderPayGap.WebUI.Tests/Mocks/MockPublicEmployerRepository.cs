using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GenderPayGap.Core.Classes;
using GenderPayGap.Core.Interfaces;
using GenderPayGap.Core.Models;
using GenderPayGap.Extensions;

public class MockPublicEmployerRepository : IPagedRepository<EmployerRecord>
{

    public List<EmployerRecord> AllEmployers = new List<EmployerRecord>();

    public void ClearSearch() { }

    public void Delete(EmployerRecord employer)
    {
        AllEmployers.Remove(employer);
    }

    public void Insert(EmployerRecord employer)
    {
        AllEmployers.Add(employer);
    }

    public Task<string> GetSicCodesAsync(string companyNumber)
    {
        string sics = null;
        if (!string.IsNullOrWhiteSpace(companyNumber))
        {
            sics = AllEmployers.FirstOrDefault(c => c.CompanyNumber == companyNumber)?.SicCodeIds;
        }

        if (!string.IsNullOrWhiteSpace(sics))
        {
            sics = "," + sics;
        }

        sics = "1" + sics;
        return Task.FromResult(sics);
    }

    public Task<PagedResult<EmployerRecord>> SearchAsync(string searchText, int page, int pageSize)
    {
        //var searchResults = PublicSectorOrgs.Messages.List.Where(o => o.Name.ContainsI(searchText));

        var result = new PagedResult<EmployerRecord> {Results = new List<EmployerRecord>()};

        //result.Results = AllEmployers.Where(e => e.Name.ContainsI(searchText)).Page(page, pageSize).ToList();
        //TODO: ste -> using this until Page function lines 879 and 888  in Lists.cs is fixed.  
        result.Results = AllEmployers.Where(e => e.OrganisationName.ContainsI(searchText)).ToList();

        //DONE:NastyBug! Page method arguments Page(pageSize, page) where in vice-versa positions as in Page(page, pageSize)! now fixed 
        result.ActualRecordTotal = result.Results.Count;
        result.VirtualRecordTotal = result.Results.Count;
        result.CurrentPage = page;
        result.PageSize = pageSize;
        // result.Results = searchResults;
        return Task.FromResult(result);
    }

    public PagedResult<EmployerRecord> Search(string searchText, int page, int pageSize)
    {
        var result = new PagedResult<EmployerRecord>();
        List<EmployerRecord> allEmployers = AllEmployers.Where(e => e.OrganisationName.ContainsI(searchText)).ToList();
        //DONE:NastyBug! Page method arguments Page(pageSize, page) where in vice-versa positions as in Page(page, pageSize)! now fixed 
        result.Results = allEmployers.Page(page, pageSize).ToList();
        result.ActualRecordTotal = allEmployers.Count;
        result.VirtualRecordTotal = result.Results.Count;
        result.CurrentPage = page;
        result.PageSize = pageSize;
        return result;
    }

}
