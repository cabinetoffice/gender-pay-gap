using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;

namespace GenderPayGap.Extensions.AspNetCore
{
    public interface IHttpCache
    {

        Task<T> GetAsync<T>(string key);
        Task SetAsync<T>(string key, T value);
        Task AddAsync(string key, object value, DateTime? absoluteExpiration = null, TimeSpan? slidingExpiration = null);
        Task RemoveAsync(string key);

    }

    public class HttpCache : IHttpCache
    {

        private readonly IDistributedCache _Cache;

        public HttpCache(IDistributedCache cache)
        {
            _Cache = cache ?? throw new ArgumentNullException(nameof(cache));
        }

        public async Task SetAsync<T>(string key, T value)
        {
            if (value == null || value.Equals(default(T)))
            {
                await RemoveAsync(key);
            }
            else
            {
                await AddAsync(key, value);
            }
        }

        public async Task<T> GetAsync<T>(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentNullException(nameof(key));
            }

            byte[] bytes = await _Cache.GetAsync(key);
            if (bytes == null || bytes.Length == 0)
            {
                return default;
            }

            bytes = Encryption.Decompress(bytes);
            if (bytes == null || bytes.Length == 0)
            {
                return default;
            }

            string value = Encoding.UTF8.GetString(bytes);
            if (string.IsNullOrWhiteSpace(value))
            {
                return default;
            }

            if (typeof(T).IsSimpleType())
            {
                return (T) Convert.ChangeType(value, typeof(T));
            }

            return JsonConvert.DeserializeObject<T>(value);
        }

        public async Task AddAsync(string key, object value, DateTime? absoluteExpiration = null, TimeSpan? slidingExpiration = null)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentNullException(nameof(key));
            }

            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            var options = new DistributedCacheEntryOptions();
            if (slidingExpiration != null && slidingExpiration.Value != TimeSpan.Zero)
            {
                options.SetSlidingExpiration(slidingExpiration.Value);
            }

            if (absoluteExpiration != null && absoluteExpiration.Value != DateTime.MinValue)
            {
                options.SetAbsoluteExpiration(absoluteExpiration.Value);
            }

            string str = null;
            if (value.GetType().IsSimpleType())
            {
                str = value.ToString();
            }
            else
            {
                str = JsonConvert.SerializeObject(value);
                if (str.Length > 250)
                {
                    str = Encryption.Compress(str);
                }
            }

            await _Cache.SetStringAsync(key, str, options);
        }

        public async Task RemoveAsync(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentNullException(nameof(key));
            }

            await _Cache.RemoveAsync(key);
        }

    }
}
