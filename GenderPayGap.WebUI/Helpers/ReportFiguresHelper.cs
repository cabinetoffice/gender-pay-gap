using System;
using System.Linq.Expressions;
using GenderPayGap.Core.Helpers;
using GenderPayGap.Database;
using GenderPayGap.Database.Models;
using GenderPayGap.WebUI.Models.Report;
using GovUkDesignSystem.Parsers;
using Microsoft.AspNetCore.Http;
using GovUkDesignSystem;

namespace GenderPayGap.WebUI.Helpers
{
    public static class ReportFiguresHelper
    {
        public static void SetFigures(ReportFiguresViewModel viewModel, DraftReturn draftReturn, Return submittedReturn)
        {
            if (draftReturn != null)
            {
                SetFiguresFromDraftReturn(viewModel, draftReturn);
            }
            else if (submittedReturn != null)
            {
                SetFiguresFromSubmittedReturn(viewModel, submittedReturn);
            }
        }
        
        public static void SaveFiguresToDraftReturn(ReportFiguresViewModel viewModel, DraftReturn draftReturn)
        {
           SaveBonusPayFiguresToDraftReturn(viewModel, draftReturn);
           SaveHourlyPayFiguresToDraftReturn(viewModel, draftReturn);
           SaveOptedOutOfReportingPayQuartersToDraftReturn(viewModel, draftReturn);
           SavePayQuartileFiguresToDraftReturn(viewModel, draftReturn);
        }
        
        public static void ValidateUserInput(ReportFiguresViewModel viewModel, HttpRequest request, int reportingYear)
        {
            ValidateBonusPayFigures(viewModel, request);
            ValidateHourlyPayFigures(viewModel, request);
            ValidatePayQuartileFigures(viewModel, request);

            if (viewModel.OptedOutOfReportingPayQuarters)
            {
                if (!ReportingYearsHelper.IsReportingYearWithFurloughScheme(reportingYear))
                {
                    const string errorMessage = "You cannot opt out of reporting your pay quarter figures for this reporting year";
                    viewModel.AddErrorFor(m => m.OptedOutOfReportingPayQuarters, errorMessage);
                }

                if (HasPayQuarterFigures(viewModel))
                {
                    const string errorMessage = "Do not enter the data for the percentage of men and women in each hourly pay quarter "
                                                + "if you have opted out of reporting your pay quarter figures";
                    viewModel.AddErrorFor(m => m.OptedOutOfReportingPayQuarters, errorMessage);
                }
            }
        }

        private static bool HasPayQuarterFigures(ReportFiguresViewModel viewModel)
        {
            return viewModel.MaleLowerPayBand.HasValue
                   || viewModel.FemaleLowerPayBand.HasValue
                   || viewModel.MaleLowerMiddlePayBand.HasValue
                   || viewModel.FemaleLowerMiddlePayBand.HasValue
                   || viewModel.MaleUpperPayBand.HasValue
                   || viewModel.FemaleUpperPayBand.HasValue
                   || viewModel.MaleUpperMiddlePayBand.HasValue
                   || viewModel.FemaleUpperMiddlePayBand.HasValue;
        }

        private static void SetFiguresFromDraftReturn(ReportFiguresViewModel viewModel, DraftReturn draftReturn) 
        {
            SetHourlyPayQuarterFiguresFromDraftReturn(viewModel, draftReturn);
            SetMeanAndMedianGenderPayGapUsingHourlyPayFiguresFromDraftReturn(viewModel, draftReturn);
            SetBonusPayFiguresFromDraftReturn(viewModel, draftReturn);
            SetOptedOutOfReportingPayQuarterFromDraftReturn(viewModel, draftReturn);
        }

        private static void SetFiguresFromSubmittedReturn(ReportFiguresViewModel viewModel, Return submittedReturn)
        {
            SetHourlyPayQuarterFiguresFromSubmittedReturn(viewModel, submittedReturn);
            SetMeanAndMedianGenderPayGapUsingHourlyPayFiguresFromSubmittedReturn(viewModel, submittedReturn);
            SetBonusPayFiguresFromSubmittedReturn(viewModel, submittedReturn);
            SetOptedOutOfReportingPayQuarterFromSubmittedReturn(viewModel, submittedReturn);
        }
         
