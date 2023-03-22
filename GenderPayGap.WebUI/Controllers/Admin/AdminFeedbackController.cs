using System.Collections.Generic;
using System.Linq;
using GenderPayGap.Core;
using GenderPayGap.Core.Interfaces;
using GenderPayGap.Database.Models;
using GenderPayGap.WebUI.ErrorHandling;
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
