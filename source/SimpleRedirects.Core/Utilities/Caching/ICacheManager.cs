using System;

namespace SimpleRedirects.Core.Utilities.Caching
{
    public interface ICacheManager
    {
        T GetCacheItem<T>(string key, Func<T> getCacheItem);
        void ClearByKeyPrefix(string keyPrefix);
    }
}