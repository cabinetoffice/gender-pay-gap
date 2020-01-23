using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CsvHelper;
using GenderPayGap.Core;
using GenderPayGap.Core.Interfaces;
using GenderPayGap.Database;
using GenderPayGap.Database.Models;
using GenderPayGap.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GenderPayGap.WebUI.Controllers
{
    [Authorize(Roles = "GPGadmin")]
    [Route("admin")]
    public class AdminMiscController : Controller
    {

        private readonly IDataRepository dataRepository;

        public AdminMiscController(IDataRepository dataRepository)
        {
            this.dataRepository = dataRepository;
        }

        [HttpGet("salt-passwords")]
        public async Task<IActionResult> SaltedHash(long? previousUserId = null)
        {
            DateTime start = DateTime.Now;
            long? startedFrom = previousUserId;
            long finishedUpTo = previousUserId ?? 0;

            while (DateTime.Now.Subtract(start).TotalSeconds < 10)
            {
                User user = dataRepository
                    .GetAll<User>()
                    .Where(u => u.HashingAlgorithm == HashingAlgorithm.SHA512)
                    .Where(u => u.UserId > finishedUpTo)
                    .OrderBy(u => u.UserId)
                    .FirstOrDefault();

                if (user == null)
                {
                    break;
                }

                byte[] salt = Crypto.GetSalt();
                user.PasswordHash = Crypto.GetPBKDF2(user.PasswordHash, salt);
                user.HashingAlgorithm = HashingAlgorithm.PBKDF2AppliedToSHA512;
                user.Salt = Convert.ToBase64String(salt);
                await dataRepository.SaveChangesAsync();

                finishedUpTo = user.UserId;
            }

            var model = new Dictionary<string, long?> { { "StartedFrom", startedFrom }, { "FinishedUpTo", finishedUpTo } };
            return Ok(model);
        }

    }
}
