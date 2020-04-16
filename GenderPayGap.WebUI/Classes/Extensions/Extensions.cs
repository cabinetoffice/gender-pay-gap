using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading.Tasks;
using GenderPayGap.Core;
using GenderPayGap.Core.Classes.ErrorMessages;
using GenderPayGap.Core.Interfaces;
using GenderPayGap.Database;
using GenderPayGap.Extensions;
using GenderPayGap.WebUI.Services;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;

namespace GenderPayGap.WebUI.Classes
{
    public static partial class Extensions
    {

        public static void AddStringTrimmingProvider(this MvcOptions option)
        {
            IModelBinderProvider binderToFind =
                option.ModelBinderProviders.FirstOrDefault(x => x.GetType() == typeof(SimpleTypeModelBinderProvider));
            if (binderToFind == null)
            {
                return;
            }

            int index = option.ModelBinderProviders.IndexOf(binderToFind);
            option.ModelBinderProviders.Insert(index, new TrimmingModelBinderProvider());
        }

        #region AntiSpam

        public static IHtmlContent SpamProtectionTimeStamp(this IHtmlHelper helper)
        {
            var builder = new TagBuilder("input");

            builder.MergeAttribute("id", "SpamProtectionTimeStamp");
            builder.MergeAttribute("name", "SpamProtectionTimeStamp");
            builder.MergeAttribute("type", "hidden");
            builder.MergeAttribute("value", Encryption.EncryptData(VirtualDateTime.Now.ToSmallDateTime()));
            return builder.RenderSelfClosingTag();
        }

        #endregion

        #region IPrinciple

        public static string GetClaim(this IPrincipal principal, string claimType)
        {
            if (principal == null || !principal.Identity.IsAuthenticated)
            {
                return null;
            }

            IEnumerable<Claim> claims = (principal as ClaimsPrincipal).Claims;

            //Use this to lookup the long UserID from the db - ignore the authProvider for now
            Claim claim = claims.FirstOrDefault(c => c.Type.ToLower() == claimType.ToLower());
            return claim == null ? null : claim.Value;
        }

        public static long GetUserId(this IPrincipal principal)
        {
            return principal.GetClaim("sub").ToLong();
        }

        #endregion

        #region User Entity

        public static bool IsSuperAdministrator(this User user)
        {
            if (!user.EmailAddress.IsEmailAddress())
            {
                throw new ArgumentException("Bad email address");
            }

            if (string.IsNullOrWhiteSpace(Global.SuperAdminEmails))
            {
                throw new ArgumentException("Missing SuperAdminEmails from web.config");
            }

            return user.EmailAddress.LikeAny(Global.SuperAdminEmails.SplitI(";"));
        }

        public static bool IsDatabaseAdministrator(this User user)
        {
            if (!user.EmailAddress.IsEmailAddress())
            {
                throw new ArgumentException("Bad email address");
            }

            if (string.IsNullOrWhiteSpace(Global.DatabaseAdminEmails))
            {
                return user.IsSuperAdministrator();
            }

            return user.EmailAddress.LikeAny(Global.DatabaseAdminEmails.SplitI(";"));
        }

        public static User FindUser(this IDataRepository repository, IPrincipal principal)
        {
            if (principal == null)
            {
                return null;
            }

            //GEt the logged in users identifier
            long userId = principal.GetUserId();

            //If internal user the load it using the identifier as the UserID
            if (userId > 0)
            {
                return repository.Get<User>(userId);
            }

            return null;
        }

        #endregion
        
        #region Session Handling

        public static void StashModel<T>(this BaseController controller, T model)
        {
            controller.Session[controller + ":Model"] = Json.SerializeObjectDisposed(model);
        }

        public static void StashModel<KeyT, ModelT>(this BaseController controller, KeyT keyController, ModelT model)
        {
            controller.Session[keyController + ":Model"] = Json.SerializeObjectDisposed(model);
        }

        public static void ClearStash(this BaseController controller)
        {
            controller.Session.Remove(controller + ":Model");
        }

        public static void ClearAllStashes(this BaseController controller)
        {
            foreach (string key in controller.Session.Keys.ToList())
            {
                if (key.EndsWithI(":Model"))
                {
                    controller.Session.Remove(key);
                }
            }
        }

