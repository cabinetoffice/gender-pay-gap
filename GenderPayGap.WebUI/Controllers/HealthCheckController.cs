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
            try
            {
                string fileContentsToSave = Guid.NewGuid().ToString("N");
            
                string healthCheckFileName = $"health-check-files/health-check-file-{Guid.NewGuid().ToString("N")}.txt";

                fileRepository.Write(healthCheckFileName, fileContentsToSave);
                Thread.Sleep(250);

                string fileContentsRead = fileRepository.Read(healthCheckFileName);
                Thread.Sleep(250);
            
                fileRepository.Delete(healthCheckFileName);

                if (fileContentsRead != fileContentsToSave)
                {
                    throw new Exception($"Could not read or write a file: fileContentsRead({fileContentsRead}) did not match fileContentsToSave({fileContentsToSave})");
                }
            }
            catch (Exception e)
            {
                throw new Exception($"Could not read or write a file: {e.Message}", e);
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
