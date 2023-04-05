using System;
using System.Collections.Generic;
using System.Linq;
using GenderPayGap.Core;
using GenderPayGap.Core.Interfaces;
using GenderPayGap.Database.Models;
using GenderPayGap.WebUI.Helpers;
using GenderPayGap.WebUI.Models.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GenderPayGap.WebUI.Controllers
{
    [Authorize(Roles = LoginRoles.GpgAdmin)]
    [Route("admin")]
    public class AdminFeedbackController : Controller
    {

        private readonly IDataRepository dataRepository;

        public AdminFeedbackController(IDataRepository dataRepository)
        {
            this.dataRepository = dataRepository;
        }

        [HttpGet("feedback")]
        public IActionResult ViewFeedback()
        {
            List<Feedback> feedback = dataRepository
                .GetAll<Feedback>()
                .Where(f => f.FeedbackStatus == FeedbackStatus.NotSpam)
                .OrderByDescending(f => f.CreatedDate)
                .ToList();

            return View("ViewFeedback", feedback);
        }

        [HttpGet("feedback/categorise-feedback/next")]
        public IActionResult CategoriseNextFeedback()
        {
            Feedback nextFeedback = dataRepository
                .GetAll<Feedback>()
                .Where(f => f.FeedbackStatus == FeedbackStatus.New)
                .OrderBy(f => f.FeedbackId)
                .FirstOrDefault();

            if (nextFeedback == null)
            {
                return RedirectToAction("ViewFeedback", "AdminFeedback");
            }
            
            return RedirectToAction("CategoriseFeedbackGet", "AdminFeedback", new {feedbackId = nextFeedback.FeedbackId});
        }

        [HttpGet("feedback/categorise-feedback/{feedbackId}")]
        public IActionResult CategoriseFeedbackGet(long feedbackId)
        {
            int numberOfNewFeedbacks = dataRepository
                .GetAll<Feedback>()
                .Where(f => f.FeedbackStatus == FeedbackStatus.New)
                .Count();

            Feedback nextFeedback = dataRepository.Get<Feedback>(feedbackId);

            var viewModel = new AdminFeedbackToCategoriseViewModel
            {
                NumberOfNewFeedbacks = numberOfNewFeedbacks,
                FeedbackToCategorise = nextFeedback
            };

            return View("CategoriseFeedback", viewModel);
        }

        [HttpPost("feedback/categorise-feedback/{feedbackId}")]
        [ValidateAntiForgeryToken]
        public IActionResult CategoriseFeedbackPost(long feedbackId, FeedbackStatus status)
        {
            Feedback feedback = dataRepository.Get<Feedback>(feedbackId);

            feedback.FeedbackStatus = status;

            dataRepository.SaveChanges();

            return RedirectToAction("CategoriseNextFeedback", "AdminFeedback");
        }

        [HttpGet("feedback/bulk-mark-feedback-as-spam")]
        public IActionResult BulkMarkFeedbackAsSpamGet()
        {
            var viewModel = new AdminBulkMarkFeedbackAsSpamViewModel();

            return View("BulkMarkFeedbackAsSpam", viewModel);
        }

        [HttpPost("feedback/bulk-mark-feedback-as-spam")]
        [ValidateAntiForgeryToken]
        public IActionResult BulkMarkFeedbackAsSpamPost(AdminBulkMarkFeedbackAsSpamViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                return View("BulkMarkFeedbackAsSpam", viewModel);
            }

            string[] feedbackIdsAsStrings = viewModel.FeedbackIdsToMarkAsSpam.Split('\n', StringSplitOptions.RemoveEmptyEntries);

            var feedbackIdsAsNumbers = new List<long>();
            var nonNumericFeedbackIds = new List<string>();
            foreach (string feedbackIdAsString in feedbackIdsAsStrings)
            {
                if (!string.IsNullOrWhiteSpace(feedbackIdAsString))
                {
                    if (long.TryParse(feedbackIdAsString.Trim(), out long feedbackId))
                    {
                        feedbackIdsAsNumbers.Add(feedbackId);
                    }
                    else
                    {
                        nonNumericFeedbackIds.Add(feedbackIdAsString);
                    }
                }
            }

            if (nonNumericFeedbackIds.Any())
            {
                string listOfInvalidFeedbackIds = string.Join(", ", nonNumericFeedbackIds);
                string message = $"Some feedback IDs are not numeric ({listOfInvalidFeedbackIds})";
                ModelState.AddModelError(nameof(viewModel.FeedbackIdsToMarkAsSpam), message);
                return View("BulkMarkFeedbackAsSpam", viewModel);
            }

            List<long> allFeedbackIds = dataRepository
                .GetAll<Feedback>()
                .Select(f => f.FeedbackId)
                .ToList();

            var invalidFeedbackIds = new List<long>();
            foreach (long feedbackId in feedbackIdsAsNumbers)
            {
                if (!allFeedbackIds.Contains(feedbackId))
                {
                    invalidFeedbackIds.Add(feedbackId);
                }
            }
            
            if (invalidFeedbackIds.Any())
            {
                string listOfInvalidFeedbackIds = string.Join(", ", invalidFeedbackIds);
                string message = $"Some IDs that you supplied are not IDs of real feedback messages ({listOfInvalidFeedbackIds})";
                ModelState.AddModelError(nameof(viewModel.FeedbackIdsToMarkAsSpam), message);
                return View("BulkMarkFeedbackAsSpam", viewModel);
            }

            List<Feedback> feedbacksToMarkAsSpam = dataRepository
                .GetAll<Feedback>()
                .Where(f => feedbackIdsAsNumbers.Contains(f.FeedbackId))
                .ToList();

            foreach (Feedback feedback in feedbacksToMarkAsSpam)
            {
                feedback.FeedbackStatus = FeedbackStatus.Spam;
            }
            
            dataRepository.SaveChanges();

            return RedirectToAction("CategoriseNextFeedback", "AdminFeedback");
        }

        [HttpGet("feedback/download")]
        public FileContentResult DownloadFeedback(bool nonSpamOnly = false)
        {
            var feedback = dataRepository.GetAll<Feedback>();
            if (nonSpamOnly)
            {
                feedback = feedback.Where(f => f.FeedbackStatus == FeedbackStatus.NotSpam);
            }

            return DownloadHelper.CreateCsvDownload(feedback, "Feedback.csv");
        }

    }
}
