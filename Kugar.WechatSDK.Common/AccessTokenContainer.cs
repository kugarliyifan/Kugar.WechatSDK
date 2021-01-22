﻿using System;
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
        private Dictionary<string, (string appID, string appSerect)> _config =
            new Dictionary<string, (string appID, string appSerect)>();

        private IMemoryCache _accessTokenCache = null;
        private ILogger _logger = null;
        private HttpRequestHelper _request = null;

        internal AccessTokenContainer(IMemoryCache cache,ILogger logger,HttpRequestHelper request)
        {
            _accessTokenCache = cache;
            _logger = logger;
            _request = request;
        }

        public async Task<string> GetAccessToken(string appID)
        {
            return await _accessTokenCache.GetOrCreateAsync(appID,async x =>
            {
                Debugger.Break();
                if (_config.TryGetValue(appID,out var tmp))
                {
                    var data=await _request.SendApiJson(
                        $"/cgi-bin/token?grant_type=client_credential&appid={appID}&secret={tmp.appSerect}",
                        HttpMethod.Get);

                    //var data=await WebHelper.Create($"https://api.weixin.qq.com/cgi-bin/token?grant_type=client_credential&appid={appID}&secret={tmp.appSerect}")
                    //    .Get_JsonAsync();

                    var errorCode = data.GetInt("errcode",0);

                    if (errorCode==0)
                    {
                        var token = data.GetString("access_token");
                        var expire = data.GetInt("expires_in");

                        x.AbsoluteExpiration=DateTimeOffset.Now.AddSeconds(expire-2);

                        return token;
                    }
                    else
                    {
                        _logger?.Log(LogLevel.Error,$"调用微信获取token失败,错误代码:{errorCode.ToStringEx()}");
                        throw new Exception($"调用微信获取token失败,错误代码:{errorCode.ToStringEx()}");
                    }
                }
                else
                {
                    throw new ArgumentOutOfRangeException(nameof(appID));
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

        public async Task<string> RefreshAccessToken(string appID)
        {
            _config.Remove(appID);
            return await GetAccessToken(appID);
        }

        public async Task<bool> CheckAccessToken(string appID)
        {
            if (_accessTokenCache.TryGetValue(appID,out var token))
            {
                var result=await _request.SendApiJson($"https://api.weixin.qq.com/cgi-bin/get_api_domain_ip?access_token={token.ToStringEx()}",HttpMethod.Get);

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
                    throw new Exception($"检查{appID}的AccessToken发生错误,返回的数据为:{result.ToStringEx(Formatting.None)}");
                }
            }
            else
            {
                return true;
            }
            
        }

        public bool Register(string appID, string appSerect)
        {
            if (_config.TryAdd(appID, (appID, appSerect)))
            {
                _accessTokenCache.Remove(appID);
                return true;
            }
            else
            {
                return false;
            }
        }

        public void Remove(string appID)
        {
            _config.Remove(appID);
            _accessTokenCache.Remove(appID);
        }
        
        internal void RemoveAccessToken(string appID)
        {
            _accessTokenCache.Remove(appID);
        }
    }
}