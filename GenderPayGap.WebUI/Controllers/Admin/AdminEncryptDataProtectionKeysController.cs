using System.Collections.Generic;
using System.IO;
using System.Linq;
using CsvHelper;
using GenderPayGap.Core;
using GenderPayGap.Core.Interfaces;
using GenderPayGap.Database.Models;
using GenderPayGap.Extensions;
using GenderPayGap.WebUI.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GenderPayGap.WebUI.Controllers
{
    [Authorize(Roles = LoginRoles.GpgAdmin)]
    [Route("admin")]
    public class AdminEncryptDataProtectionKeysController : Controller
    {

        private readonly IDataRepository dataRepository;

        public AdminEncryptDataProtectionKeysController(IDataRepository dataRepository)
        {
            this.dataRepository = dataRepository;
        }

        [HttpGet("encrypt-data-protection-keys")]
        public IActionResult ViewFeedback()
        {
            List<DataProtectionKey> keys = dataRepository.GetAll<DataProtectionKey>().ToList();

            foreach (DataProtectionKey key in keys)
            {
                if (!Encryption.IsEncryptedData(key.Xml))
                {
                    key.Xml = Encryption.EncryptData(key.Xml);
                }
            }

            dataRepository.SaveChangesAsync().Wait();

            return Json("done");
        }


    }
}
