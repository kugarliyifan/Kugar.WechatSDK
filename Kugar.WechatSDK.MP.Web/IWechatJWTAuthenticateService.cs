using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Kugar.Core.BaseStruct;
using Kugar.WechatSDK.MP.Enums;
using Kugar.WechatSDK.MP.Results;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json.Linq;
using RedirectUrl=System.String;

namespace Kugar.WechatSDK.MP.Web
{
    public interface IWechatJWTAuthenticateService
    {
        /// <summary>
        /// 每次访问网页时,回调该函数,用于验证用户是否有效,如需调用DI的类,可用context.RequestService获取
        /// </summary>
        /// <param name="context"></param>
        /// <param name="appID">本次授权使用的AppID</param>
        /// <param name="openID">当前用户的OpenID</param>
        /// <param name="oauthType">当前授权类型</param>
        /// <param name="mp"></param>
        /// <returns></returns>
        Task<ResultReturn<string>> Login(HttpContext context,string appID, string openID, SnsapiType oauthType, IWechatMPApi mp);

        /// <summary>
        /// 微信授权回调之后出发该函数,并传入授权信息,一般用于自动添加/注册用户等操作,只有在授权失败跳转微信授权回调后,才会调用一次该函数,如需调用DI的类,可用context.RequestService获取
        /// </summary>
        /// <param name="context"></param>
        /// <param name="appid">本次授权使用的AppID</param>
        /// <param name="openID">本次授权的公众号AppID</param>
        /// <param name="refresh_token">当前用户的accesstoken刷新token</param>
        /// <param name="accesstoken">当前用户的accesstoken</param>
        /// <param name="userinfo">如果使用userinfo的方式,则为用户信息,,base方式为null</param>
        /// <param name="mpapi">注入的IWechatMPApi</param>
        /// <param name="backUrl">原回跳地址</param>
        /// <returns>如需特殊情况下的跳转,则返回跳转地址,如只需按原地址跳转,返回空字符串</returns>
        Task<RedirectUrl> OnOAuthCompleted(HttpContext context,
            string appid,
            string openID,
            string refresh_token,
            string accesstoken,
            WxUserInfo_Result userinfo,
            IWechatMPApi mpapi,
            string backUrl,
            JObject tempData
        );

        /// <summary>
        /// 用于登录的时候,构造登录函数的时候,将一些需要传递给OnOAuthCompleted的数据缓存起来
        /// </summary>
        /// <param name="context"></param>
        /// <param name="appId"></param>
        /// <param name="loginUrl"></param>
        /// <param name="mpapi"></param>
        /// <returns></returns>
        Task<JObject> OnBeforeLoginTempData(HttpContext context, string appId, string loginUrl, IWechatMPApi mpapi)
        {
            return Task.FromResult((JObject)null);
        }
    }
}
