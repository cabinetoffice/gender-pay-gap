using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace GenderPayGap.WebUI.Classes
{
    public class TrimModelBinder : IDisplayMetadataProvider
    {

        public void CreateDisplayMetadata(DisplayMetadataProviderContext context)
        {
            if (context.Key.MetadataKind == ModelMetadataKind.Property)
            {
                context.DisplayMetadata.ConvertEmptyStringToNull = true;
            }
        }

    }

    public class TrimmingModelBinder : IModelBinder
    {

        private readonly IModelBinder FallbackBinder;

        public TrimmingModelBinder(IModelBinder fallbackBinder)
        {
            FallbackBinder = fallbackBinder ?? throw new ArgumentNullException(nameof(fallbackBinder));
        }

        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            if (bindingContext == null)
            {
                throw new ArgumentNullException(nameof(bindingContext));
            }

            ValueProviderResult valueProviderResult = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);
            if (valueProviderResult != null && valueProviderResult.FirstValue is string str && !string.IsNullOrEmpty(str))
            {
                bindingContext.Result = ModelBindingResult.Success(str.Trim());
                return Task.CompletedTask;
            }

            return FallbackBinder.BindModelAsync(bindingContext);
        }

    }

    public class TrimmingModelBinderProvider : IModelBinderProvider
    {

        public IModelBinder GetBinder(ModelBinderProviderContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (!context.Metadata.IsComplexType && context.Metadata.ModelType == typeof(string))
            {
                var loggerFactory = context.Services.GetRequiredService<ILoggerFactory>();
                return new TrimmingModelBinder(new SimpleTypeModelBinder(context.Metadata.ModelType, loggerFactory));
            }

            return null;
        }

    }
}