        private static void SetHourlyPayQuarterFiguresFromDraftReturn(ReportFiguresViewModel viewModel, DraftReturn draftReturn)
        {
            viewModel.MaleUpperPayBand = draftReturn.MaleUpperQuartilePayBand;
            viewModel.FemaleUpperPayBand = draftReturn.FemaleUpperQuartilePayBand;
            viewModel.MaleUpperMiddlePayBand = draftReturn.MaleUpperPayBand;
            viewModel.FemaleUpperMiddlePayBand = draftReturn.FemaleUpperPayBand;
            viewModel.MaleLowerMiddlePayBand = draftReturn.MaleMiddlePayBand;
            viewModel.FemaleLowerMiddlePayBand = draftReturn.FemaleMiddlePayBand;
            viewModel.MaleLowerPayBand = draftReturn.MaleLowerPayBand;
            viewModel.FemaleLowerPayBand = draftReturn.FemaleLowerPayBand;
        }

        private static void SetHourlyPayQuarterFiguresFromSubmittedReturn(ReportFiguresViewModel viewModel, Return submittedReturn)
        {
            viewModel.MaleUpperPayBand = submittedReturn.MaleUpperQuartilePayBand;
            viewModel.FemaleUpperPayBand = submittedReturn.FemaleUpperQuartilePayBand;
            viewModel.MaleUpperMiddlePayBand = submittedReturn.MaleUpperPayBand;
            viewModel.FemaleUpperMiddlePayBand = submittedReturn.FemaleUpperPayBand;
            viewModel.MaleLowerMiddlePayBand = submittedReturn.MaleMiddlePayBand;
            viewModel.FemaleLowerMiddlePayBand = submittedReturn.FemaleMiddlePayBand;
            viewModel.MaleLowerPayBand = submittedReturn.MaleLowerPayBand;
            viewModel.FemaleLowerPayBand = submittedReturn.FemaleLowerPayBand;
        }

        private static void SetMeanAndMedianGenderPayGapUsingHourlyPayFiguresFromDraftReturn(ReportFiguresViewModel viewModel, DraftReturn draftReturn)
        {
            viewModel.DiffMeanHourlyPayPercent = draftReturn.DiffMeanHourlyPayPercent;
            viewModel.DiffMedianHourlyPercent = draftReturn.DiffMedianHourlyPercent;
        }

        private static void SetMeanAndMedianGenderPayGapUsingHourlyPayFiguresFromSubmittedReturn(ReportFiguresViewModel viewModel, Return submittedReturn)
        {
            viewModel.DiffMeanHourlyPayPercent = submittedReturn.DiffMeanHourlyPayPercent;
            viewModel.DiffMedianHourlyPercent = submittedReturn.DiffMedianHourlyPercent;
        }

        private static void SetBonusPayFiguresFromDraftReturn(ReportFiguresViewModel viewModel, DraftReturn draftReturn)
        {
            viewModel.FemaleBonusPayPercent = draftReturn.FemaleMedianBonusPayPercent;
            viewModel.MaleBonusPayPercent = draftReturn.MaleMedianBonusPayPercent;
            viewModel.DiffMeanBonusPercent = draftReturn.DiffMeanBonusPercent;
            viewModel.DiffMedianBonusPercent = draftReturn.DiffMedianBonusPercent;
        }

        private static void SetBonusPayFiguresFromSubmittedReturn(ReportFiguresViewModel viewModel, Return submittedReturn)
        {
            viewModel.FemaleBonusPayPercent = submittedReturn.FemaleMedianBonusPayPercent;
            viewModel.MaleBonusPayPercent = submittedReturn.MaleMedianBonusPayPercent;
            viewModel.DiffMeanBonusPercent = submittedReturn.DiffMeanBonusPercent;
            viewModel.DiffMedianBonusPercent = submittedReturn.DiffMedianBonusPercent;
        }
        
        private static void SetOptedOutOfReportingPayQuarterFromDraftReturn(ReportFiguresViewModel viewModel, DraftReturn draftReturn)
        {
            viewModel.OptedOutOfReportingPayQuarters = draftReturn.OptedOutOfReportingPayQuarters;
        }
        
        private static void SetOptedOutOfReportingPayQuarterFromSubmittedReturn(ReportFiguresViewModel viewModel, Return submittedReturn)
        {
            viewModel.OptedOutOfReportingPayQuarters = submittedReturn.OptedOutOfReportingPayQuarters;
        }
        
