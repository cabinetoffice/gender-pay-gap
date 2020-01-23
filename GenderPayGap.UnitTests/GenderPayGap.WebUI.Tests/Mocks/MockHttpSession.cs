using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace GenderPayGap.WebUI.Tests.Mocks
{
    //public static partial class TestHelper {
    public class MockHttpSession : ISession
    {

        private readonly Dictionary<string, object> sessionStorage = new Dictionary<string, object>();

        public object this[string name]
        {
            get => sessionStorage[name];
            set => sessionStorage[name] = value;
        }

        string ISession.Id => Guid.NewGuid().ToString();

        bool ISession.IsAvailable => throw new NotImplementedException();

        IEnumerable<string> ISession.Keys => sessionStorage.Keys;

        void ISession.Clear()
        {
            sessionStorage.Clear();
        }

        Task ISession.CommitAsync(CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        Task ISession.LoadAsync(CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        void ISession.Remove(string key)
        {
            sessionStorage.Remove(key);
        }

        void ISession.Set(string key, byte[] value)
        {
            sessionStorage[key] = value;
        }

        bool ISession.TryGetValue(string key, out byte[] value)
        {
            object val = null;
            bool result = sessionStorage.TryGetValue(key, out val);
            if (val is byte[])
            {
                value = val as byte[];
            }
            else
            {
                var str = val as string;
                value = string.IsNullOrWhiteSpace(str) ? null : Encoding.UTF8.GetBytes(str);
            }

            return result;
        }

    }
}
