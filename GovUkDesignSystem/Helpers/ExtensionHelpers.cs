using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.Extensions.Primitives;

namespace GovUkDesignSystem.Helpers
{
    public static class ExtensionHelpers
    {

        public static TAttributeType GetSingleCustomAttribute<TAttributeType>(this MemberInfo property)
            where TAttributeType : Attribute
        {
            return property.GetCustomAttributes(typeof(TAttributeType)).SingleOrDefault() as TAttributeType;
        }

        public static string GetCurrentValue<TModel, TProperty>(
            TModel model,
            PropertyInfo property,
            Expression<Func<TModel, TProperty>> propertyLambdaExpression)
            where TModel : GovUkViewModel
        {
            var displayFormatAttribute = property.GetSingleCustomAttribute<DisplayFormatAttribute>();
            TProperty propertyValue = ExpressionHelpers.GetPropertyValueFromModelAndExpression(model, propertyLambdaExpression);
            
            string formattedPropertyValue = displayFormatAttribute?.ApplyFormatInEditMode == true && displayFormatAttribute?.DataFormatString != null
                ? string.Format(displayFormatAttribute.DataFormatString, propertyValue)
                : propertyValue.ToString();
            
            if (model.HasSuccessfullyParsedValue(property))
            {
                return formattedPropertyValue;
            }

            if (propertyValue != null)
            {
                return formattedPropertyValue;
            }

            string parameterName = $"GovUk_Text_{property.Name}";
            StringValues unparsedValues = model.GetUnparsedValues(parameterName);

            string unparsedValueOrNull = unparsedValues.Count > 0 ? unparsedValues[0] : null;
            return unparsedValueOrNull;
        }

    }
}