        private static void SavePayQuartileFiguresToDraftReturn(ReportFiguresViewModel viewModel, DraftReturn draftReturn)
        {
            draftReturn.MaleUpperQuartilePayBand = viewModel.MaleUpperPayBand;
            draftReturn.FemaleUpperQuartilePayBand = viewModel.FemaleUpperPayBand;
            draftReturn.MaleUpperPayBand = viewModel.MaleUpperMiddlePayBand;
            draftReturn.FemaleUpperPayBand = viewModel.FemaleUpperMiddlePayBand;
            draftReturn.MaleMiddlePayBand = viewModel.MaleLowerMiddlePayBand;
            draftReturn.FemaleMiddlePayBand = viewModel.FemaleLowerMiddlePayBand;
            draftReturn.MaleLowerPayBand = viewModel.MaleLowerPayBand;
            draftReturn.FemaleLowerPayBand = viewModel.FemaleLowerPayBand;
        }
        
        private static void SaveBonusPayFiguresToDraftReturn(ReportFiguresViewModel viewModel, DraftReturn draftReturn)
        {
            draftReturn.FemaleMedianBonusPayPercent = viewModel.FemaleBonusPayPercent;
            draftReturn.MaleMedianBonusPayPercent = viewModel.MaleBonusPayPercent;
            draftReturn.DiffMeanBonusPercent = viewModel.DiffMeanBonusPercent;
            draftReturn.DiffMedianBonusPercent = viewModel.DiffMedianBonusPercent;
        }

        private static void SaveHourlyPayFiguresToDraftReturn(ReportFiguresViewModel viewModel, DraftReturn draftReturn)
        {
            draftReturn.DiffMeanHourlyPayPercent = viewModel.DiffMeanHourlyPayPercent;
            draftReturn.DiffMedianHourlyPercent = viewModel.DiffMedianHourlyPercent;
        }

        private static void SaveOptedOutOfReportingPayQuartersToDraftReturn(ReportFiguresViewModel viewModel, DraftReturn draftReturn)
        {
            draftReturn.OptedOutOfReportingPayQuarters = viewModel.OptedOutOfReportingPayQuarters;
        }
        
        private static void ValidateBonusPayFigures(ReportFiguresViewModel viewModel, HttpRequest request)
        {
            ParseAndValidateParameters(viewModel, request, m => m.FemaleBonusPayPercent);
            ParseAndValidateParameters(viewModel, request, m => m.MaleBonusPayPercent);
            ParseAndValidateParameters(viewModel, request, m => m.DiffMeanBonusPercent);
            ParseAndValidateParameters(viewModel, request, m => m.DiffMedianBonusPercent);
            
            ValidateBonusPayIntegrity(viewModel);
        }

        private static void ValidateBonusPayIntegrity(ReportFiguresViewModel viewModel)
        {
            const string errorMessageFemaleBonusGreaterThanZero = "Where the % of women receiving a bonus is > 0 AND men also received a bonus greater than 0, "
                                        + "then the mean or median bonus difference must be less than 100%";
            
            // ensure that bonus differences do not exceed 100% when females have a bonus 
            if (viewModel.FemaleBonusPayPercent > 0) 
            { 
                if (viewModel.DiffMeanBonusPercent > 100) 
                {
                    viewModel.AddErrorFor(m => m.DiffMeanBonusPercent, errorMessageFemaleBonusGreaterThanZero);
                } 
 
                if (viewModel.DiffMedianBonusPercent > 100)
                {
                    viewModel.AddErrorFor(m => m.DiffMedianBonusPercent, errorMessageFemaleBonusGreaterThanZero);
                } 
            }

            const string errorMessageMaleBonusIsZero = "Do not enter a bonus difference if 0% of men received a bonus";
            
            // prevents entering a difference when male bonus percent is 0 
            if (viewModel.MaleBonusPayPercent == 0) 
            { 
                if (viewModel.DiffMeanBonusPercent.HasValue) 
                {
                    viewModel.AddErrorFor(m => m.DiffMeanBonusPercent, errorMessageMaleBonusIsZero);
                } 
 
                if (viewModel.DiffMedianBonusPercent.HasValue) 
                {
                    viewModel.AddErrorFor(m => m.DiffMedianBonusPercent, errorMessageMaleBonusIsZero);
                } 
            }

            const string errorMessageMaleBonusGreaterThanZero = "Enter a percentage lower than or equal to 100";
             
            if (viewModel.MaleBonusPayPercent > 0)
            { 
                if (!viewModel.DiffMeanBonusPercent.HasValue) 
                {
                    viewModel.AddErrorFor(m => m.DiffMeanBonusPercent, errorMessageMaleBonusGreaterThanZero);
                } 
 
                if (!viewModel.DiffMedianBonusPercent.HasValue) 
                {
                    viewModel.AddErrorFor(m => m.DiffMedianBonusPercent, errorMessageMaleBonusGreaterThanZero);
                } 
            }
        }

