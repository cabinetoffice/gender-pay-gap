using System.Threading.Tasks;
using GenderPayGap.Core;
using GenderPayGap.Core.Models;
using GenderPayGap.Database;
using GenderPayGap.Extensions;
using GenderPayGap.WebUI.BusinessLogic.Models.Submit;
using GenderPayGap.WebUI.Classes;
using Microsoft.AspNetCore.Mvc;

namespace GenderPayGap.WebUI.Controllers.Submission
{
    public partial class SubmitController : BaseController
    {

        #region public methods

        [HttpGet("enter-calculations")]
        public async Task<IActionResult> EnterCalculations(string returnUrl = null)
        {
            #region Check user, then retrieve model from Session 

            IActionResult checkResult = CheckUserRegisteredOk(out User currentUser);
            if (checkResult != null)
            {
                return checkResult;
            }

            var stashedReturnViewModel = this.UnstashModel<ReturnViewModel>();

            #endregion

            if (ReportingOrganisationStartYear == 0)
            {
                ReportingOrganisationStartYear = submissionService.GetCurrentSnapshotDate().Year;
            }

            stashedReturnViewModel = await LoadReturnViewModelFromDBorFromDraftFileAsync(stashedReturnViewModel, currentUser.UserId);

            if (!stashedReturnViewModel.ReportInfo.Draft.IsUserAllowedAccess)
            {
                this.CleanModelErrors<ReturnViewModel>();
                return View("CustomError", new ErrorViewModel(3040));
            }

            stashedReturnViewModel.ReturnUrl = returnUrl;

            this.StashModel(stashedReturnViewModel);

            return View("EnterCalculations", stashedReturnViewModel);
        }

