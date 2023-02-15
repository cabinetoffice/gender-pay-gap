using System;
using System.Linq;
using GenderPayGap.Core.Interfaces;
using GenderPayGap.Database;
using GenderPayGap.WebUI.ExternalServices.FileRepositories;
using GenderPayGap.WebUI.Search;
using Microsoft.AspNetCore.Mvc;

namespace GenderPayGap.WebUI.Controllers
{
    public class HealthCheckController : Controller
    {

        private readonly IDataRepository dataRepository;
        private readonly IFileRepository fileRepository;
        private const string HealthCheckFileName = "HealthCheckFile.txt";

        public HealthCheckController(
            IDataRepository dataRepository,
            IFileRepository fileRepository)
        {
            this.dataRepository = dataRepository;
            this.fileRepository = fileRepository;
        }


        [HttpGet("health-check")]
        public IActionResult HealthCheck()
        {       
            CheckDatabaseConnection();
            CheckFileConnection();
            CheckSearchRepositoryIsLoaded();

            return new JsonResult(new {Status = "OK"});
        }

        private void CheckDatabaseConnection()
        {
            Organisation anOrganisation = dataRepository.GetAll<Organisation>().FirstOrDefault();

            if (anOrganisation == null)
            {
                throw new Exception("Could not load an organisation from the database");
            }
        }

        private void CheckFileConnection()
        {
            string guid = Guid.NewGuid().ToString("N");

              fileRepository.Write(HealthCheckFileName, guid);

              string fileContents = fileRepository.Read(HealthCheckFileName);

              if (fileContents != guid)
              {
                  throw new Exception("Could not read or write a file");
              }
        }

        private void CheckSearchRepositoryIsLoaded()
        {
            if (SearchRepository.CachedOrganisations == null ||
                SearchRepository.CachedUsers == null)
            {
                throw new Exception("SearchRepository was not loaded");
            }
        }
    }
}
