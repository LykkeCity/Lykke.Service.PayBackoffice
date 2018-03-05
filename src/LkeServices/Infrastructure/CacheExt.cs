using System;
using System.Threading.Tasks;
using Common;
using Common.Log;
using Microsoft.Extensions.Caching.Distributed;

namespace LkeServices.Infrastructure
{
    public static class CacheExt
    {
        public static async Task<T> TryGetFromCache<T>(this IDistributedCache cache, string key,
            Func<Task<T>> getRecordFunc, TimeSpan expiration)
        {
                var record = await TryGetRecordFromCache<T>(cache, key);

                if (record == null)
                {
                    record = await getRecordFunc();
                    await TryUpdateRecordInCache(cache, key, record, expiration);
                }

                return record;                        
        }

        private static async Task<T> TryGetRecordFromCache<T>(IDistributedCache cache, string key)
        {            
            string value = await cache.GetStringAsync(key);

            if (value != null)
            {
                return value.DeserializeJson<T>();
            }
            
            return default(T);
        }

        private static async Task TryUpdateRecordInCache<T>(IDistributedCache cache, string key, T record, TimeSpan? expiration)
        {            
            await cache.SetStringAsync(key, record.ToJson(), new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = expiration });            
        }
    }
}