        public static T UnstashModel<T>(this BaseController controller, bool delete = false) where T : class
        {
            string json = controller.Session[controller + ":Model"].ToStringOrNull();
            T result = string.IsNullOrWhiteSpace(json) ? null : JsonConvert.DeserializeObject<T>(json);
            if (delete)
            {
                controller.Session.Remove(controller + ":Model");
            }

            return result;
        }

        public static T UnstashModel<T>(this BaseController controller, Type keyController, bool delete = false) where T : class
        {
            string json = controller.Session[controller + ":Model"].ToStringOrNull();
            T result = string.IsNullOrWhiteSpace(json) ? null : JsonConvert.DeserializeObject<T>(json);
            if (delete)
            {
                controller.Session.Remove(keyController + ":Model");
            }

            return result;
        }

        #endregion

        #region Encypt Decrypt

        public static bool DecryptToId(this string enc, out long decId)
        {
            decId = 0;
            if (string.IsNullOrWhiteSpace(enc))
            {
                return false;
            }

            long id = Encryption.DecryptQuerystring(enc).ToInt64();
            if (id <= 0)
            {
                return false;
            }

            decId = id;
            return true;
        }

        public static bool DecryptToParams(this string enc, out List<string> outParams)
        {
            outParams = null;
            if (string.IsNullOrWhiteSpace(enc))
            {
                return false;
            }

            string decParams = Encryption.DecryptData(enc.DecodeUrlBase64());
            if (string.IsNullOrWhiteSpace(decParams))
            {
                return false;
            }

            outParams = new List<string>(decParams.Split(':'));
            return true;
        }

        #endregion

        #region Helpers

        public static void CleanModelErrors<TModel>(this Controller controller)
        {
            Type containerType = typeof(TModel);
            //Save the old modelstate
            var oldModelState = new ModelStateDictionary();
            foreach (KeyValuePair<string, ModelStateEntry> modelState in controller.ModelState)
            {
                string propertyName = modelState.Key;
                foreach (ModelError error in modelState.Value.Errors)
                {
                    bool exists = oldModelState.Any(
                        m => m.Key == propertyName && m.Value.Errors.Any(e => e.ErrorMessage == error.ErrorMessage));

                    //add the inline message if it doesnt already exist
                    if (!exists)
                    {
                        oldModelState.AddModelError(propertyName, error.ErrorMessage);
                    }
                }
            }

            //Clear the model state ready for refill
            controller.ModelState.Clear();

            foreach (KeyValuePair<string, ModelStateEntry> modelState in oldModelState)
            {
                //Get the property name
                string propertyName = modelState.Key;

                //Get the validation attributes
                PropertyInfo propertyInfo = string.IsNullOrWhiteSpace(propertyName) ? null : containerType.GetPropertyInfo(propertyName);
                List<ValidationAttribute> attributes = propertyInfo == null
                    ? null
                    : propertyInfo.GetCustomAttributes(typeof(ValidationAttribute), false).ToList<ValidationAttribute>();

                //Get the display name
                var displayNameAttribute =
                    propertyInfo?.GetCustomAttributes(typeof(DisplayNameAttribute), false).FirstOrDefault() as DisplayNameAttribute;
                var displayAttribute =
                    propertyInfo?.GetCustomAttributes(typeof(DisplayAttribute), false).FirstOrDefault() as DisplayAttribute;
                string displayName = displayNameAttribute != null ? displayNameAttribute.DisplayName :
                    displayAttribute != null ? displayAttribute.Name : propertyName;

                foreach (ModelError error in modelState.Value.Errors)
                {
                    string title = string.IsNullOrWhiteSpace(propertyName) ? error.ErrorMessage : null;
                    string description = !string.IsNullOrWhiteSpace(propertyName) ? error.ErrorMessage : null;

                    if (error.ErrorMessage.Like("The value * is not valid for *."))
                    {
                        title = "There's a problem with your values.";
                        description = "The value here is invalid.";
                    }

                    if (attributes == null || !attributes.Any())
                    {
                        goto addModelError;
                    }

                    ValidationAttribute attribute = attributes.FirstOrDefault(a => a.FormatErrorMessage(displayName) == error.ErrorMessage);
                    if (attribute == null)
                    {
                        goto addModelError;
                    }

                    string validatorKey = $"{containerType.Name}.{propertyName}:{attribute.GetType().Name.TrimSuffix("Attribute")}";
                    CustomErrorMessage customError = CustomErrorMessages.GetValidationError(validatorKey);
                    if (customError == null)
                    {
                        goto addModelError;
                    }

                    title = attribute.FormatError(customError.Title, displayName);
                    description = attribute.FormatError(customError.Description, displayName);

                    addModelError:

                    //add the summary message if it doesnt already exist
                    if (!string.IsNullOrWhiteSpace(title)
                        && !controller.ModelState.Any(m => m.Key == "" && m.Value.Errors.Any(e => e.ErrorMessage == title)))
                    {
                        controller.ModelState.AddModelError("", title);
                    }

                    //add the inline message if it doesnt already exist
                    if (!string.IsNullOrWhiteSpace(description)
                        && !string.IsNullOrWhiteSpace(propertyName)
                        && !controller.ModelState.Any(
                            m => m.Key.EqualsI(propertyName) && m.Value.Errors.Any(e => e.ErrorMessage == description)))
                    {
                        controller.ModelState.AddModelError(propertyName, description);
                    }
                }
            }
        }

