using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kugar.Core.BaseStruct;
using Kugar.Core.ExtMethod;
using Kugar.WechatSDK.Common;
using Kugar.WechatSDK.Common.Gateway;
using Kugar.WechatSDK.MP.Enums;
using Kugar.WechatSDK.MP.Results;

namespace Kugar.WechatSDK.MP
{
    /// <summary>
    /// 用户授权以及用户信息功能
    /// </summary>
    public interface IOAuthService
    {
        /// <summary>
        /// 构造网页授权跳转的Url
        /// </summary>
        /// <param name="appID"></param>
        /// <param name="redirect_uri">重定向后跳转的地址</param>
        /// <param name="type">授权类型</param>
        /// <param name="state">附带数据</param>
        /// <returns></returns>
        string BuildOAuthUrl(string appID, string redirect_uri, SnsapiType type = SnsapiType.Base,
            string state = "");

        /// <summary>
        /// 通过网页跳转后获取到的code,取得用户openID
        /// </summary>
        /// <param name="appID"></param>
        /// <param name="code">授权跳转后,传入的code值</param>
        /// <returns></returns>
        Task<ResultReturn<GetAccessToken_Result>> GetAccessToken(string appID, string code);

        /// <summary>
        /// 用于刷新用户的accesstoken
        /// </summary>
        /// <param name="appID"></param>
        /// <param name="accessToken">从GetAccessToken函数中返回的accesstoken</param>
        /// <returns></returns>
        Task<ResultReturn<RefreshAccessToken_Result>> RefreshAccessToken(string appID, string accessToken);

        /// <summary>
        /// 通过openid和accesstoken获取用户信息,如果要只通过openid获取,可使用IWechatMPApi.UserManagement.GetUserInfo获取,如果需要获取用户信息信息,需要用户为已关注用户才可以,否则只能获取基础信息
        /// </summary>
        /// <param name="appID"></param>
        /// <param name="openID"></param>
        /// <param name="accessToken"></param>
        /// <returns></returns>
        Task<ResultReturn<WxUserInfo_Result>> GetUserInfo(string appID,string openID, string accessToken);

        /// <summary>
        /// 检验授权凭证（access_token）是否有效
        /// </summary>
        /// <param name="appID"></param>
        /// <param name="openID"></param>
        /// <param name="accessToken">用户的accessToken</param>
        /// <returns></returns>
        Task<bool> IsAccessTokenValidate(string appID, string openID, string accessToken);

        /// <summary>
        /// 获取订阅用户的信息,必须是已经订阅的用户,才能获取到用户信息
        /// </summary>
        /// <param name="appID"></param>
        /// <param name="openID"></param>
        /// <returns></returns>
        Task<ResultReturn<SubscribeWxUserInfo_Result>> GetSubscribeUserInfo(string appID, string openID);
    }

    /// <summary>
    /// 用户授权以及用户信息功能
    /// </summary>
    public class OAuthService:MPBaseService, IOAuthService
    {
        private IWechatGateway _gateway=null;

        public OAuthService(ICommonApi api,IWechatGateway gateway) : base(api)
        {
            _gateway = gateway;
        }

        /// <summary>
        /// 构造网页授权跳转的Url
        /// </summary>
        /// <param name="appID"></param>
        /// <param name="redirect_uri">重定向后跳转的地址</param>
        /// <param name="type">授权类型</param>
        /// <param name="state">附带数据</param>
        /// <returns></returns>
        public string BuildOAuthUrl(string appID, string redirect_uri, SnsapiType type = SnsapiType.Base,
            string state = "")
        {
            if (!string.IsNullOrWhiteSpace(state))
            {
                if (state.Length>128)
                {
                    throw new ArgumentOutOfRangeException(nameof(state), "state参数不能超过128个字符");
                }
            }

            var url =
                $"https://open.weixin.qq.com/connect/oauth2/authorize?appid={appID}&redirect_uri={Uri.EscapeUriString(redirect_uri)}&response_type=code&scope={(type == SnsapiType.Base ? "snsapi_base" : "snsapi_userinfo")}&state={Uri.EscapeUriString(state.ToStringEx())}#wechat_redirect";

            return url;
        }

        /// <summary>
        /// 通过网页跳转后获取到的code,取得用户openID
        /// </summary>
        /// <param name="appID"></param>
        /// <param name="code">授权跳转后,传入的code值</param>
        /// <returns></returns>
        public async Task<ResultReturn<GetAccessToken_Result>> GetAccessToken(string appID, string code)
        {
            var item = _gateway.Get(appID);

            if (item==null)
            {
                throw new ArgumentOutOfRangeException(nameof(appID));
            }

            if (string.IsNullOrWhiteSpace(code))
            {
                throw new ArgumentNullException(nameof(code));
            }

            var result =await CommonApi.Get(appID,
                $"/sns/oauth2/access_token?appid={appID}&secret={item.AppSerect}&code={code}&grant_type=authorization_code");

            if (result.IsSuccess)
            {
                var json = result.ReturnData;
                var ret = new GetAccessToken_Result()
                {
                    AccessToken = json.GetString("access_token"),
                    Expires = json.GetInt("expires_in"),
                    RefreshToken = json.GetString("refresh_token"),
                    OpenId = json.GetString("openid"),
                    Type = json.GetString("scope") == "base" ? SnsapiType.Base : SnsapiType.UserInfo
                };

                return new SuccessResultReturn<GetAccessToken_Result>(ret);
            }
            else
            {
                return result.Cast<GetAccessToken_Result>(default);
            }
        }

