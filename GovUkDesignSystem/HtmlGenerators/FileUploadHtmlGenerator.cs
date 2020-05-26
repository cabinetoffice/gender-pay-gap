using System;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using GovUkDesignSystem.GovUkDesignSystemComponents;
using GovUkDesignSystem.Helpers;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GovUkDesignSystem.HtmlGenerators
{
    internal static class FileUploadHtmlGenerator
    {

        internal static async Task<IHtmlContent> GenerateHtml<TModel, TProperty>(
            IHtmlHelper<TModel> htmlHelper,
            Expression<Func<TModel, TProperty>> propertyLambdaExpression,
            LabelViewModel labelOptions = null,
            HintViewModel hintOptions = null,
            FormGroupViewModel formGroupOptions = null,
            string classes = null)
            where TModel : GovUkViewModel
        {
            PropertyInfo property = ExpressionHelpers.GetPropertyFromExpression(propertyLambdaExpression);

            string propertyName = property.Name;

            TModel model = htmlHelper.ViewData.Model;

            string id = propertyName;
            if (labelOptions != null)
            {
                labelOptions.For = id;
            }

            var fileUploadViewModel = new FileUploadViewModel
            {
                Id = id,
                Name = propertyName,
                Label = labelOptions,
                Hint = hintOptions,
                FormGroup = formGroupOptions,
                Classes = classes
            };

            if (model.HasErrorFor(property))
            {
                fileUploadViewModel.ErrorMessage = new ErrorMessageViewModel {Text = model.GetErrorFor(property)};
            }

            return await htmlHelper.PartialAsync("/GovUkDesignSystemComponents/FileUpload.cshtml", fileUploadViewModel);
        }

    }
}