        public static string FormatError(this ValidationAttribute attribute, string error, string displayName)
        {
            if (string.IsNullOrWhiteSpace(error))
            {
                return error;
            }

            string par1 = null;
            string par2 = null;

            if (attribute is RangeAttribute)
            {
                par1 = ((RangeAttribute) attribute).Minimum.ToString();
                par2 = ((RangeAttribute) attribute).Maximum.ToString();
            }
            else if (attribute is MinLengthAttribute)
            {
                par1 = ((MinLengthAttribute) attribute).Length.ToString();
            }
            else if (attribute is MaxLengthAttribute)
            {
                par1 = ((MaxLengthAttribute) attribute).Length.ToString();
            }
            else if (attribute is StringLengthAttribute)
            {
                par1 = ((StringLengthAttribute) attribute).MinimumLength.ToString();
                par2 = ((StringLengthAttribute) attribute).MaximumLength.ToString();
            }

            return string.Format(error, displayName, par1, par2);
        }

        //Removes all but the specified properties from the model state
        public static void Include(this ModelStateDictionary modelState, params string[] properties)
        {
            foreach (string key in modelState.Keys.ToList())
            {
                if (string.IsNullOrWhiteSpace(key))
                {
                    continue;
                }

                if (properties.ContainsI(key))
                {
                    continue;
                }

                modelState.Remove(key);
            }
        }

        //Removes all the specified properties from the model state
        public static void Exclude(this ModelStateDictionary modelState, params string[] properties)
        {
            foreach (string key in modelState.Keys.ToList())
            {
                if (string.IsNullOrWhiteSpace(key))
                {
                    continue;
                }

                if (!properties.ContainsI(key))
                {
                    continue;
                }

                modelState.Remove(key);
            }
        }

        public static void AddModelError(this ModelStateDictionary modelState,
            int errorCode,
            string propertyName = null,
            object parameters = null)
        {
            //Try and get the custom error
            CustomErrorMessage customError = CustomErrorMessages.GetError(errorCode);
            if (customError == null)
            {
                throw new ArgumentException("errorCode", "Cannot find custom error message with this code");
            }

            //Add the error to the modelstate
            string title = customError.Title;
            string description = customError.Description;

            //Resolve the parameters
            if (parameters != null)
            {
                title = parameters.Resolve(title);
                description = parameters.Resolve(description);
            }

            //add the summary message if it doesnt already exist
            if (!string.IsNullOrWhiteSpace(title) && !modelState.Any(m => m.Key == "" && m.Value.Errors.Any(e => e.ErrorMessage == title)))
            {
                modelState.AddModelError("", title);
            }

            if (!string.IsNullOrWhiteSpace(description))
            {
                //If no property then add description as second line of summary
                if (string.IsNullOrWhiteSpace(propertyName))
                {
                    if (!string.IsNullOrWhiteSpace(title)
                        && !modelState.Any(m => m.Key == "" && m.Value.Errors.Any(e => e.ErrorMessage == title)))
                    {
                        modelState.AddModelError("", title);
                    }
                }

                //add the inline message if it doesnt already exist
                else if (!modelState.Any(m => m.Key.EqualsI(propertyName) && m.Value.Errors.Any(e => e.ErrorMessage == description)))
                {
                    modelState.AddModelError(propertyName, description);
                }
            }
        }

        #endregion

    }
}
