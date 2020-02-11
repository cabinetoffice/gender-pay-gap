using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace GenderPayGap.WebUI.Controllers
{
    public class SubmissionPrototypeController : Controller
    {

        [HttpGet("my-organisations")]
        public IActionResult MyOrganisations()
        {
            return View("MyOrganisations");
        }

        [HttpGet("my-organisations/{id}")]
        public IActionResult Organisation(long id)
        {
            return View("Organisation");
        }

        [HttpGet("my-organisations/{id}/reporting-year-{year}")]
        public IActionResult OrganisationDetailsForYear(long id, int year)
        {
            ViewBag.Year = year;
            return View("DetailsForYear");
        }

        [HttpGet("my-organisations/{id}/reporting-year-{year}/declare-scope")]
        public IActionResult DeclareScopeForYear(long id, int year)
        {
            ViewBag.Year = year;
            return View("DeclareScopeForYear");
        }

        [HttpGet("my-organisations/{id}/reporting-year-{year}/scope-decision")]
        public IActionResult ScopeDecisionForYear(long id, int year)
        {
            ViewBag.Year = year;
            return View("ScopeDecisionForYear");
        }

        [HttpGet("my-organisations/{id}/reporting-year-{year}/view-report")]
        public IActionResult ViewReportForYear(long id, int year)
        {
            ViewBag.Year = year;
            return View("ViewReportForYear");
        }

        [HttpGet("my-organisations/{id}/reporting-year-{year}/edit-report/hourly-rate")]
        public IActionResult EditReportHourlyRateForYear(long id, int year)
        {
            ViewBag.Year = year;
            return View("EditReportHourlyRateForYear");
        }

        [HttpGet("my-organisations/{id}/reporting-year-{year}/edit-report/review-changes")]
        public IActionResult EditReportForYearReviewChanges(long id, int year)
        {
            ViewBag.Year = year;
            return View("EditReportForYearReviewChanges");
        }

    }
}
