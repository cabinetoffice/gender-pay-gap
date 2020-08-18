using GenderPayGap.Core.Classes.Logger;
using GenderPayGap.Core.Models;
using GenderPayGap.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace GenderPayGap.WebUI.ErrorHandling
{
    public class ErrorHandlingFilter : ExceptionFilterAttribute
    {
        public override void OnException(ExceptionContext context)
        {
            if (context.Exception is HttpException hex)
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
            else if (context.Exception is CustomErrorPageException exception)
            {
                context.Result = new ViewResult
                {
                    StatusCode = exception.StatusCode,
                    ViewName = exception.ViewName,
                    ViewData = new ViewDataDictionary(new EmptyModelMetadataProvider(), context.ModelState)
                    {
                        // For this type of custom error page, we use the exception itself as the model
                        Model = exception
                    }

                };
            }
        }

    }
}
