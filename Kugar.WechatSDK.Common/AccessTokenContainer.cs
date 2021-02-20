using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Kugar.Core.ExtMethod;
using Kugar.Core.Network;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Kugar.WechatSDK.Common
{
    public class AccessTokenContainer:IAccessTokenContainer
    {
        private ConcurrentDictionary<string, (string appID, string appSerect)> _config =
            new ConcurrentDictionary<string, (string appID, string appSerect)>();

        private IMemoryCache _accessTokenCache = null;
        private ILoggerFactory _loggerFactory = null;
        private HttpRequestHelper _request = null;
        

        public AccessTokenContainer(IMemoryCache cache,HttpRequestHelper request,ILoggerFactory loggerFactory=null)
        {
            _accessTokenCache = cache;
            _loggerFactory = loggerFactory;
            _request = request;
        }

        public async Task<string> GetAccessToken(string appId)
        {
            return await _accessTokenCache.GetOrCreateAsync(appId,async x =>
            {
                Debugger.Break();
                if (_config.TryGetValue(appId,out var tmp))
                {
                    var data=await _request.SendApiJson(
                        $"/cgi-bin/token?grant_type=client_credential&appid={appId}&secret={tmp.appSerect}",
                        HttpMethod.Get);

                    //var data=await WebHelper.Create($"https://api.weixin.qq.com/cgi-bin/token?grant_type=client_credential&appid={appID}&secret={tmp.appSerect}")
                    //    .Get_JsonAsync();

                    var errorCode = data.GetInt("errcode",0);

                    if (errorCode==0)
                    {
                        var token = data.GetString("access_token");
                        var expire = data.GetInt("expires_in");

                        _loggerFactory?.CreateLogger("weixin")?.Log(LogLevel.Trace,$"获取accesstoken返回值:{data.ToStringEx(Formatting.None)}");

                        x.AbsoluteExpiration=DateTimeOffset.Now.AddSeconds(expire-2);

                        return token;
                    }
                    else
                    {
                        _loggerFactory?.CreateLogger("weixin")?.Log(LogLevel.Error,$"调用微信获取token失败,错误代码:{errorCode.ToStringEx()}");
                        throw new Exception($"调用微信获取token失败,错误代码:{errorCode.ToStringEx()}");
                    }
                }
                else
                {
                    throw new ArgumentOutOfRangeException(nameof(appId),"为托管对应的AccessToken");
                }
            });
        }

        public IEnumerable<(string appID, string token)> GetAllTokens()
        {
            return _config.Select(x =>
            {
                if (_accessTokenCache.TryGetValue(x.Key, out var t))
                {
                    return (x.Key,(string) t);
                }
                else
                {
                    return (x.Key,string.Empty);
                }
            });
        }

        public async Task<string> RefreshAccessToken(string appId)
        {
            _config.Remove(appId,out _);
            return await GetAccessToken(appId);
        }

        public async Task<bool> CheckAccessToken(string appId)
        {
            if (_accessTokenCache.TryGetValue(appId,out var token))
            {
                var result=await _request.SendApiJson($"/cgi-bin/get_api_domain_ip?access_token={token.ToStringEx()}",HttpMethod.Get);

                var errorCode = result.GetInt("errcode",0);

                if (errorCode==0)
                {
                    return true;
                }
                else if(errorCode==4001)
                {
                    return false;
                }
                else
                {
                    _loggerFactory?.CreateLogger("weixin")?.Log(LogLevel.Error,$"检查{appId}的AccessToken发生错误,返回的数据为:{result.ToStringEx(Formatting.None)}");
                    throw new Exception($"检查{appId}的AccessToken发生错误,返回的数据为:{result.ToStringEx(Formatting.None)}");
                }
            }
            else
            {
                return true;
            }
            
        }

        public bool Register(string appId, string appSerect)
        {
            if (_config.TryAdd(appId, (appId, appSerect)))
            {
                _accessTokenCache.Remove(appId);
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool Exists(string appId)
        {
            return _config.ContainsKey(appId);
        }

        public void Remove(string appId)
        {
            _config.Remove(appId,out _);
            _accessTokenCache.Remove(appId);
        }
        
        //internal void RemoveAccessToken(string appId)
        //{
        //    _accessTokenCache.Remove(appId);
        //}
    }
}