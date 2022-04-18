using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;
using Kugar.Core.BaseStruct;
using Kugar.Core.ExtMethod;
using Kugar.Core.Log;
using Kugar.WechatSDK.Common;
using Kugar.WechatSDK.Common.Gateway;
using Kugar.WechatSDK.Common.Helpers;
using Kugar.WechatSDK.MP.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using ILoggerFactory = Microsoft.Extensions.Logging.ILoggerFactory;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace Kugar.WechatSDK.MP
{
    public interface IJsTicketContainer
    {
        /// <summary>
        /// 获取JsTicket
        /// </summary>
        /// <param name="appID"></param>
        /// <returns></returns>
        Task<string> GetJsTicket(string appID);

        /// <summary>
        /// 强制刷新ticket
        /// </summary>
        /// <param name="appId"></param>
        /// <returns></returns>
        Task RefreshAccessToken(string appId);

        bool Register(string appID);

        bool Register(string appId, AccessTokenFactory factory);

        void Remove(string appID);

        IEnumerable<string> GetAllAppIDs();
    }

    public class JsTicketContainer : IJsTicketContainer
    {
        private HashSet<string> _config =new HashSet<string>();

        private IMemoryCache _accessTokenCache = null;
        private ILoggerFactory _loggerFactory = null;
        private ICommonApi _api = null;
        private IWechatGateway _gateway = null;
        private IHttpContextAccessor _context = null;

        private ConcurrentDictionary<string, AccessTokenFactory> _otherTokens =
            new ConcurrentDictionary<string, AccessTokenFactory>();

        public JsTicketContainer(IMemoryCache cache,ICommonApi api,ILoggerFactory loggerFactory, IWechatGateway gateway,IHttpContextAccessor accessor,IAccessTokenContainer container)
        {
            _accessTokenCache = cache;
            _loggerFactory = loggerFactory;
            _api = api;
            _gateway = gateway;
            _context = accessor;

            container.TokenRefresh += Container_TokenRefresh;
        }

        private async void Container_TokenRefresh(object sender, TokenRefreshEventArgs e)
        {
            //Debugger.Break();
            await this.RefreshAccessToken(e.AppId);

            _loggerFactory?.CreateLogger("wechat").Log(LogLevel.Debug, $"{e.AppId}已刷新");
        }

        /// <summary>
        /// 获取JsTicket
        /// </summary>
        /// <param name="appID"></param>
        /// <returns></returns>
        public async Task<string> GetJsTicket(string appID)
        {
            var config = _gateway.Get(appID);
            //Debugger.Break();
            if (!config.ManagerAccessToken)
            {
                if (_otherTokens.TryGetValue(appID,out var f))
                {
                    return await f(appID);
                }
                else
                {
                    throw new Exception("当前AppId未设置管理ticket,并且也未设置JsTicketFactory,无法获取JsTicket");
                }
            } 
            //Debugger.Break();
            return await _accessTokenCache.GetOrCreateAsync($"JT-{appID}",async x =>
            {
                
                if (_config.TryGetValue(appID,out var tmp))
                {
                    //Debugger.Break();
                    var data = await _api.Get(appID,
                        $"/cgi-bin/ticket/getticket?access_token=ACCESS_TOKEN&type=jsapi");

                    //var data = await _api.Get(appID,
                    //    $"/cgi-bin/ticket/getticket?access_token=ACCESS_TOKEN&type=wx_card");

                    //var data=await WebHelper.Create($"https://api.weixin.qq.com/cgi-bin/token?grant_type=client_credential&appid={appID}&secret={tmp.appSerect}")
                    //    .Get_JsonAsync();

                    //Console.WriteLine("jsTicket:" + data.ReturnData.ToStringEx());
                    LoggerManager.Default.Debug("jsTicket:" + data.ReturnData.ToStringEx());

                    if (data.IsSuccess)
                    {
                        var token = data.ReturnData.GetString("ticket");
                        var expire = data.ReturnData.GetInt("expires_in");
                        _loggerFactory.CreateLogger("wechat").LogDebug("JsTicket:{0}",token);
                        x.AbsoluteExpiration=DateTimeOffset.Now.AddSeconds(expire-60);
                        x.Value = token;
                        return token;
                    }
                    else
                    {
                        //Debugger.Break();
                        _loggerFactory?.CreateLogger("weixin")?.Log(LogLevel.Error,$"调用微信获取token失败,错误代码:{data.ReturnCode.ToStringEx()}");
                        throw new Exception($"调用微信获取token失败,错误代码:{data.ReturnCode.ToStringEx()}");
                    }
                }
                else
                {
                    throw new ArgumentOutOfRangeException(nameof(appID));
                }
            });
        }

        public async Task  RefreshAccessToken(string appId)
        {
            
            _accessTokenCache.Remove(appId);
            //return await GetJsTicket(appId);

            
        } 

        public bool Register(string appId, AccessTokenFactory factory)
        {
            if (_otherTokens.TryAdd(appId, factory))
            {
                _accessTokenCache.Remove(appId);
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool Register(string appID)
        {
            //return Register(appID, getTicketByToken);

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

        private async Task<string> getTicketByToken(string appId)
        {
            return await _accessTokenCache.GetOrCreateAsync(appId, async x =>
            {
                if (_config.TryGetValue(appId, out var tmp))
                {
                    //Debugger.Break();
                    var data = await _api.Get(appId,
                        $"/cgi-bin/ticket/getticket?access_token=ACCESS_TOKEN&type=jsapi");

                    //var data=await WebHelper.Create($"https://api.weixin.qq.com/cgi-bin/token?grant_type=client_credential&appid={appID}&secret={tmp.appSerect}")
                    //    .Get_JsonAsync();

                    if (data.IsSuccess)
                    {
                        var token = data.ReturnData.GetString("ticket");
                        var expire = data.ReturnData.GetInt("expires_in");

                        x.AbsoluteExpiration = DateTimeOffset.Now.AddSeconds(expire - 60);

                        return token;
                    }
                    else
                    {
                        _loggerFactory?.CreateLogger("weixin")?.Log(LogLevel.Error,
                            $"调用微信获取token失败,错误代码:{data.ReturnCode.ToStringEx()}");
                        throw new Exception($"调用微信获取token失败,错误代码:{data.ReturnCode.ToStringEx()}");
                    }
                }
                else
                {
                    throw new ArgumentOutOfRangeException(nameof(appId));
                }
            });
        }

    }
}