        private static void ValidatePayQuartileFigures(ReportFiguresViewModel viewModel, HttpRequest request)
        {
            ParseAndValidateParameters(viewModel, request, m => m.MaleUpperPayBand);
            ParseAndValidateParameters(viewModel, request, m => m.FemaleUpperPayBand);
            ParseAndValidateParameters(viewModel, request, m => m.MaleUpperMiddlePayBand);
            ParseAndValidateParameters(viewModel, request, m => m.FemaleUpperMiddlePayBand);
            ParseAndValidateParameters(viewModel, request, m => m.MaleLowerMiddlePayBand);
            ParseAndValidateParameters(viewModel, request, m => m.FemaleLowerMiddlePayBand);
            ParseAndValidateParameters(viewModel, request, m => m.MaleLowerPayBand);
            ParseAndValidateParameters(viewModel, request, m => m.FemaleLowerPayBand);

            ValidatePayQuartersAddUpToOneHundred(viewModel);
        }

        private static void ValidatePayQuartersAddUpToOneHundred(ReportFiguresViewModel viewModel)
        {
            // Validate percents add up to 100
            string errorMessage = "Figures for each quarter must add up to 100%";

            if (viewModel.FemaleUpperPayBand.HasValue
                && viewModel.MaleUpperPayBand.HasValue
                && viewModel.FemaleUpperPayBand.Value + viewModel.MaleUpperPayBand.Value != 100)
            {
                viewModel.AddErrorFor(m => m.FemaleUpperPayBand, errorMessage);
                viewModel.AddErrorFor(m => m.MaleUpperPayBand, errorMessage);
            }

            if (viewModel.FemaleUpperMiddlePayBand.HasValue
                && viewModel.MaleUpperMiddlePayBand.HasValue
                && viewModel.FemaleUpperMiddlePayBand.Value + viewModel.MaleUpperMiddlePayBand.Value != 100)
            {
                viewModel.AddErrorFor(m => m.FemaleUpperMiddlePayBand, errorMessage);
                viewModel.AddErrorFor(m => m.MaleUpperMiddlePayBand, errorMessage);
            }

            if (viewModel.FemaleLowerMiddlePayBand.HasValue
                && viewModel.MaleLowerMiddlePayBand.HasValue
                && viewModel.FemaleLowerMiddlePayBand.Value + viewModel.MaleLowerMiddlePayBand.Value != 100)
            {
                viewModel.AddErrorFor(m => m.FemaleLowerMiddlePayBand, errorMessage);
                viewModel.AddErrorFor(m => m.MaleLowerMiddlePayBand, errorMessage);
            }

            if (viewModel.FemaleLowerPayBand.HasValue
                && viewModel.MaleLowerPayBand.HasValue
                && viewModel.FemaleLowerPayBand.Value + viewModel.MaleLowerPayBand.Value != 100)
            {
                viewModel.AddErrorFor(m => m.FemaleLowerPayBand, errorMessage);
                viewModel.AddErrorFor(m => m.MaleLowerPayBand, errorMessage);
            }
        }

        private static void ValidateHourlyPayFigures(ReportFiguresViewModel viewModel, HttpRequest request)
        {
            ParseAndValidateParameters(viewModel, request, m => m.DiffMeanHourlyPayPercent);
            ParseAndValidateParameters(viewModel, request, m => m.DiffMedianHourlyPercent);
        }

        private static void ParseAndValidateParameters<TModel, TProperty>(
            TModel viewModel, 
            HttpRequest request, 
            params Expression<Func<TModel, TProperty>>[] propertyLambdaExpressions)
            where TModel : GovUkViewModel
        {
            if (!viewModel.HasAnyErrors())
            {
                viewModel.ParseAndValidateParameters(request, propertyLambdaExpressions);
            }
        }
    }
}