        [HttpPost("enter-calculations")]
        [PreventDuplicatePost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EnterCalculations(ReturnViewModel postedReturnViewModel, string returnUrl = null)
        {
            #region Check user, then retrieve model from Session

            IActionResult checkResult = CheckUserRegisteredOk(out User currentUser);
            if (checkResult != null)
            {
                return checkResult;
            }

            var stashedReturnViewModel = this.UnstashModel<ReturnViewModel>();

            #endregion

            if (stashedReturnViewModel == null)
            {
                return SessionExpiredView();
            }

            postedReturnViewModel.ReportInfo = stashedReturnViewModel.ReportInfo;

            ExcludeBlankFieldsFromModelState(postedReturnViewModel);

            ConfirmPayBandsAddUpToOneHundred(postedReturnViewModel);

            ValidateBonusIntegrity(postedReturnViewModel);

            #region Keep draft file locked to this user

            await submissionService.KeepDraftFileLockedToUserAsync(postedReturnViewModel, CurrentUser.UserId);

            if (!postedReturnViewModel.ReportInfo.Draft.HasDraftBeenModifiedDuringThisSession)
            {
                postedReturnViewModel.ReportInfo.Draft.HasDraftBeenModifiedDuringThisSession =
                    IsEnterCalculationsModified(postedReturnViewModel, stashedReturnViewModel);
            }

            if (!stashedReturnViewModel.ReportInfo.Draft.IsUserAllowedAccess)
            {
                this.CleanModelErrors<ReturnViewModel>();
                return View("CustomError", new ErrorViewModel(3040));
            }

            #endregion

            if (!ModelState.IsValid)
            {
                this.CleanModelErrors<ReturnViewModel>();
                return View("EnterCalculations", postedReturnViewModel);
            }

            this.StashModel(postedReturnViewModel);

            string actionUrl = returnUrl.EqualsI("CheckData") ? "CheckData" :
                postedReturnViewModel.SectorType == SectorTypes.Public ? "OrganisationSize" : "PersonResponsible";

            return RedirectToAction(actionUrl);
        }

        [HttpPost("cancel-enter-calculations")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelEnterCalculations(ReturnViewModel postedReturnViewModel)
        {
            postedReturnViewModel.OriginatingAction = "EnterCalculations";
            return await ManageDraftAsync(postedReturnViewModel, IsEnterCalculationsModified);
        }

        #endregion

        #region private methods

        private static bool HasThisEnterCalculationPropertyChanged(decimal? postedValue, decimal? stashedValue)
        {
            if ((postedValue == null) & (stashedValue == null))
            {
                return false;
            }

            if ((postedValue == null) | (stashedValue == null))
            {
                return true;
            }

            return postedValue.Value != stashedValue.Value;
        }

        private static bool IsEnterCalculationsModified(ReturnViewModel postedReturnViewModel, ReturnViewModel stashedReturnViewModel)
        {
            if (postedReturnViewModel == null || stashedReturnViewModel == null)
            {
                return false;
            }

            bool hasDiffMeanBonusPercentChanged = HasThisEnterCalculationPropertyChanged(
                postedReturnViewModel.DiffMeanBonusPercent,
                stashedReturnViewModel.DiffMeanBonusPercent);
            bool hasDiffMeanHourlyPayPercentChanged = HasThisEnterCalculationPropertyChanged(
                postedReturnViewModel.DiffMeanHourlyPayPercent,
                stashedReturnViewModel.DiffMeanHourlyPayPercent);
            bool hasDiffMedianBonusPercentChanged = HasThisEnterCalculationPropertyChanged(
                postedReturnViewModel.DiffMedianBonusPercent,
                stashedReturnViewModel.DiffMedianBonusPercent);
            bool hasDiffMedianHourlyPercentChanged = HasThisEnterCalculationPropertyChanged(
                postedReturnViewModel.DiffMedianHourlyPercent,
                stashedReturnViewModel.DiffMedianHourlyPercent);
            bool hasFemaleLowerPayBandChanged = HasThisEnterCalculationPropertyChanged(
                postedReturnViewModel.FemaleLowerPayBand,
                stashedReturnViewModel.FemaleLowerPayBand);
            bool hasFemaleMedianBonusPayPercentChanged = HasThisEnterCalculationPropertyChanged(
                postedReturnViewModel.FemaleMedianBonusPayPercent,
                stashedReturnViewModel.FemaleMedianBonusPayPercent);
            bool hasFemaleMiddlePayBandChanged = HasThisEnterCalculationPropertyChanged(
                postedReturnViewModel.FemaleMiddlePayBand,
                stashedReturnViewModel.FemaleMiddlePayBand);
            bool hasFemaleUpperPayBandChanged = HasThisEnterCalculationPropertyChanged(
                postedReturnViewModel.FemaleUpperPayBand,
                stashedReturnViewModel.FemaleUpperPayBand);
            bool hasFemaleUpperQuartilePayBandChanged = HasThisEnterCalculationPropertyChanged(
                postedReturnViewModel.FemaleUpperQuartilePayBand,
                stashedReturnViewModel.FemaleUpperQuartilePayBand);
            bool hasMaleLowerPayBandChanged = HasThisEnterCalculationPropertyChanged(
                postedReturnViewModel.MaleLowerPayBand,
                stashedReturnViewModel.MaleLowerPayBand);
            bool hasMaleMedianBonusPayPercentChanged = HasThisEnterCalculationPropertyChanged(
                postedReturnViewModel.MaleMedianBonusPayPercent,
                stashedReturnViewModel.MaleMedianBonusPayPercent);
            bool hasMaleMiddlePayBandChanged = HasThisEnterCalculationPropertyChanged(
                postedReturnViewModel.MaleMiddlePayBand,
                stashedReturnViewModel.MaleMiddlePayBand);
            bool hasMaleUpperPayBandChanged = HasThisEnterCalculationPropertyChanged(
                postedReturnViewModel.MaleUpperPayBand,
                stashedReturnViewModel.MaleUpperPayBand);
            bool hasMaleUpperQuartilePayBandChanged = HasThisEnterCalculationPropertyChanged(
                postedReturnViewModel.MaleUpperQuartilePayBand,
                stashedReturnViewModel.MaleUpperQuartilePayBand);

            return hasDiffMeanBonusPercentChanged
                   || hasDiffMeanHourlyPayPercentChanged
                   || hasDiffMedianBonusPercentChanged
                   || hasDiffMedianHourlyPercentChanged
                   || hasFemaleLowerPayBandChanged
                   || hasFemaleMedianBonusPayPercentChanged
                   || hasFemaleMiddlePayBandChanged
                   || hasFemaleUpperPayBandChanged
                   || hasFemaleUpperQuartilePayBandChanged
                   || hasMaleLowerPayBandChanged
                   || hasMaleMedianBonusPayPercentChanged
                   || hasMaleMiddlePayBandChanged
                   || hasMaleUpperPayBandChanged
                   || hasMaleUpperQuartilePayBandChanged;
        }

        private void ConfirmPayBandsAddUpToOneHundred(ReturnViewModel postedReturnViewModel)
        {
            if (!postedReturnViewModel.MaleUpperQuartilePayBand.IsNull() || !postedReturnViewModel.FemaleUpperQuartilePayBand.IsNull())
            {
                if (postedReturnViewModel.MaleUpperQuartilePayBand + postedReturnViewModel.FemaleUpperQuartilePayBand != 100)
                {
                    AddModelError(2052, nameof(postedReturnViewModel.FemaleUpperQuartilePayBand));
                }
            }

            if (!postedReturnViewModel.MaleUpperPayBand.IsNull() || !postedReturnViewModel.FemaleUpperPayBand.IsNull())
            {
                if (postedReturnViewModel.MaleUpperPayBand + postedReturnViewModel.FemaleUpperPayBand != 100)
                {
                    AddModelError(2052, nameof(postedReturnViewModel.FemaleUpperPayBand));
                }
            }

            if (!postedReturnViewModel.MaleMiddlePayBand.IsNull() || !postedReturnViewModel.FemaleMiddlePayBand.IsNull())
            {
                if (postedReturnViewModel.MaleMiddlePayBand + postedReturnViewModel.FemaleMiddlePayBand != 100)
                {
                    AddModelError(2052, nameof(postedReturnViewModel.FemaleMiddlePayBand));
                }
            }

            if (!postedReturnViewModel.MaleLowerPayBand.IsNull() || !postedReturnViewModel.FemaleLowerPayBand.IsNull())
            {
                if (postedReturnViewModel.MaleLowerPayBand + postedReturnViewModel.FemaleLowerPayBand != 100)
                {
                    AddModelError(2052, nameof(postedReturnViewModel.FemaleLowerPayBand));
                }
            }
        }

        private void ValidateBonusIntegrity(ReturnViewModel postedReturnViewModel)
        {
            // ensure that bonus differences do not exceed 100% when females have a bonus
            if (postedReturnViewModel.FemaleMedianBonusPayPercent > 0)
            {
                if (postedReturnViewModel.DiffMeanBonusPercent > 100)
                {
                    AddModelError(2130, nameof(postedReturnViewModel.DiffMeanBonusPercent));
                }

                if (postedReturnViewModel.DiffMedianBonusPercent > 100)
                {
                    AddModelError(2130, nameof(postedReturnViewModel.DiffMedianBonusPercent));
                }
            }

            // prevents entering a difference when male bonus percent is 0
            if (postedReturnViewModel.MaleMedianBonusPayPercent == 0)
            {
                if (postedReturnViewModel.DiffMeanBonusPercent.HasValue)
                {
                    AddModelError(2131, nameof(postedReturnViewModel.DiffMeanBonusPercent));
                }

                if (postedReturnViewModel.DiffMedianBonusPercent.HasValue)
                {
                    AddModelError(2131, nameof(postedReturnViewModel.DiffMedianBonusPercent));
                }
            }
        }

        #endregion

    }
}
