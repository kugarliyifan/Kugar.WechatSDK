using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Kugar.Core.BaseStruct;
using Kugar.Core.ExtMethod;
using Kugar.Core.Log;
using Kugar.WechatSDK.Common.Gateway;
using Kugar.WechatSDK.MP.Enums;
using Kugar.WechatSDK.MP.Results;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ILoggerFactory = Microsoft.Extensions.Logging.ILoggerFactory;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace Kugar.WechatSDK.MP.Web
{

    public class WechatMPController : ControllerBase
    {
        [Route("Core/MP/Callback/{authScheme}/{appID}")]
        [AllowAnonymous]
        public async Task<IActionResult> Callback([FromRoute] string appID,
            [FromServices] IWechatMPApi mp, 
            [FromServices] IMemoryCache cache,
            [FromServices] OptionsManager<WechatJWTOption> options,
            [FromRoute] string authScheme,
            [FromQuery] string code = "", [FromQuery] string state = "")
        {
            Debugger.Break();
            

            if (code == "" || state == "")
            {
                return Content("无效code");
            }

            if (!cache.TryGetValue(state, out var stateData))
            {
                return Content($"state无效:{state}");
            }

            var json = JObject.Parse(stateData.ToStringEx());

            var redirectUrl = json.GetString("redirectUrl");
            var oauthType = (SnsapiType)json.GetInt("oauthType");
            //var scheme = json.GetString("scheme");

            var ret1 = await mp.OAuth.GetAccessToken(appID, code);

            if (!ret1.IsSuccess)
            {
                return Content($"accesstoken无效:code={ret1.ReturnCode};message={ret1.Message}");
            }

            LoggerManager.Default.Debug($"accesstoken:{JsonConvert.SerializeObject(ret1.ReturnData)}");

            WxUserInfo_Result wxUserInfo = null;

            if (oauthType == SnsapiType.UserInfo)
            {
                wxUserInfo = (await mp.OAuth.GetUserInfo(appID, ret1.ReturnData.OpenId, ret1.ReturnData.AccessToken)).ReturnData;

                LoggerManager.Default.Debug($"userInfo:{JsonConvert.SerializeObject(wxUserInfo)}");
            }

            var option = options.Get(authScheme);

            if (option.AfterOAuth != null)
            {
                await option.AfterOAuth(this.HttpContext,
                    appID,
                    ret1.ReturnData.OpenId,
                    ret1.ReturnData.RefreshToken,
                    ret1.ReturnData.AccessToken,
                    wxUserInfo,
                    mp
                );
            }

            ResultReturn<string> ret;
            
            try
            {
                var loginService = (IWechatJWTLoginService) HttpContext.RequestServices.GetService(option.LoginService);

                ret = await loginService.Login(this.HttpContext, appID, ret1.ReturnData.OpenId, ret1.ReturnData.Type, mp);
            }
            catch (Exception e)
            {
                LoggerManager.Default.Error("登录接口抛错", e);
                throw;
            }


            if (ret.IsSuccess)
            {
                var tokenHandler = new JwtSecurityTokenHandler();

                var authTime = DateTime.UtcNow;
                var expiresAt = authTime.Add(option.ExpireTimeSpan);
                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(new Claim[]
                    {
                        new Claim("aud", option.Audience),
                        new Claim("iss", option.Issuer),
                        //new Claim("k",pw.DesEncrypt(option.TokenEncKey.Left(8))),
                        new Claim("OpenID",ret1.ReturnData.OpenId),
                        new Claim("AppID",appID),
                        new Claim("OAuthType",((int)oauthType).ToStringEx()),
                        new Claim(ClaimTypes.NameIdentifier,ret.ReturnData)
                    }),
                    Expires = expiresAt,
                    SigningCredentials =
                        new SigningCredentials(new SymmetricSecurityKey(option.ActualEncKey), SecurityAlgorithms.HmacSha256Signature),
                    EncryptingCredentials = new EncryptingCredentials(new SymmetricSecurityKey(option.ActualEncKey),
                        JwtConstants.DirectKeyUseAlg, SecurityAlgorithms.Aes256CbcHmacSha512)
                };
                var token = tokenHandler.CreateToken(tokenDescriptor);
                var tokenString = tokenHandler.WriteToken(token);

                if (option.Cookie.Expiration == null)
                {
                    option.Cookie.Expiration = option.ExpireTimeSpan;
                }

                Response.Cookies.Append(string.IsNullOrEmpty(option.Cookie.Name) ? $"jwt.{authScheme}" : option.Cookie.Name, tokenString, option.Cookie.Build(HttpContext));

                return Redirect(redirectUrl);
            }
            else
            {
                return Content(ret.Message);
            }

        }

        [Route("Core/MPCallback/service/{appID}")]
        [HttpGet]
        public async Task<IActionResult> Service([FromServices] IWechatGateway gateway,
            [FromServices] ILoggerFactory logger,
            [FromQuery] string signature,
            [FromQuery] string timestamp,
            [FromQuery] string nonce,
            [FromQuery] string echostr,
            [FromRoute] string appID = "")
        {
            if (gateway == null)
            {
                return Content("请先注册微信公众号服务");
            }

            logger?.CreateLogger("weixin")?.Log(LogLevel.Trace, $"微信调用:signature={signature},timestamp={timestamp},nonce={nonce},echostr={echostr}");

            if (string.IsNullOrWhiteSpace(appID))
            {
                return Content("AppID不能为空");
            }

            var config = gateway.Get(appID) as MPConfiguration;

            if (config == null)
            {
                return Content("该AppID非公众号配置");
            }

            if (CheckSignature.Check(signature, timestamp, nonce, config.Token))
            {
                return Content(echostr); //返回随机字符串则表示验证通过
            }
            else
            {
                return Content("failed:" + signature + "," + CheckSignature.GetSignature(timestamp, nonce, config.Token) + "。" +
                               "如果你在浏览器中看到这句话，说明此地址可以被作为微信公众账号后台的Url，请注意保持Token一致。");
            }

        }

        [Route("Core/MPCallback/service/{appID}")]
        [ActionName("Service")]
        [HttpPost]
        public async Task<IActionResult> ServicePost([FromServices] IWechatGateway gateway,
            [FromServices] ILoggerFactory logger,
            [FromQuery] string signature,
            [FromQuery] string timestamp,
            [FromQuery] string nonce,
            [FromQuery] string echostr,
            [FromRoute] string appID = "")
        {
            return Content("");

            if (gateway == null)
            {
                return Content("请先注册微信公众号服务");
            }

            logger?.CreateLogger("weixin")?.Log(LogLevel.Trace, $"微信调用:signature={signature},timestamp={timestamp},nonce={nonce},echostr={echostr}");

            if (string.IsNullOrWhiteSpace(appID))
            {
                return Content("AppID不能为空");
            }

            var config = gateway.Get(appID) as MPConfiguration;

            if (config == null)
            {
                return Content("该AppID非公众号配置");
            }

            if (CheckSignature.Check(signature, timestamp, nonce, config.Token))
            {
                return Content("校验无效,请检查token");
            }

            // v4.2.2之后的版本，可以设置每个人上下文消息储存的最大数量，防止内存占用过多，如果该参数小于等于0，则不限制
            var maxRecordCount = 10;

            Request.EnableBuffering();

            //自定义MessageHandler，对微信请求的详细判断操作都在这里面。
            var inputStream = Request.Body;
            inputStream.Position = 0;

            var xml = new XmlDocument();
            xml.Load(inputStream);



            return null;
        }
    }

    /// <summary>
    /// 签名验证类
    /// </summary>
    internal class CheckSignature
    {
        /// <summary>
        /// 在网站没有提供Token（或传入为null）的情况下的默认Token，建议在网站中进行配置。
        /// </summary>
        public const string Token = "weixin";


        /// <summary>
        /// 检查签名是否正确
        /// </summary>
        /// <param name="signature"></param>
        /// <param name="timestamp"></param>
        /// <param name="nonce"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public static bool Check(string signature, string timestamp, string nonce, string token = null)
        {
            return signature == GetSignature(timestamp, nonce, token);
        }


        /// <summary>
        /// 返回正确的签名
        /// </summary>
        /// <param name="timestamp"></param>
        /// <param name="nonce"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public static string GetSignature(string timestamp, string nonce, string token = null)
        {
            token = token ?? Token;
            var arr = new[] { token, timestamp, nonce }.OrderBy(z => z).ToArray();
            var arrString = string.Join("", arr);
            //var enText = FormsAuthentication.HashPasswordForStoringInConfigFile(arrString, "SHA1");//使用System.Web.Security程序集
            using (var sha1 = SHA1.Create())
            {
                var sha1Arr = sha1.ComputeHash(Encoding.UTF8.GetBytes(arrString));
                StringBuilder enText = new StringBuilder();
                foreach (var b in sha1Arr)
                {
                    enText.AppendFormat("{0:x2}", b);
                }

                return enText.ToString();
            }


        }
    }
}
