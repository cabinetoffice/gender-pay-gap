using System;
using System.Reflection;
using System.Web;
using GenderPayGap.Core.Classes.ErrorMessages;
using GenderPayGap.Extensions;

namespace GenderPayGap.Core.Models
{
    [Serializable]
    public class ErrorViewModel
    {

        public ErrorViewModel() { }

        public ErrorViewModel(int errorCode, object parameters = null)
        {
            ErrorCode = errorCode;
            CustomErrorMessage customErrorMessage = CustomErrorMessages.GetPageError(errorCode) ?? CustomErrorMessages.DefaultPageError;

            Title = customErrorMessage.Title;
            Subtitle = customErrorMessage.Subtitle;
            Description = customErrorMessage.Description;
            CallToAction = customErrorMessage.CallToAction;
            Uri uri = HttpContext.Current?.GetUri();
            ActionUrl = customErrorMessage.ActionUrl == "#" && uri != null ? uri.PathAndQuery : customErrorMessage.ActionUrl;
            ActionText = customErrorMessage.ActionText;

            //Assign any values to variables
            if (parameters != null)
            {
                foreach (PropertyInfo prop in parameters.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public))
                {
                    var value = prop.GetValue(parameters, null) as string;
                    if (string.IsNullOrWhiteSpace(prop.Name) || string.IsNullOrWhiteSpace(value))
                    {
                        continue;
                    }

                    Title = Title.ReplaceI("{" + prop.Name + "}", value);
                    Subtitle = Subtitle.ReplaceI("{" + prop.Name + "}", value);
                    Description = Description.ReplaceI("{" + prop.Name + "}", value);
                    CallToAction = CallToAction.ReplaceI("{" + prop.Name + "}", value);
                    ActionUrl = ActionUrl.ReplaceI("{" + prop.Name + "}", value);
                    ActionText = ActionText.ReplaceI("{" + prop.Name + "}", value);
                }
            }
        }

        public int ErrorCode { get; }
        public string Title { get; set; }
        public string Subtitle { get; set; }
        public string Description { get; set; }
        public string CallToAction { get; set; }
        public string ActionText { get; set; } = "Continue";
        public string ActionUrl { get; set; }

    }
}
