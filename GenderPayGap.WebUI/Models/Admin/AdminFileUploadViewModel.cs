﻿using System.ComponentModel.DataAnnotations;
using GovUkDesignSystem;
using Microsoft.AspNetCore.Http;

namespace GenderPayGap.WebUI.Models.Admin
{
    public class AdminFileUploadViewModel : GovUkViewModel
    {

        public FileUploadType FileUploadType { get; set; }

        public IFormFile File { get; set; }

    }

    public enum FileUploadType
    {

        [Display(Name = "SIC section")]
        SicSection = 0,

        [Display(Name = "SIC code")]
        SicCode = 1

    }
}
