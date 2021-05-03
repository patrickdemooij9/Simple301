using System;
using Microsoft.Extensions.Options;
using SimpleRedirects.Core.Options;
using Umbraco.Cms.Core.Cache;
using Umbraco.Extensions;

namespace SimpleRedirects.Core.Utilities.Caching
{
    public class CacheManager : ICacheManager
    {
        private readonly IAppPolicyCache _cache;
        private readonly bool _cacheEnabled;
        private readonly int _cacheDuration;

        public CacheManager(AppCaches appCaches, IOptions<SimpleRedirectsOptions> options)
        {
            _cache = appCaches.RuntimeCache;

            var settings = options.Value;
            _cacheEnabled = settings.CacheEnabled;
            _cacheDuration = settings.CacheDuration;
        }

        public void ClearByKeyPrefix(string keyPrefix)
        {
            _cache.ClearByKey(keyPrefix);
        }

        public T GetCacheItem<T>(string key, Func<T> getCacheItem)
        {
            return _cacheEnabled ? _cache.GetCacheItem(key, getCacheItem, TimeSpan.FromSeconds(_cacheDuration)) : getCacheItem.Invoke();
        }
    }
}