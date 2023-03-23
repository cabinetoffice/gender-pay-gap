using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace GenderPayGap.Extensions.AspNetCore
{
    public interface IHttpSession
    {

        object this[string key] { get; set; }
        void Remove(string key);

    }

    public class HttpSession : IHttpSession
    {

        private readonly IHttpContextAccessor _httpContextAccessor;
        private bool Dirty;

        public HttpSession(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        }

        public string SessionID => _httpContextAccessor.HttpContext.Session.Id;

        public object this[string key]
        {
            get => Get<string>(key);
            set
            {
                if (string.IsNullOrWhiteSpace(key))
                {
                    throw new ArgumentNullException(nameof(key));
                }

                if (value == null)
                {
                    Remove(key);
                }
                else
                {
                    Add(key, value);
                }
            }
        }

        private T Get<T>(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentNullException(nameof(key));
            }

            //Get value from session
            if (!_httpContextAccessor.HttpContext.Session.TryGetValue(key, out byte[] bytes))
            {
                return default;
            }

            if (bytes == null || bytes.Length == 0)
            {
                return default;
            }

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

        private void Add(string key, object value)
        {
            string str = null;
            if (value.GetType().IsSimpleType())
            {
                str = value.ToString();
            }
            else
            {
                str = JsonConvert.SerializeObject(value);
            }

            byte[] bytes = Encoding.UTF8.GetBytes(str);
            _httpContextAccessor.HttpContext.Session.Set(key, bytes);
            Dirty = true;
        }

        public void Remove(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentNullException(nameof(key));
            }

            _httpContextAccessor.HttpContext.Session.Remove(key);
            Dirty = true;
        }

    }
}
