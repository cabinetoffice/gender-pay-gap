using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using GenderPayGap.Core.Classes.Logger;
using GenderPayGap.Core.Interfaces;
using GenderPayGap.Database;
using GenderPayGap.Database.Models;
using GenderPayGap.Extensions;
using GenderPayGap.WebUI.Classes;
using GovUkDesignSystem.Parsers;
using Microsoft.AspNetCore.Mvc;

namespace GenderPayGap.WebUI.Controllers.SendFeedback
{
    [Route("send-feedback")]
    public class FeedbackController : Controller
    {
        private readonly IDataRepository dataRepository;

        public FeedbackController(
            IDataRepository dataRepository)
        {
            this.dataRepository = dataRepository;
        }

        [HttpGet]
        public IActionResult SendFeedbackGet()
        {
            var model = new FeedbackViewModel();

            PrePopulateEmailAndPhoneNumberFromLoggedInUser(model);

            return View("SendFeedback", model);
        }

        private void PrePopulateEmailAndPhoneNumberFromLoggedInUser(FeedbackViewModel model)
        {
            if (User.Identity.IsAuthenticated)
            {
                User user = dataRepository.FindUser(User);

                model.EmailAddress =
                    !string.IsNullOrWhiteSpace(user.ContactEmailAddress)
                        ? user.ContactEmailAddress
                        : user.EmailAddress;

                model.PhoneNumber = user.ContactPhoneNumber;
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SendFeedbackPost(FeedbackViewModel viewModel)
        {
            viewModel.ParseAndValidateParameters(Request, m => m.HowEasyIsThisServiceToUse);
            viewModel.ParseAndValidateParameters(Request, m => m.HowDidYouHearAboutGpg);
            viewModel.ParseAndValidateParameters(Request, m => m.OtherSourceText);
            viewModel.ParseAndValidateParameters(Request, m => m.WhyVisitGpgSite);
            viewModel.ParseAndValidateParameters(Request, m => m.OtherReasonText);
            viewModel.ParseAndValidateParameters(Request, m => m.WhoAreYou);
            viewModel.ParseAndValidateParameters(Request, m => m.OtherPersonText);
            viewModel.ParseAndValidateParameters(Request, m => m.Details);

            if (viewModel.HasAnyErrors())
            {
                // If there are any errors, return the user back to the same page to correct the mistakes
                return View("SendFeedback", viewModel);
            }

            CustomLogger.Information("Feedback has been received", viewModel);

            Feedback feedbackDatabaseModel = ConvertFeedbackViewModelIntoFeedbackDatabaseModel(viewModel);

            dataRepository.Insert(feedbackDatabaseModel);
            dataRepository.SaveChanges();

            return View("FeedbackSent");
        }

        private Feedback ConvertFeedbackViewModelIntoFeedbackDatabaseModel(FeedbackViewModel feedbackViewModel)
        {
            return new Feedback {
                Difficulty = feedbackViewModel.HowEasyIsThisServiceToUse.HasValue
                    ? (DifficultyTypes)(int) feedbackViewModel.HowEasyIsThisServiceToUse.Value
                    : (DifficultyTypes?) null,
                
                NewsArticle = feedbackViewModel.HowDidYouHearAboutGpg?.Contains(HowDidYouHearAboutGpg.NewsArticle),
                SocialMedia = feedbackViewModel.HowDidYouHearAboutGpg?.Contains(HowDidYouHearAboutGpg.SocialMedia),
                CompanyIntranet = feedbackViewModel.HowDidYouHearAboutGpg?.Contains(HowDidYouHearAboutGpg.CompanyIntranet),
                EmployerUnion = feedbackViewModel.HowDidYouHearAboutGpg?.Contains(HowDidYouHearAboutGpg.EmployerUnion),
                InternetSearch = feedbackViewModel.HowDidYouHearAboutGpg?.Contains(HowDidYouHearAboutGpg.InternetSearch),
                Charity = feedbackViewModel.HowDidYouHearAboutGpg?.Contains(HowDidYouHearAboutGpg.SocialMedia),
                LobbyGroup = feedbackViewModel.HowDidYouHearAboutGpg?.Contains(HowDidYouHearAboutGpg.LobbyGroup),
                Report = feedbackViewModel.HowDidYouHearAboutGpg?.Contains(HowDidYouHearAboutGpg.Report),
                OtherSource = feedbackViewModel.HowDidYouHearAboutGpg?.Contains(HowDidYouHearAboutGpg.Other),
                OtherSourceText = feedbackViewModel.OtherSourceText,

                FindOutAboutGpg = feedbackViewModel.WhyVisitGpgSite?.Contains(WhyVisitGpgSite.FindOutAboutGpg),
                ReportOrganisationGpgData = feedbackViewModel.WhyVisitGpgSite?.Contains(WhyVisitGpgSite.ReportOrganisationGpgData),
                CloseOrganisationGpg = feedbackViewModel.WhyVisitGpgSite?.Contains(WhyVisitGpgSite.CloseOrganisationGpg),
                ViewSpecificOrganisationGpg = feedbackViewModel.WhyVisitGpgSite?.Contains(WhyVisitGpgSite.ViewSpecificOrganisationGpg),
                ActionsToCloseGpg = feedbackViewModel.WhyVisitGpgSite?.Contains(WhyVisitGpgSite.ActionsToCloseGpg),
                OtherReason = feedbackViewModel.WhyVisitGpgSite?.Contains(WhyVisitGpgSite.Other),
                OtherReasonText = feedbackViewModel.OtherReasonText,

                EmployeeInterestedInOrganisationData = feedbackViewModel.WhoAreYou?.Contains(WhoAreYou.EmployeeInterestedInOrganisationData),
                ManagerInvolvedInGpgReport = feedbackViewModel.WhoAreYou?.Contains(WhoAreYou.ManagerInvolvedInGpgReport),
                ResponsibleForReportingGpg = feedbackViewModel.WhoAreYou?.Contains(WhoAreYou.ResponsibleForReportingGpg),
                PersonInterestedInGeneralGpg = feedbackViewModel.WhoAreYou?.Contains(WhoAreYou.PersonInterestedInGeneralGpg),
                PersonInterestedInSpecificOrganisationGpg = feedbackViewModel.WhoAreYou?.Contains(WhoAreYou.PersonInterestedInSpecificOrganisationGpg),
                OtherPerson = feedbackViewModel.WhoAreYou?.Contains(WhoAreYou.Other),
                OtherPersonText = feedbackViewModel.OtherPersonText,

                Details = TruncateDetails(feedbackViewModel.Details),
                EmailAddress = feedbackViewModel.EmailAddress,
                PhoneNumber = feedbackViewModel.PhoneNumber,
                
                CreatedDate = VirtualDateTime.Now
            };
        }

        private string TruncateDetails(string details)
        {
            int? truncatingLength = typeof(Feedback)
                .GetProperty(nameof(Feedback.Details))
                ?.GetCustomAttributes<MaxLengthAttribute>()
                .FirstOrDefault()
                ?.Length;

            return !string.IsNullOrEmpty(details) && details.Length >= truncatingLength
                ? details.Substring(0, truncatingLength ?? 2000)
                : details;
        }

    }

}
