using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Kugar.Core.BaseStruct;
using Kugar.Core.ExtMethod;
using Kugar.WechatSDK.MP.Entities;
using Microsoft.Extensions.Caching.Memory;

namespace Kugar.WechatSDK.MP
{
    public interface IMPMessageCache
    {
        Task<bool> Add(string msgID,DateTimeOffset expireDt);
    }

    public class DefaultMPMessageCache : IMPMessageCache
    {
        private ConcurrentDictionary<string, DateTimeOffset> _messageCache =
            new ConcurrentDictionary<string, DateTimeOffset>();

        private readonly TimerEx _timer = null;

        public DefaultMPMessageCache()
        {
            _timer = new TimerEx(check,60*1000);
            _timer.Start();
        }

        public async Task<bool> Add(string msgID, DateTimeOffset expireDt)
        {
            return _messageCache.TryAdd(msgID, expireDt);
        }

        private void check(object state)
        {
            if (!_messageCache.HasData())
            {
                return;
            }

            var lst = new List<string>();

            foreach (var item in _messageCache)
            {
                if (item.Value<DateTimeOffset.Now)
                {
                    lst.Add(item.Key);
                }
            }

            if (lst.HasData())
            {
                foreach (var key in lst)
                {
                    _messageCache.TryRemove(key,out _);
                }
            }
        }
    }

    public class MessageQueue
    {
        private IMPMessageCache _cache = null;

        public MessageQueue(IMPMessageCache cache)
        {
            _cache = cache;
        }

        public async Task<bool> AddMessage(WechatMPRequestBase msg)
        {
            string key = String.Empty;

            if (msg is WechatMPRequestEventBase e)
            {
                key = $"{e.FromUserOpenId}{e.CreateTime}";
            }
            else
            {
                key=msg.MsgId.ToString();
            }

            if (await _cache.Add(key,DateTimeOffset.Now.AddMinutes(5)))
            {
                return true;
            }
            else
            {
                return false;
            }
        }


    }
}