        /// <summary>
        /// 用于刷新用户的accesstoken
        /// </summary>
        /// <param name="appID"></param>
        /// <param name="accessToken">从GetAccessToken函数中返回的accesstoken</param>
        /// <returns></returns>
        public async Task<ResultReturn<RefreshAccessToken_Result>> RefreshAccessToken(string appID, string accessToken)
        {

            if (string.IsNullOrWhiteSpace(accessToken))
            {
                throw new ArgumentNullException(nameof(accessToken));
            }

            var result =await CommonApi.Get(appID,
                $"/sns/oauth2/refresh_token?appid={appID}&grant_type=refresh_token&refresh_token={accessToken}");

            if (result.IsSuccess)
            {
                var json = result.ReturnData;
                var ret = new RefreshAccessToken_Result()
                {
                    AccessToken = json.GetString("access_token"),
                    Expires = json.GetInt("expires_in"),
                    RefreshToken = json.GetString("refresh_token"),
                    OpenID = json.GetString("openid"),
                    Type = json.GetString("scope") == "base" ? SnsapiType.Base : SnsapiType.UserInfo
                };

                return new SuccessResultReturn<RefreshAccessToken_Result>(ret);
            }
            else
            {
                return result.Cast<RefreshAccessToken_Result>(null, null);
            }
        }

        public async Task<ResultReturn<WxUserInfo_Result>> GetUserInfo(string appID,string openID, string accessToken)
        {
            var result =await CommonApi.Get(appID,
                $"/sns/userinfo?access_token={accessToken}&openid={openID}&lang=zh_CN");

            if (result.IsSuccess)
            {
                var json = result.ReturnData;
                var ret = new WxUserInfo_Result()
                {
                    OpenID = json.GetString("openid"),
                    NickName = json.GetString("nickname"),
                    Sex = json.GetString("sex").ToInt(),
                    Province = json.GetString("province"),
                    City = json.GetString("city"),
                    Country = json.GetString("country"),
                    HeadImageUrl = json.GetString("headimgurl"),
                    Privilege = json.GetJArray("privilege").Select(x=>x.ToStringEx()).ToArrayEx(),
                    UnionID = json.GetString("unionid")
                };

                return new SuccessResultReturn<WxUserInfo_Result>(ret);
            }
            else
            {
                return result.Cast<WxUserInfo_Result>(null, null);
            }
        }

        /// <summary>
        /// 获取订阅用户的信息,必须是已经订阅的用户,才能获取到用户信息
        /// </summary>
        /// <param name="appID"></param>
        /// <param name="openID"></param>
        /// <returns></returns>
        public async Task<ResultReturn<SubscribeWxUserInfo_Result>> GetSubscribeUserInfo(string appID, string openID)
        {
            var result =await CommonApi.Get(appID,
                $"/cgi-bin/user/info?access_token=ACCESS_TOKEN&openid={openID}&lang=zh_CN");

            if (result.IsSuccess)
            {
                var json = result.ReturnData;
                var ret = new SubscribeWxUserInfo_Result()
                {
                    OpenID = json.GetString("openid"),
                    NickName = json.GetString("nickname"),
                    Sex = json.GetString("sex").ToInt(),
                    Province = json.GetString("province"),
                    City = json.GetString("city"),
                    Country = json.GetString("country"),
                    HeadImageUrl = json.GetString("headimgurl"),
                    Privilege = json.GetJArray("privilege").Select(x=>x.ToStringEx()).ToArrayEx(),
                    UnionID = json.GetString("unionid"),
                    IsSubscribe = json.GetInt("subscribe")==1,
                    SubscribeScene = json.GetString("subscribe_scene"),
                    QrScene = json.GetString("qr_scene")
                };

                return new SuccessResultReturn<SubscribeWxUserInfo_Result>(ret);
            }
            else
            {
                return result.Cast<SubscribeWxUserInfo_Result>(null, null);
            }
        }

        /// <summary>
        /// 检验授权凭证（access_token）是否有效
        /// </summary>
        /// <param name="appID"></param>
        /// <param name="openID"></param>
        /// <param name="accessToken">用户的accessToken</param>
        /// <returns></returns>
        public async Task<bool> IsAccessTokenValidate(string appID, string openID, string accessToken)
        {
            var result =await CommonApi.Get(appID,
                $"/sns/auth?access_token={accessToken}&openid={openID}");

            if (result.ReturnCode==0)
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
