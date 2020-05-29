using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using CsvHelper;
using GenderPayGap.BusinessLogic.Services;
using GenderPayGap.Core;
using GenderPayGap.Core.Interfaces;
using GenderPayGap.Database;
using GenderPayGap.Extensions;
using GenderPayGap.WebUI.Helpers;
using GenderPayGap.WebUI.Models.Admin;
using GovUkDesignSystem;
using GovUkDesignSystem.Parsers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace GenderPayGap.WebUI.Controllers
{
    [Authorize(Roles = "GPGadmin")]
    [Route("admin")]
    public class AdminUploadsController : Controller
    {

        private readonly AuditLogger auditLogger;
        private readonly IDataRepository dataRepository;

        public AdminUploadsController(IDataRepository dataRepository, AuditLogger auditLogger)
        {
            this.dataRepository = dataRepository;
            this.auditLogger = auditLogger;
        }

        [HttpGet("uploads")]
        public IActionResult Uploads()
        {
            var viewModel = new AdminUploadsViewModel
            {
                SicSectionsCount = dataRepository.GetAll<SicSection>().Count(), 
                SicCodesCount = dataRepository.GetAll<SicCode>().Count()
            };

            return View("Uploads", viewModel);
        }

        [HttpGet("downloads/all-sic-sections")]
        public FileContentResult DownloadAllSicSections()
        {
            List<SicSection> allSicSections = dataRepository.GetAll<SicSection>().ToList();

            var records = allSicSections
                .Select(sicSection => new {sicSection.SicSectionId, sicSection.Description})
                .ToList();

            string fileDownloadName = $"Gpg-SicSections-{VirtualDateTime.Now:yyyy-MM-dd HH:mm}.csv";
            FileContentResult fileContentResult = CsvDownloadHelper.CreateCsvDownload(records, fileDownloadName);

            return fileContentResult;
        }

        [HttpGet("downloads/all-sic-codes")]
        public FileContentResult DownloadAllSicCodes()
        {
            List<SicCode> allSicCodes = dataRepository.GetAll<SicCode>()
                .Include(sicCode => sicCode.SicSection)
                .ToList();

            var records = allSicCodes.Select(sicCode => new {sicCode.SicCodeId, sicCode.SicSectionId, sicCode.Description})
                .ToList();

            string fileDownloadName = $"Gpg-SicCodes-{VirtualDateTime.Now:yyyy-MM-dd HH:mm}.csv";
            FileContentResult fileContentResult = CsvDownloadHelper.CreateCsvDownload(records, fileDownloadName);

            return fileContentResult;
        }

        [HttpGet("uploads/file-upload")]
        public IActionResult FileUploadGet(FileUploadType fileUploadType)
        {
            var viewModel = new AdminFileUploadViewModel {FileUploadType = fileUploadType};
            return View("FileUpload", viewModel);
        }

        [HttpPost("uploads/file-upload")]
        public IActionResult FileUploadPost(AdminFileUploadViewModel viewModel)
        {
            if (viewModel.File == null)
            {
                viewModel.AddErrorFor(m => m.File, "Select a CSV file");
                return View("FileUpload", viewModel);
            }

            if (!Path.GetExtension(viewModel.File.FileName).Equals(".csv"))
            {
                viewModel.AddErrorFor(m => m.File, "The selected file must be a CSV");
                return View("FileUpload", viewModel);
            }

            try
            {
                using (var reader = new StreamReader(viewModel.File.OpenReadStream(), Encoding.UTF8))
                {
                    var csvReader = new CsvReader(reader);
                    csvReader.Configuration.WillThrowOnMissingField = false;
                    csvReader.Configuration.TrimFields = true;
                    csvReader.Configuration.IgnoreQuotes = false;
                    csvReader.Configuration.IgnoreBlankLines = true;
                    csvReader.ReadHeader();
                    string[] fieldHeaders = csvReader.FieldHeaders;

                    switch (viewModel.FileUploadType)
                    {
                        case FileUploadType.SicSection:

                            if (!fieldHeaders.SequenceEqual(new[] {"SicSectionId", "Description"}))
                            {
                                viewModel.AddErrorFor(
                                    m => m.File,
                                    "The selected file has the wrong column headings. Download the current version of this data "
                                    + "on the previous page to find the correct format.");
                                return View("FileUpload", viewModel);
                            }

                            List<SicSection> sicSections = csvReader.GetRecords<SicSection>().ToList();

                            if (sicSections.Count < 1)
                            {
                                viewModel.AddErrorFor(m => m.File, "The selected file is empty");
                                return View("FileUpload", viewModel);
                            }

                            AdminSicSectionUploadCheckViewModel sicSectionUploadCheckViewModelModel = 
                                GenerateAdminSicSectionUploadCheckViewModel(sicSections);
                            
                            if (sicSectionUploadCheckViewModelModel.RecordsToCreate.Count == 0
                                && sicSectionUploadCheckViewModelModel.RecordsToUpdate.Count == 0
                                && sicSectionUploadCheckViewModelModel.RecordsToDelete.Count == 0)
                            {
                                viewModel.AddErrorFor(m => m.File, "The selected file does not contain "
                                                                   + "any changes to the current SIC sections");
                                return View("FileUpload", viewModel);
                            }

                            return View("SicSectionUploadCheck", sicSectionUploadCheckViewModelModel);

                        case FileUploadType.SicCode:
                            if (!fieldHeaders.SequenceEqual(new[] {"SicCodeId", "SicSectionId", "Description"}))
                            {
                                viewModel.AddErrorFor(
                                    m => m.File,
                                    "The selected file has the wrong column headings. Download the current version of this data "
                                    + "on the previous page to find the correct format.");
                                return View("FileUpload", viewModel);
                            }
                            
                            List<SicCode> sicCodes = csvReader.GetRecords<SicCode>().ToList();
                            
                            if (sicCodes.Count < 1)
                            {
                                viewModel.AddErrorFor(m => m.File, "The selected file is empty");
                                return View("FileUpload", viewModel);
                            }

                            AdminSicCodeUploadCheckViewModel sicCodeUploadCheckViewModel =
                                GenerateAdminSicCodeUploadCheckViewModel(sicCodes);
                            
                            if (sicCodeUploadCheckViewModel.RecordsToCreate.Count == 0
                                && sicCodeUploadCheckViewModel.RecordsToUpdate.Count == 0
                                && sicCodeUploadCheckViewModel.RecordsToDelete.Count == 0)
                            {
                                viewModel.AddErrorFor(m => m.File, "The selected file does not contain "
                                                                   + "any changes to the current SIC codes");
                                return View("FileUpload", viewModel);
                            }

                            return View("SicCodeUploadCheck", sicCodeUploadCheckViewModel);

                        
                        default:
                            throw new Exception("Invalid file upload type");
                    }
                }
            }
            catch (Exception ex)
            {
                viewModel.AddErrorFor(m => m.File, 
                    "The selected file could not be uploaded – try again.");
                return View("FileUpload", viewModel);
            }
        }
        
        [HttpPost("uploads/sic-section-upload-check")]
        public IActionResult SicSectionUploadCheckPost(AdminSicSectionUploadCheckViewModel viewModel)
        {
            
            var newRecords = JsonConvert.DeserializeObject<List<SicSection>>(viewModel.SerializedNewRecords);
            
            viewModel.ParseAndValidateParameters(Request, m => m.Reason);
            if (viewModel.HasAnyErrors())
            {
                var uploadCheckViewModel = GenerateAdminSicSectionUploadCheckViewModel(newRecords);
                return View("SicSectionUploadCheck", uploadCheckViewModel);
            }
            
            List<SicSection> existingRecords = dataRepository.GetAll<SicSection>().ToList();
            existingRecords.ForEach(record => dataRepository.Delete(record));
            newRecords.ForEach(record => dataRepository.Insert(record));
            dataRepository.SaveChangesAsync().Wait();
            
            User currentUser = ControllerHelper.GetGpgUserFromAspNetUser(User, dataRepository);
            auditLogger.AuditGeneralAction(AuditedAction.AdminUpdatedSicSections, new
            {
                ExistingRecords = JsonConvert.SerializeObject(existingRecords),
                NewRecords = JsonConvert.SerializeObject(newRecords),
                Reason = viewModel.Reason
            }, currentUser);
            
            return View("UploadConfirmation", viewModel.FileUploadType);
        }
        
        [HttpPost("uploads/sic-code-upload-check")]
        public IActionResult SicCodeUploadCheckPost(AdminSicCodeUploadCheckViewModel viewModel)
        {
            
            var newRecords = JsonConvert.DeserializeObject<List<SicCode>>(viewModel.SerializedNewRecords);
            
            viewModel.ParseAndValidateParameters(Request, m => m.Reason);
            if (viewModel.HasAnyErrors())
            {
                var uploadCheckViewModel = GenerateAdminSicCodeUploadCheckViewModel(newRecords);
                return View("SicCodeUploadCheck", uploadCheckViewModel);
            }
            
            List<SicCode> existingRecords = dataRepository.GetAll<SicCode>().ToList();
            existingRecords.ForEach(record => dataRepository.Delete(record));
            newRecords.ForEach(record => dataRepository.Insert(record));
            dataRepository.SaveChangesAsync().Wait();
            
            User currentUser = ControllerHelper.GetGpgUserFromAspNetUser(User, dataRepository);
            auditLogger.AuditGeneralAction(AuditedAction.AdminUpdatedSicCodes, new
            {
                ExistingRecords = JsonConvert.SerializeObject(existingRecords),
                NewRecords = JsonConvert.SerializeObject(newRecords),
                Reason = viewModel.Reason
            }, currentUser);
            
            return View("UploadConfirmation", viewModel.FileUploadType);
        }

        private AdminSicSectionUploadCheckViewModel GenerateAdminSicSectionUploadCheckViewModel(List<SicSection> newRecords)
        {
            List<SicSection> existingRecords = dataRepository.GetAll<SicSection>().ToList();

            var uploadCheckViewModel = new AdminSicSectionUploadCheckViewModel
            {
                RecordsToCreate = new List<SicSection>(),
                RecordsToUpdate = new List<SicSectionToUpdate>(),
                RecordsToDelete = new List<SicSection>(),
                SerializedNewRecords = JsonConvert.SerializeObject(newRecords),
                AbleToProceed = true
                
            };

            foreach (SicSection newRecord in newRecords)
            {
                if (existingRecords.Select(r => r.SicSectionId).Contains(newRecord.SicSectionId))
                {
                    SicSection matchingExistingRecord =
                        existingRecords.FirstOrDefault(r => r.SicSectionId == newRecord.SicSectionId);
                    if (matchingExistingRecord.Description.Trim() != newRecord.Description)
                    {
                        var recordToUpdate = new SicSectionToUpdate
                        {
                            SicSectionId = newRecord.SicSectionId,
                            PreviousDescription = matchingExistingRecord.Description,
                            NewDescription = newRecord.Description
                        };
                        uploadCheckViewModel.RecordsToUpdate.Add(recordToUpdate);
                    }
                }
                else if (newRecord.SicSectionId != "")
                {
                    uploadCheckViewModel.RecordsToCreate.Add(newRecord);
                }
            }

            foreach (SicSection existingRecord in existingRecords)
            {
                if (!newRecords.Select(r => r.SicSectionId).Contains(existingRecord.SicSectionId))
                {
                    uploadCheckViewModel.RecordsToDelete.Add(existingRecord);
                    if (existingRecord.SicCodes.Count > 0)
                    {
                        uploadCheckViewModel.AbleToProceed = false;
                    }
                }
            }
            
            return uploadCheckViewModel;
        }
        
        private AdminSicCodeUploadCheckViewModel GenerateAdminSicCodeUploadCheckViewModel(List<SicCode> newRecords)
        {
            List<SicCode> existingRecords = dataRepository.GetAll<SicCode>().ToList();

            var uploadCheckViewModel = new AdminSicCodeUploadCheckViewModel
            {
                RecordsToCreate = new List<SicCode>(),
                RecordsToUpdate = new List<SicCodeToUpdate>(),
                RecordsToDelete = new List<SicCode>(),
                SerializedNewRecords = JsonConvert.SerializeObject(newRecords),
                AbleToProceed = true
                
            };

            foreach (SicCode newRecord in newRecords)
            {
                if (existingRecords.Select(r => r.SicCodeId).Contains(newRecord.SicCodeId))
                {
                    SicCode matchingExistingRecord =
                        existingRecords.FirstOrDefault(r => r.SicCodeId == newRecord.SicCodeId);
                    if (matchingExistingRecord.SicSectionId.Trim() != newRecord.SicSectionId
                        || matchingExistingRecord.Description.Trim() != newRecord.Description)
                    {
                        var recordToUpdate = new SicCodeToUpdate
                        {
                            SicCodeId = newRecord.SicCodeId,
                            PreviousSicSectionId = matchingExistingRecord.SicSectionId,
                            NewSicSectionId = newRecord.SicSectionId,
                            PreviousDescription = matchingExistingRecord.Description,
                            NewDescription = newRecord.Description
                        };
                        uploadCheckViewModel.RecordsToUpdate.Add(recordToUpdate);
                    }
                }
                else 
                {
                    uploadCheckViewModel.RecordsToCreate.Add(newRecord);
                }
            }

            foreach (SicCode existingRecord in existingRecords)
            {
                if (!newRecords.Select(r => r.SicCodeId).Contains(existingRecord.SicCodeId))
                {
                    uploadCheckViewModel.RecordsToDelete.Add(existingRecord);
                    if (existingRecord.OrganisationSicCodes.Count > 0)
                    {
                        uploadCheckViewModel.AbleToProceed = false;
                    }
                }
            }
            
            return uploadCheckViewModel;
        }
    }
}
