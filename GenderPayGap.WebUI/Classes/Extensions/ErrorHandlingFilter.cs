using GenderPayGap.Core.Models;
using GenderPayGap.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Logging;

namespace GenderPayGap.WebUI.Classes
{
    public class ErrorHandlingFilter : ExceptionFilterAttribute
    {

        private readonly ILogger _logger;

        public ErrorHandlingFilter(ILogger<ErrorHandlingFilter> logger)
        {
            _logger = logger;
        }

        public override void OnException(ExceptionContext context)
        {
            var hex = context.Exception as HttpException;
            if (hex != null)
            {
                _logger.LogWarning(hex, hex.Message);
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
