using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;
using GovUkDesignSystem.GovUkDesignSystemComponents;
using GovUkDesignSystem.Helpers;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GovUkDesignSystem.HtmlGenerators
{
    internal static class TextInputHtmlGenerator
    {

        internal static IHtmlContent GenerateHtml<TModel, TProperty>(
            IHtmlHelper<TModel> htmlHelper,
            Expression<Func<TModel, TProperty>> propertyLambdaExpression,
            LabelViewModel labelOptions = null,
            HintViewModel hintOptions = null,
            FormGroupViewModel formGroupOptions = null,
            string classes = null,
            TextInputAppendixViewModel textInputAppendix = null,
            string type = "text",
            bool? spellcheck = null,
            string autocomplete = null
        )
            where TModel : GovUkViewModel
        {
            PropertyInfo property = ExpressionHelpers.GetPropertyFromExpression(propertyLambdaExpression);

            string propertyName = property.Name;

            TModel model = htmlHelper.ViewData.Model;

            string currentValue = ExtensionHelpers.GetCurrentValue(model, property, propertyLambdaExpression);

            string id = $"GovUk_{propertyName}";
            if (labelOptions != null)
            {
                labelOptions.For = id;
            }

            var textInputViewModel = new TextInputViewModel
            {
                Name = $"GovUk_Text_{propertyName}",
                Id = id,
                Value = currentValue,
                Label = labelOptions,
                Hint = hintOptions,
                FormGroup = formGroupOptions,
                Classes = classes,
                TextInputAppendix = textInputAppendix,
                Type = type,
                Spellcheck = spellcheck?.ToString().ToLowerInvariant(),
                Attributes = new Dictionary<string, string> {{"autocomplete", autocomplete}}
            };

            if (model.HasErrorFor(property))
            {
                textInputViewModel.ErrorMessage = new ErrorMessageViewModel {Text = model.GetErrorFor(property)};
            }

            return htmlHelper.Partial("/GovUkDesignSystemComponents/TextInput.cshtml", textInputViewModel);
        }

    }
}
