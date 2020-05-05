using GenderPayGap.Core.Classes.Logger;
using GenderPayGap.Core.Models;
using GenderPayGap.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace GenderPayGap.WebUI.Classes
{
    public class ErrorHandlingFilter : ExceptionFilterAttribute
    {
        public override void OnException(ExceptionContext context)
        {
            var hex = context.Exception as HttpException;
            if (hex != null)
            {
                CustomLogger.Warning(hex.Message, hex);
                context.Result = new ViewResult {
                    ViewName = "CustomError",
                    ViewData = new ViewDataDictionary(new EmptyModelMetadataProvider(), context.ModelState) {
                        Model = new ErrorViewModel(hex.StatusCode) // set the model
                    }
                };

                context.ExceptionHandled = true;
            }
        }

    }
}
