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
            TProperty propertyValue = ExpressionHelpers.GetPropertyValueFromModelAndExpression(model, propertyLambdaExpression);

            if (model.HasSuccessfullyParsedValue(property) || propertyValue != null)
            {
                var displayFormatAttribute = property.GetSingleCustomAttribute<DisplayFormatAttribute>();
                return displayFormatAttribute?.ApplyFormatInEditMode == true && displayFormatAttribute?.DataFormatString != null
                    ? string.Format(displayFormatAttribute.DataFormatString, propertyValue)
                    : propertyValue.ToString();
            }

            string parameterName = $"GovUk_Text_{property.Name}";
            StringValues unparsedValues = model.GetUnparsedValues(parameterName);

            string unparsedValueOrNull = unparsedValues.Count > 0 ? unparsedValues[0] : null;
            return unparsedValueOrNull;
        }

    }
}
