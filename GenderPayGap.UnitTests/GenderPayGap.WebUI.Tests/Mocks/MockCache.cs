using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;

namespace GenderPayGap.WebUI.Tests.Mocks
{
    public class MockCache : IDistributedCache
    {

        private readonly Dictionary<string, byte[]> cacheStorage = new Dictionary<string, byte[]>();

        public byte[] Get(string key)
        {
            return cacheStorage[key];
        }

        public Task<byte[]> GetAsync(string key, CancellationToken token = default)
        {
            return Task.FromResult(cacheStorage.ContainsKey(key) ? cacheStorage[key] : null);
        }

        public void Refresh(string key)
        {
            throw new NotImplementedException();
        }

        public Task RefreshAsync(string key, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        public void Remove(string key)
        {
            cacheStorage.Remove(key);
        }

        public Task RemoveAsync(string key, CancellationToken token = default)
        {
            cacheStorage.Remove(key);
            return Task.CompletedTask;
        }

        public void Set(string key, byte[] value, DistributedCacheEntryOptions options)
        {
            cacheStorage[key] = value;
        }

        public Task SetAsync(string key, byte[] value, DistributedCacheEntryOptions options, CancellationToken token = default)
        {
            cacheStorage[key] = value;
            return Task.CompletedTask;
        }

    }
}
