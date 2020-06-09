using Microsoft.Extensions.DependencyInjection;

namespace GenderPayGap.IdentityServer4.Classes
{
    public static class CustomIdentityServerBuilderExtensions
    {

        public static IIdentityServerBuilder AddCustomUserStore(this IIdentityServerBuilder builder)
        {
            builder.AddProfileService<CustomProfileService>();
            builder.AddResourceOwnerValidator<CustomResourceOwnerPasswordValidator>();

            return builder;
        }

    }
}
