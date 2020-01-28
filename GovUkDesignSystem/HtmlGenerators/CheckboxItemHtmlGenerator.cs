using System;
using System.Linq.Expressions;
using System.Reflection;
using GovUkDesignSystem.GovUkDesignSystemComponents;
using GovUkDesignSystem.Helpers;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace GovUkDesignSystem.HtmlGenerators
{
    public class CheckboxItemHtmlGenerator
    {

        public static IHtmlContent GenerateHtml<TModel>(
            IHtmlHelper<TModel> htmlHelper,
            Expression<Func<TModel, bool>> propertyLambdaExpression,
            LabelViewModel labelOptions = null,
            HintViewModel hintOptions = null,
            Conditional conditional = null,
            bool disabled = false)
        {
            PropertyInfo property = ExpressionHelpers.GetPropertyFromExpression(propertyLambdaExpression);
            string propertyName = property.Name;

            TModel model = htmlHelper.ViewData.Model;
            bool isChecked = ExpressionHelpers.GetPropertyValueFromModelAndExpression(model, propertyLambdaExpression);

            if (labelOptions != null)
            {
                labelOptions.For = propertyName;
            }

            var checkboxItemViewModel = new CheckboxItemViewModel
            {
                Id = propertyName,
                Name = propertyName,
                Value = true.ToString(),
                Label = labelOptions,
                Hint = hintOptions,
                Conditional = conditional,
                Disabled = disabled,
                Checked = isChecked
            };

            return htmlHelper.Partial("/GovUkDesignSystemComponents/CheckboxItem.cshtml", checkboxItemViewModel);
        }

    }
}
