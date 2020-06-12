using System;
using System.Collections.Generic;
using System.Linq;
using GenderPayGap.BusinessLogic.Services;
using GenderPayGap.Core;
using GenderPayGap.Core.Interfaces;
using GenderPayGap.Database;
using GenderPayGap.Extensions;
using GenderPayGap.WebUI.Helpers;
using GenderPayGap.WebUI.Models.Admin;
using GenderPayGap.WebUI.Models.AdminReferenceData;
using GenderPayGap.WebUI.Services;
using GovUkDesignSystem;
using GovUkDesignSystem.Parsers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace GenderPayGap.WebUI.Controllers
{
    [Authorize(Roles = "GPGadmin")]
    [Route("admin")]
    public class AdminReferenceDataController : Controller
    {

        private readonly AuditLogger auditLogger;
        private readonly IDataRepository dataRepository;

        public AdminReferenceDataController(IDataRepository dataRepository, AuditLogger auditLogger)
        {
            this.dataRepository = dataRepository;
            this.auditLogger = auditLogger;
        }

        [HttpGet("reference-data")]
        public IActionResult ReferenceData()
        {
            var viewModel = new AdminUploadsViewModel
            {
                SicSectionsCount = dataRepository.GetAll<SicSection>().Count(), 
                SicCodesCount = dataRepository.GetAll<SicCode>().Count()
            };

            return View("ReferenceData", viewModel);
        }

        [HttpGet("reference-data/sic-sections/download")]
        public FileContentResult DownloadAllSicSections()
        {
            List<SicSection> allSicSections = dataRepository.GetAll<SicSection>().ToList();

            var records = allSicSections
                .Select(GetSicSectionDetails)
                .ToList();

            string fileDownloadName = $"Gpg-SicSections-{VirtualDateTime.Now:yyyy-MM-dd HH:mm}.csv";
            FileContentResult fileContentResult = DownloadHelper.CreateCsvDownload(records, fileDownloadName);

            return fileContentResult;
        }

        private static object GetSicSectionDetails(SicSection sicSection)
        {
            return new {sicSection.SicSectionId, sicSection.Description};
        }

        [HttpGet("reference-data/sic-codes/download")]
        public FileContentResult DownloadAllSicCodes()
        {
            List<SicCode> allSicCodes = dataRepository.GetAll<SicCode>().ToList();

            var records = allSicCodes.Select(GetSicCodeDetails)
                .ToList();

            string fileDownloadName = $"Gpg-SicCodes-{VirtualDateTime.Now:yyyy-MM-dd HH:mm}.csv";
            FileContentResult fileContentResult = DownloadHelper.CreateCsvDownload(records, fileDownloadName);

            return fileContentResult;
        }

        private static object GetSicCodeDetails(SicCode sicCode)
        {
            return new {sicCode.SicCodeId, sicCode.SicSectionId, sicCode.Description, sicCode.Synonyms};
        }

        [HttpGet("reference-data/sic-sections/upload")]
        public IActionResult SicSectionUploadGet()
        {
            var viewModel = new AdminFileUploadViewModel();
            return View("SicSectionUpload", viewModel);
        }

        [HttpGet("reference-data/sic-codes/upload")]
        public IActionResult SicCodeUploadGet()
        {
            var viewModel = new AdminFileUploadViewModel();
            return View("SicCodeUpload", viewModel);
        }

        [HttpPost("reference-data/sic-sections/upload")]
        public IActionResult SicSectionUploadPost(AdminFileUploadViewModel viewModel)
        {
            if (!ReferenceDataHelper.TryParseCsvFileWithHeadings(
                viewModel.File,
                new[] { "SicSectionId", "Description" },
                out List<SicSection> sicSectionsFromUploadFile,
                out string errorMessage))
            {
                viewModel.AddErrorFor(m => m.File, errorMessage);
                return View("SicSectionUpload", viewModel);
            }

            List<SicSection> sicSectionsFromDatabase = dataRepository.GetAll<SicSection>().ToList();

            var uploadCheckViewModel = new AdminSicSectionUploadCheckViewModel
            {
                SerializedNewRecords = JsonConvert.SerializeObject(sicSectionsFromUploadFile),
                AddsEditsDeletesSet = new AddsEditsDeletesSet<SicSection>(
                    sicSectionsFromDatabase,
                    sicSectionsFromUploadFile,
                    s => s.SicSectionId,
                    (s1, s2) => s1.Description == s2.Description,
                    s => s.SicCodes.Count > 0)
            };

            return View("SicSectionUploadCheck", uploadCheckViewModel);
        }

        [HttpPost("reference-data/sic-codes/upload")]
        public IActionResult SicCodeUploadPost(AdminFileUploadViewModel viewModel)
        {
            if (!ReferenceDataHelper.TryParseCsvFileWithHeadings(
                viewModel.File,
                new[] { "SicCodeId", "SicSectionId", "Description", "Synonyms" },
                out List<SicCode> sicCodesFromUploadFile,
                out string errorMessage))
            {
                viewModel.AddErrorFor(m => m.File, errorMessage);
                return View("SicCodeUpload", viewModel);
            }

            List<SicCode> sicCodesFromDatabase = dataRepository.GetAll<SicCode>().ToList();

            var sicCodeUploadCheckViewModel = new AdminSicCodeUploadCheckViewModel
            {
                SerializedNewRecords = JsonConvert.SerializeObject(sicCodesFromUploadFile),
                AddsEditsDeletesSet = new AddsEditsDeletesSet<SicCode>(
                    sicCodesFromDatabase,
                    sicCodesFromUploadFile,
                    s => s.SicCodeId,
                    (s1, s2) => s1.SicSectionId == s2.SicSectionId 
                                && s1.Description == s2.Description 
                                && s1.Synonyms == s2.Synonyms,
                    s => s.OrganisationSicCodes.Count > 0)
            };

            return View("SicCodeUploadCheck", sicCodeUploadCheckViewModel);
        }

        [HttpPost("reference-data/sic-sections/upload/check")]
        public IActionResult SicSectionUploadCheckPost(AdminSicSectionUploadCheckViewModel viewModel)
        {
            var newRecords = JsonConvert.DeserializeObject<List<SicSection>>(viewModel.SerializedNewRecords);

            List<SicSection> sicSectionsFromDatabase = dataRepository.GetAll<SicSection>().ToList();

            viewModel.AddsEditsDeletesSet =
                new AddsEditsDeletesSet<SicSection>(
                    sicSectionsFromDatabase,
                    newRecords,
                    s => s.SicSectionId,
                    (s1, s2) => s1.Description == s2.Description
                );

            viewModel.ParseAndValidateParameters(Request, m => m.Reason);
            if (viewModel.HasAnyErrors())
            {
                return View("SicSectionUploadCheck", viewModel);
            }

            List<SicSection> existingRecords = dataRepository.GetAll<SicSection>().ToList();
            auditLogger.AuditGeneralAction(AuditedAction.AdminUpdatedSicSections, new
            {
                ExistingRecords = JsonConvert.SerializeObject(existingRecords.Select(GetSicSectionDetails)),
                NewRecords = JsonConvert.SerializeObject(newRecords.Select(GetSicSectionDetails)),
                Reason = viewModel.Reason
            }, User);

            SaveChangesToSicSections(viewModel);
            
            return View("UploadConfirmation", FileUploadType.SicSection);
        }

        private void SaveChangesToSicSections(AdminSicSectionUploadCheckViewModel viewModel)
        {
            foreach (SicSection sicSectionFromUser in viewModel.AddsEditsDeletesSet.ItemsToAdd)
            {
                var sicSection = new SicSection
                {
                    SicSectionId = sicSectionFromUser.SicSectionId,
                    Description = sicSectionFromUser.Description
                };
                dataRepository.Insert(sicSection);
            }

            foreach (OldAndNew<SicSection> oldAndNew in viewModel.AddsEditsDeletesSet.ItemsToChange)
            {
                SicSection sicSection = dataRepository.Get<SicSection>(oldAndNew.Old.SicSectionId);
                sicSection.Description = oldAndNew.New.Description;
            }

            foreach (SicSection sicSectionFromUser in viewModel.AddsEditsDeletesSet.ItemsToDelete)
            {
                SicSection sicSection = dataRepository.Get<SicSection>(sicSectionFromUser.SicSectionId);
                dataRepository.Delete(sicSection);
            }

            dataRepository.SaveChangesAsync().Wait();
        }

        [HttpPost("reference-data/sic-codes/upload/check")]
        public IActionResult SicCodeUploadCheckPost(AdminSicCodeUploadCheckViewModel viewModel)
        {
            var newRecords = JsonConvert.DeserializeObject<List<SicCode>>(viewModel.SerializedNewRecords);

            List<SicCode> sicCodesFromDatabase = dataRepository.GetAll<SicCode>().ToList();

            viewModel.AddsEditsDeletesSet =
                new AddsEditsDeletesSet<SicCode>(
                    sicCodesFromDatabase,
                    newRecords,
                    s => s.SicCodeId,
                    (s1, s2) => s1.SicSectionId == s2.SicSectionId 
                                && s1.Description == s2.Description
                                && s1.Synonyms == s2.Synonyms,
                    s => s.OrganisationSicCodes.Count > 0);

            viewModel.ParseAndValidateParameters(Request, m => m.Reason);
            if (viewModel.HasAnyErrors())
            {
                return View("SicCodeUploadCheck", viewModel);
            }

            if (viewModel.AddsEditsDeletesSet.AnyItemsThatCannotBeDeleted)
            {
                throw new Exception("Some SIC codes are not allowed to be deleted, because they have OrganisationSicCodes associated with them");
            }

            List<SicCode> existingRecords = dataRepository.GetAll<SicCode>().ToList();
            auditLogger.AuditGeneralAction(AuditedAction.AdminUpdatedSicCodes, new
            {
                ExistingRecords = JsonConvert.SerializeObject(existingRecords.Select(GetSicCodeDetails)),
                NewRecords = JsonConvert.SerializeObject(newRecords.Select(GetSicCodeDetails)),
                Reason = viewModel.Reason
            }, User);

            SaveChangesToSicCodes(viewModel);
            
            return View("UploadConfirmation", FileUploadType.SicCode);
        }

        private void SaveChangesToSicCodes(AdminSicCodeUploadCheckViewModel viewModel)
        {
            foreach (SicCode sicCodeFromUser in viewModel.AddsEditsDeletesSet.ItemsToAdd)
            {
                var sicCode = new SicCode{
                    SicCodeId = sicCodeFromUser.SicCodeId,
                    SicSectionId = sicCodeFromUser.SicSectionId,
                    Description = sicCodeFromUser.Description,
                    Synonyms = sicCodeFromUser.Synonyms
                };
                dataRepository.Insert(sicCode);
            }

            foreach (OldAndNew<SicCode> oldAndNew in viewModel.AddsEditsDeletesSet.ItemsToChange)
            {
                SicCode sicCode = dataRepository.Get<SicCode>(oldAndNew.Old.SicCodeId);

                sicCode.SicSectionId = oldAndNew.New.SicSectionId;
                sicCode.Description = oldAndNew.New.Description;
                sicCode.Synonyms = oldAndNew.New.Synonyms;
            }

            foreach (SicCode sicCodeFromUser in viewModel.AddsEditsDeletesSet.ItemsToDelete)
            {
                SicCode sicCode = dataRepository.Get<SicCode>(sicCodeFromUser.SicCodeId);
                dataRepository.Delete(sicCode);
            }

            dataRepository.SaveChangesAsync().Wait();
        }

    }
}
