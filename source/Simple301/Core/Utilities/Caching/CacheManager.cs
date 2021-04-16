using SimpleRedirects.Core.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Caching;
using Umbraco.Core.Cache;

namespace SimpleRedirects.Core.Utilities.Caching
{
    /// <summary>
    /// Cache Manager service
    /// </summary>
    public class CacheManager : ICacheManager
    {
        private readonly IAppPolicyCache _cache;
        private readonly bool _cacheEnabled;
        private readonly int _cacheDuration;

        public CacheManager(AppCaches appCaches)
        {
            _cache = appCaches.RuntimeCache;

            var settingsUtility = new SettingsUtility();

            // define the cache duration
            _cacheDuration = settingsUtility.AppSettingExists(SettingsKeys.CacheDurationKey) ?
                settingsUtility.GetAppSetting<int>(SettingsKeys.CacheDurationKey) : 86400;

            // define cache enabled
            _cacheEnabled = !settingsUtility.AppSettingExists(SettingsKeys.CacheEnabledKey) || settingsUtility.GetAppSetting<bool>(SettingsKeys.CacheEnabledKey);
        }

        public T GetCacheItem<T>(string key, Func<T> getCacheItem)
        {
            return _cacheEnabled ? _cache.GetCacheItem(key, getCacheItem, TimeSpan.FromSeconds(_cacheDuration)) : getCacheItem.Invoke();
        }

        public void ClearByKeyPrefix(string keyPrefix)
        {
            _cache.ClearByKey(keyPrefix);
        }
    }
}
