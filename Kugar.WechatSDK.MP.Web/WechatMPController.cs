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
using Kugar.WechatSDK.Common;
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
        [ApiExplorerSettings(IgnoreApi = true)]
        [Route("Core/MP/Callback/{authScheme}/{appID}")]
        [AllowAnonymous]
        public async Task<IActionResult> Callback([FromRoute] string appID,
            [FromServices] IWechatMPApi mp, 
            [FromServices] IMemoryCache cache,
            [FromServices] OptionsManager<WechatJWTOption> options,
            [FromRoute] string authScheme,
            [FromServices] IWechatJWTAuthenticateService loginService=null,
            [FromQuery] string code = "", [FromQuery] string state = "")
        {
            //Debugger.Break();
            

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
            var tempData = json.GetJObject("tempData");
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

            if (loginService==null)
            {
                throw new ArgumentNullException("loginService为空,请使用services.RegisterMPJWTLoginService注册登录服务");
            }
            
            var t1=await loginService?.OnOAuthCompleted(this.HttpContext,
                appID,
                ret1.ReturnData.OpenId,
                ret1.ReturnData.RefreshToken,
                ret1.ReturnData.AccessToken,
                wxUserInfo,
                mp,
                backUrl:redirectUrl,
                tempData
            );

            if (!string.IsNullOrWhiteSpace(t1))
            {
                redirectUrl = t1;
            }
        
            ResultReturn<string> ret;
            
            try
            {
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

        [ApiExplorerSettings(IgnoreApi = true)]
        [Route("Core/MPCallback/service/{appID}")]
        [HttpGet]
        public async Task<IActionResult> Service([FromServices] IWechatGateway gateway,
            [FromQuery] string signature,
            [FromQuery] string timestamp,
            [FromQuery] string nonce,
            [FromQuery] string echostr,
            [FromRoute] string appID = "",
            [FromServices] ILoggerFactory logger=null
            )
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
                return Content($"failed:{signature},{CheckSignature.GetSignature(timestamp, nonce, config.Token).ToString()}。如果你在浏览器中看到这句话，说明此地址可以被作为微信公众账号后台的Url，请注意保持Token一致。");
            }

        }

        [ApiExplorerSettings(IgnoreApi = true)]
        [Route("Core/MPCallback/service/{appID}")]
        [HttpPost]
        public async Task<IActionResult> ServicePost([FromServices] IWechatGateway gateway,
            [FromQuery] string signature,
            [FromQuery] string timestamp,
            [FromQuery] string nonce,
            [FromQuery] string echostr,
            [FromRoute] string appID = "",
            [FromServices] ILoggerFactory logger=null,
            [FromServices] IWechatMPApi mpApi=null,
            [FromServices]MessageQueue msgHandler=null,
            [FromServices] IMPMessageExecutor messageExecutor=null
            )
        {
            //return Content("");

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
                return Content("该AppID非公众号配置或AppID不存在");
            }

            if (CheckSignature.Check(signature, timestamp, nonce, config.Token))
            {
                return Content("校验无效,请检查token");
            }

            if (messageExecutor==null)
            {
                return Content("success");
            }

            Request.EnableBuffering();

            //自定义MessageHandler，对微信请求的详细判断操作都在这里面。
            var inputStream = Request.Body;
            inputStream.Position = 0;

            var xml = inputStream.ReadToEnd();

            var msg=mpApi.Message.DecodeMPRequestMsg(xml);

            if (msg.IsSuccess)
            {
                if (await msgHandler.AddMessage(msg.ReturnData))
                {
                    var response = await messageExecutor.Execute(msg.ReturnData);

                    if (response==null)
                    {
                        return Content("success");
                    }

                    //if (!string.IsNullOrWhiteSpace(config.EncryptAESKey))
                    //{
                    //    return Content(mpApi.Message.EncryptMessage(appID, response.ToXml()));
                    //}

                    return Content(response.ToXml());
                }
                else
                {
                    return Content("success");
                }
            }
            else
            {
                if (msg.ReturnCode==1000)
                {
                    return  Content("success");
                }

                return Content("error");
            }
            
        }
    }

}
