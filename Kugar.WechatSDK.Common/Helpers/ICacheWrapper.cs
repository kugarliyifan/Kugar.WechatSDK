using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using EasyCaching.Core;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace Kugar.WechatSDK.Common.Helpers
{
    public class CacheWrapper
    {
        private IEasyCachingProviderBase _caching = null;
        private IMemoryCache _cache = null;

        public CacheWrapper(IHybridCachingProvider hybrid = null, IEasyCachingProvider provider=null,IMemoryCache memoryCache=null)
        {
            if (hybrid!=null)
            {
                _caching = hybrid;
            }
            else if (provider != null)
            {
                _caching = provider;
            }
            else if (memoryCache != null)
            {
                _cache = memoryCache;
            }
            else
            {
                throw new Exception("请使用AddMemoryCache或EasyCaching注入Cache");
            }
        }

        //public async Task<TValue> GetAsync<TValue>(string key, Func<Task<TValue>> valueFactory)
        //{
        //    if (_caching!=null)
        //    {
        //        var item= await _caching.GetAsync<TValue>(key, valueFactory, TimeSpan.FromSeconds(50));
        //        _caching.GetAsync<>()
        //        return item.Value;
        //    }
        //    else
        //    {
        //        await _cache.GetOrCreateAsync(key,)
        //    }
        //}
    }
}
