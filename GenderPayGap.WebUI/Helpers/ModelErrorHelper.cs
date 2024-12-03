using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace GenderPayGap.WebUI.Helpers
{
    public static class ModelErrorHelper
    {

        public static string GetErrorMessage(ModelStateDictionary modelState, string propertyName)
        {
            modelState.TryGetValue(propertyName, out var modelStateEntry);
            return modelStateEntry?.Errors.Count > 0
                ? string.Join(", ", modelStateEntry.Errors.Select(e => e.ErrorMessage))
                : null;
        }
    }
}
