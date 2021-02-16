using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Kugar.WechatSDK.MP.Entities;
using Microsoft.Extensions.Caching.Memory;

namespace Kugar.WechatSDK.MP
{
    public class MPMessageHandler
    {
        private IMemoryCache _cache = null;

        public MPMessageHandler(IMemoryCache cache)
        {
            _cache = cache;
        }

        public async Task<bool> AddMessage(WechatMPRequestBase msg)
        {
            var msgId = msg.MsgId;

            var item=await _cache.GetOrCreateAsync<string>(msg.MsgId, x =>
            {
                x.AbsoluteExpiration = DateTimeOffset.Now.AddMinutes(5);

                x.Value = "";

                return Task.FromResult("");
            });

            if (!_cache.TryGetValue(msg.MsgId,out var t))
            {
                return true;
            }
            else
            {
                var e=_cache.CreateEntry(msg.MsgId);

                e.Value = "";

                e.Dispose();

                return false;
            }
        }


    }
}
