using System;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace GenderPayGap.Extensions
{
    public static class Json
    {

        /// <summary>
        ///     Serializes objects ignoiring null, recursive and disposed objects
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string SerializeObjectDisposed(object value, bool ignoreNulls = false)
        {
            string json = JsonConvert.SerializeObject(
                value,
                new JsonSerializerSettings {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore, //Ignore recursive objects
                    NullValueHandling =
                        ignoreNulls ? NullValueHandling.Ignore : NullValueHandling.Include, //Ignore null objects (including disposed)
                    ContractResolver = new DisposedContractResolver() //Resolve data ignoring disposed exceptions
                });
            return json;
        }

    }

    public class DisposedContractResolver : DefaultContractResolver
    {

        protected override IValueProvider CreateMemberValueProvider(MemberInfo member)
        {
            IValueProvider provider = base.CreateMemberValueProvider(member);
            if (provider != null)
            {
                provider = new DisposedValueProvider(provider);
            }

            return provider;
        }

        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            JsonProperty property = base.CreateProperty(member, memberSerialization);

#warning member.ToString().ContainsI("LazyLoader") must be removed when upgrade from EntityFramework Core 2.1 to 2.2 
#warning member.DeclaringType.ToString().ContainsI("Castle.Proxies") must be removed when no longer serializing EntityFramework entities 
            /* The first part of the following line because v2.1 of EF.Core does not yet have attribute to prevent Serialization of 
             * The LazyLoader object iself which is added in Version 2.2 - this can be removed later when we upgrade to v2.2
             * The second part is to prevent serialization of virtual members  which have not yet been LazyLoaded otherwise 
             * the entire database graph is serialized due to the relations between objects and serialization gets stuck here.
             * We should never serialize entity objects to session since the data tier and presentation tier should be loosely coupled.
             * When we have removed such tight coupling we can remove this second part
             */
            if (member.ToString().ContainsI("LazyLoader") //Part 1 - prevent serialization of lazy loader
                || member.DeclaringType.ToString().ContainsI("Castle.Proxies")
            ) //Prevents serialization of lazyloaded members not yet loaded
            {
                property.Ignored = true;
            }

            return property;
        }

    }

    /// <summary>
    ///     Get and set values for a <see cref="MemberInfo" /> using dynamic methods for disconnected entity framework
    ///     entities.
    /// </summary>
    public class DisposedValueProvider : IValueProvider
    {

        private readonly IValueProvider Provider;

        public DisposedValueProvider(IValueProvider provider)
        {
            Provider = provider;
        }

        /// <summary>
        ///     Sets the value.
        /// </summary>
        /// <param name="target">The target to set the value on.</param>
        /// <param name="value">The value to set on the target.</param>
        public void SetValue(object target, object value)
        {
            try
            {
                Provider.SetValue(target, value);
            }
            catch (Exception ex)
            {
                if (ex.InnerException is ObjectDisposedException)
                {
                    return;
                }

                throw;
            }
        }

        /// <summary>
        ///     Gets the value.
        /// </summary>
        /// <param name="target">The target to get the value from.</param>
        /// <returns>The value.</returns>
        public object GetValue(object target)
        {
            try
            {
                return Provider.GetValue(target);
            }
            catch (JsonSerializationException ex)
            {
                if (ex.InnerException is ObjectDisposedException)
                {
                    return null;
                }

                throw;
            }
        }

    }
}
