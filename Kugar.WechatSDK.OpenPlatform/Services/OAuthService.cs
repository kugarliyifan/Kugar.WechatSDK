using System;
using System.Linq;
using System.Threading.Tasks;
using Kugar.Core.BaseStruct;
using Kugar.Core.ExtMethod;
using Kugar.WechatSDK.Common;
using Kugar.WechatSDK.Common.Gateway;
using Kugar.WechatSDK.Common.Helpers;
using Kugar.WechatSDK.OpenPlatform.Enums;
using Kugar.WechatSDK.OpenPlatform.Results;

namespace Kugar.WechatSDK.OpenPlatform.Services
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
        /// <param name="state">附带数据</param>
        /// <param name="self_redirect">true：手机点击确认登录后可以在 iframe 内跳转到 redirect_uri，false：手机点击确认登录后可以在 top window 跳转到 redirect_uri。默认为 false。</param>
        /// <returns></returns>
        string BuildOAuthUrl(string appID, string redirect_uri, 
            string state = "",bool self_redirect=false,string style="black",string href="");

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
    public class OAuthService:OpenPlatformBaseService, IOAuthService
    {
        private IWechatGateway _gateway=null;
        private ISdkTicketContrainer _sdkTicketContrainer = null;

        public OAuthService(ICommonApi api,IWechatGateway gateway,ISdkTicketContrainer sdkTicketContrainer) : base(api)
        {
            _gateway = gateway;
            _sdkTicketContrainer = sdkTicketContrainer;
        }

        /// <summary>
        /// 构造网页授权跳转的Url
        /// </summary>
        /// <param name="appID"></param>
        /// <param name="redirect_uri">重定向后跳转的地址</param>
        /// <param name="state">附带数据</param>
        /// <param name="self_redirect">true：手机点击确认登录后可以在 iframe 内跳转到 redirect_uri，false：手机点击确认登录后可以在 top window 跳转到 redirect_uri。默认为 false。</param>
        /// <param name="style">提供"black"、"white"可选，默认为黑色文字描述</param>
        /// <param name="href">自定义样式链接，第三方可根据实际需求覆盖默认样式</param>
        /// <returns></returns>
        public string BuildOAuthUrl(string appID, string redirect_uri, 
            string state = "",bool self_redirect=false,string style="black",string href="")
        {
            if (!string.IsNullOrWhiteSpace(state))
            {
                if (state.Length>128)
                {
                    throw new ArgumentOutOfRangeException(nameof(state), "state参数不能超过128个字符");
                }
            }

            var url =
                $"https://open.weixin.qq.com/connect/oauth2/authorize?appid={appID}&style={style}&href={href}&self_redirect={(self_redirect?"true":"false")}&redirect_uri={Uri.EscapeUriString(redirect_uri)}&response_type=code&scope=snsapi_login&state={Uri.EscapeUriString(state.ToStringEx())}#wechat_redirect";

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
                    Type = json.GetString("scope") == "base" ? SnsapiType.Base : SnsapiType.UserInfo,
                    UionId = json.GetString("unionid")
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

        /// <summary>
        /// 获取移动应用扫码登录接口,用于提供给App进行移动端提供生成二维码并由用户扫码获取登录code使用<br/>详情请见:<a>https://developers.weixin.qq.com/doc/oplatform/Mobile_App/WeChat_Login/Login_via_Scan.html</a>
        /// </summary>
        /// <param name="appId"></param>
        /// <returns></returns>
        public async Task<ResultReturn<BuildAppScanQrcodeArgument_Result>> BuildAppScanQrcodeArgument(string appId)
        {
            var jsTicket =await _sdkTicketContrainer.GetSdkTicket(appId);
            var nonce = Guid.NewGuid().ToString("N");
            var timestamp = DateTimeHelper.GetUnixDateTime(DateTime.Now);
            
            var waitSignStr =
                $"appid={appId}&noncestr={nonce}&sdk_ticket={jsTicket}&timestamp={timestamp.ToStringEx()}&url={url}";

            var signStr = EncryptHelper.GetSha1(waitSignStr);

            return new SuccessResultReturn<BuildAppScanQrcodeArgument_Result>(new BuildAppScanQrcodeArgument_Result()
            {
                AppId = appId,
                NonceStr = nonce,
                Signature = signStr,
                Timestamp = timestamp
            });
        }
    }
}
