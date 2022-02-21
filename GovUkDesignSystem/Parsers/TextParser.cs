using System.Linq;
using System.Reflection;
using GovUkDesignSystem.Attributes.ValidationAttributes;
using GovUkDesignSystem.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

namespace GovUkDesignSystem.Parsers
{
    public class TextParser
    {

        public static void ParseAndValidate(
            GovUkViewModel model,
            PropertyInfo property,
            HttpRequest httpRequest)
        {
            string parameterName = $"GovUk_Text_{property.Name}";

            StringValues parameterValues = HttpRequestParameterHelper.GetRequestParameter(httpRequest, parameterName);

            ParserHelpers.ThrowIfMoreThanOneValue(parameterValues, property);
            ParserHelpers.SaveUnparsedValueFromRequestToModel(model, httpRequest, parameterName);

            if (ParserHelpers.IsValueRequiredAndMissingOrEmpty(property, parameterValues, model))
            {
                ParserHelpers.AddRequiredAndMissingErrorMessage(model, property);
                return;
            }

            if (parameterValues.Count > 0)
            {
                string parameterValue = parameterValues[0];
                
                var customAttributes = property
                    .GetCustomAttributes(typeof(GovUkValidationAttribute))
                    .Cast<GovUkValidationAttribute>();
                foreach (var attribute in customAttributes)
                {
                    var result = attribute.CheckForValidationErrors(model, property, parameterValue);
                    if (result == false)
                    {
                        return;
                    }
                }
                    
                property.SetValue(model, parameterValue.Trim());
            }

            model.ValueWasSuccessfullyParsed(property);
        }

    }
}
