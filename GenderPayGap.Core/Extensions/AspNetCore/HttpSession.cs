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

        string SessionID { get; }
        object this[string key] { get; set; }
        IEnumerable<string> Keys { get; }
        void Add(string key, object value);
        void Remove(string key);
        T Get<T>(string key);
        Task LoadAsync();
        Task SaveAsync();

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

        public async Task LoadAsync()
        {
            //Load data from distributed data store asynchronously
            if (!_httpContextAccessor.HttpContext.Session.IsAvailable)
            {
                await _httpContextAccessor.HttpContext.Session.LoadAsync().ConfigureAwait(false);
                Dirty = false;
            }
        }

        public async Task SaveAsync()
        {
            if (Dirty)
            {
                await _httpContextAccessor.HttpContext.Session.CommitAsync().ConfigureAwait(false);
            }

            Dirty = false;
        }

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

        public IEnumerable<string> Keys => _httpContextAccessor.HttpContext.Session.Keys;

        public T Get<T>(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentNullException(nameof(key));
            }

            //Get value from session
            byte[] bytes = _httpContextAccessor.HttpContext.Session.Get(key);
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

        public void Add(string key, object value)
        {
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
                    byte[] bytes = Encoding.UTF8.GetBytes(str);
                    _httpContextAccessor.HttpContext.Session.Set(key, bytes);
                    Dirty = true;
                    return;
                }
            }

            _httpContextAccessor.HttpContext.Session.SetString(key, str);
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
