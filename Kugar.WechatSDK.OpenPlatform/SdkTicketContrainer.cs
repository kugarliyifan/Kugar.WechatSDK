using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using Kugar.Core.ExtMethod;
using Kugar.WechatSDK.Common;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace Kugar.WechatSDK.OpenPlatform
{
    public interface ISdkTicketContrainer
    {
        Task<string> GetSdkTicket(string appID);
        bool Register(string appID);
        void Remove(string appID);
        IEnumerable<string> GetAllAppIDs();
    }

    public class SdkTicketContrainer : ISdkTicketContrainer
    {
        private HashSet<string> _config =new HashSet<string>();

        private IMemoryCache _accessTokenCache = null;
        private ILoggerFactory _loggerFactory = null;
        private ICommonApi _api = null;

        public SdkTicketContrainer(IMemoryCache cache,ICommonApi api,ILoggerFactory loggerFactory)
        {
            _accessTokenCache = cache;
            _loggerFactory = loggerFactory;
            _api = api;
        }

        public async Task<string> GetSdkTicket(string appID)
        {
            return await _accessTokenCache.GetOrCreateAsync(appID,async x =>
            {
                if (_config.TryGetValue(appID,out var tmp))
                {
                    Debugger.Break();
                    var data=await _api.Get(appID,
                        $"/cgi-bin/ticket/getticket?access_token=ACCESS_TOKEN&type=2");

                    //var data=await WebHelper.Create($"https://api.weixin.qq.com/cgi-bin/token?grant_type=client_credential&appid={appID}&secret={tmp.appSerect}")
                    //    .Get_JsonAsync();

                    if (data.IsSuccess)
                    {
                        var token = data.ReturnData.GetString("ticket");
                        var expire = data.ReturnData.GetInt("expires_in");

                        x.AbsoluteExpiration=DateTimeOffset.Now.AddSeconds(expire-60);

                        return token;
                    }
                    else
                    {
                        _loggerFactory?.CreateLogger("weixin")?.Log(LogLevel.Error,$"调用微信获取sdkticket失败,错误代码:{data.ReturnCode.ToStringEx()}");
                        throw new Exception($"调用微信获取sdkticket失败,错误代码:{data.ReturnCode.ToStringEx()}");
                    }
                }
                else
                {
                    throw new ArgumentOutOfRangeException(nameof(appID));
                }
            });
        }

        public bool Register(string appID)
        {
            return _config.Add(appID);
        }

        public void Remove(string appID)
        {
            _config.Remove(appID);
        }

        public IEnumerable<string> GetAllAppIDs()
        {
            return _config;
        }
    }
}
