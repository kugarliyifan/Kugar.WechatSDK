﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Kugar.Core.ExtMethod;
using Kugar.Core.Network;
using Kugar.Core.Services;
using Kugar.WechatSDK.Common.Gateway;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Kugar.WechatSDK.Common
{
    public class AccessTokenContainer:IAccessTokenContainer
    {
        private ConcurrentDictionary<string, (string appID, string appSerect)> _config =
            new ConcurrentDictionary<string, (string appID, string appSerect)>();

        private ConcurrentDictionary<string, AccessTokenFactory> _otherTokens =
            new ConcurrentDictionary<string, AccessTokenFactory>();

        private IMemoryCache _accessTokenCache = null;
        private ILoggerFactory _loggerFactory = null;
        private HttpRequestHelper _request = null;
        //private IWechatGateway _gateway = null; 


        public AccessTokenContainer(IMemoryCache cache,HttpRequestHelper request,ILoggerFactory loggerFactory)
        {
            _accessTokenCache = cache;
            _loggerFactory = loggerFactory;
            _request = request;
        }

        public async Task<string> GetAccessToken(string appId)
        {
            var gateway = GlobalProvider.Provider.GetService<IWechatGateway>();

            var config = gateway.Get(appId);

            if (!config.ManagerAccessToken)
            {
                if (_otherTokens.TryGetValue(appId, out var f))
                {
                    return await f(appId);
                }
                else
                {
                    throw new Exception("当前AppId未设置管理ticket,并且也未设置JsTicketFactory,无法获取JsTicket");
                }
            } 

            return await _accessTokenCache.GetOrCreateAsync(appId,async x =>
            { 
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
                        x.Value = token;
                        

                        return token;
                    }
                    else
                    {
                        Debugger.Break();
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

        public async Task  RefreshAccessToken(string appId)
        {
            _accessTokenCache.Remove(appId );
            
            if (this.TokenRefresh != null)
            {
                this.TokenRefresh.Invoke(this, new TokenRefreshEventArgs(appId));
            }

        }

        public async Task<bool> CheckAccessToken(string appId)
        {
            var token = await this.GetAccessToken(appId);
            
            if (!string.IsNullOrWhiteSpace(token))
            {
                var result = await _request.SendApiJson($"/cgi-bin/get_api_domain_ip?access_token={token.ToStringEx()}", HttpMethod.Get);

                var errorCode = result.GetInt("errcode", 0);

                if (errorCode == 0)
                {
                    return true;
                }
                else if (errorCode == 40001)
                {

                    _loggerFactory.CreateLogger("accesstoken").Log(LogLevel.Information,"40001检查有效性出错");
                    //Debugger.Break();
                    return false;
                }
                else
                {
                    Debugger.Break();
                    _loggerFactory?.CreateLogger("weixin")?.Log(LogLevel.Error, $"检查{appId}的AccessToken发生错误,返回的数据为:{result.ToStringEx(Formatting.None)}");
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

        public bool Register(string appId, AccessTokenFactory factory)
        {
            if (_otherTokens.TryAdd(appId,factory))
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

        public event EventHandler<TokenRefreshEventArgs> TokenRefresh;

        //internal void RemoveAccessToken(string appId)
        //{
        //    _accessTokenCache.Remove(appId);
        //}
    }

    public delegate Task<string> AccessTokenFactory(string appId);
}