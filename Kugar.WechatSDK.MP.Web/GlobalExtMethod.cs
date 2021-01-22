using System;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Kugar.Core.ExtMethod;
using Kugar.Core.Web;
using Kugar.Core.Web.Authentications;
using Kugar.WechatSDK.MP.Results;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Kugar.WechatSDK.MP.Web
{
    public static class GlobalExtMethod
    {
        /// <summary>
        /// 添加一个jwt方式的授权验证,请配合IJWTLoginControlle接口,方便使用,将使用options.cookie.name或者headers中的Authorization 作为token的获取来源<br/>
        /// 因此,可以通用于webapi和web页面进行授权验证
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="authenticationScheme">授权名称</param>
        /// <param name="displayName">授权名称</param>
        /// <param name="options">配置项</param>
        /// <returns></returns>
        public static AuthenticationBuilder AddWchatMPJWT(this AuthenticationBuilder builder,
            string authenticationScheme,
            string displayName,
            WechatJWTOption options)
        {
            if (options.LoginService == null)
            {
                throw new ArgumentNullException("Options.LoginService不能为空");
            }
            
            builder.Services.AddSingleton(typeof(OptionsManager<>));

            builder.Services.AddOptions().Configure<WechatJWTOption>(authenticationScheme, opt =>
             {
                options.CopyValue(opt);
            });

            builder.AddJwtBearer(authenticationScheme, (opt) =>
            {
                opt.Events = opt.Events ?? new JwtBearerEvents();

                opt.Events.OnMessageReceived = async (context) =>
                {

                    var authName = context.Scheme.Name;

                    var tmp = (OptionsManager<WebJWTOption>)context.HttpContext.RequestServices.GetService(
                        typeof(OptionsManager<WebJWTOption>));

                    context.HttpContext.Items.Remove("SchemeName");
                    context.HttpContext.Items.Add("SchemeName", authName);//.TryGetValue("SchemeName", "")

                    var option = tmp.Get(authName);

                    if (context.HttpContext.Request.Cookies.TryGetValue(string.IsNullOrEmpty(option.Cookie?.Name) ? $"jwt.{authName}" : option.Cookie?.Name,
                        out var v))
                    {
                        context.Token = v;
                    }

                    if (string.IsNullOrEmpty(context.Token) && context.Request.Headers.ContainsKey("Authorization"))
                    {
                        context.Token = context.Request.Headers.TryGetValue("Authorization").FirstOrDefault();
                    }

                    if (string.IsNullOrWhiteSpace(context.Token))
                    {
                        context.Fail("未包含token");
                    }

                };


                opt.TokenValidationParameters = new TokenValidationParameters()
                {
                    ValidateIssuer = true,
                    ValidIssuer = options.Issuer ?? options.AuthenticationScheme,
                    ValidateAudience = true,
                    ValidAudience = options.Audience ?? options.AuthenticationScheme,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(options.ActualEncKey),
                    TokenDecryptionKey = new SymmetricSecurityKey(options.ActualEncKey),
                };

                opt.Events.OnTokenValidated = async (context) =>
                {

                    var authName = context.Scheme.Name;

                    var t = (OptionsManager<WechatJWTOption>)context.HttpContext.RequestServices.GetService(
                        typeof(OptionsManager<WechatJWTOption>));

                    var tmpOpt = t.Get(authName);


                    if (tmpOpt.LoginService != null)
                    {
#if NETCOREAPP2_1
                        var userName = (context.Principal.FindFirst(ClaimTypes.NameIdentifier)?.Value).ToStringEx();
                        var pw = (context.Principal.FindFirst("k")?.Value).ToStringEx();
#endif
#if NETCOREAPP3_0 || NETCOREAPP3_1 || NET5_0
                        var userName = (context.Principal.FindFirstValue(ClaimTypes.NameIdentifier)).ToStringEx();
                        var pw = (context.Principal.FindFirstValue("k")).ToStringEx();
#endif
                        if (!string.IsNullOrWhiteSpace(pw))
                        {
                            pw=pw.DesDecrypt(tmpOpt.TokenEncKey.Left(8));
                        }

                        userName = userName.Trim();
                        pw = pw.Trim();

                        var ret = await tmpOpt.LoginService.Login(context.HttpContext, userName, pw);
                         
                        if (!ret.IsSuccess)
                        {
                            context.Fail("身份校验错误");

                            return;
                        }
                        else
                        {
                            context.Principal.AddClaim("userID", ret.ReturnData);
                        }
                    }

                    context.HttpContext.Items.Remove("SchemeName");
                    context.HttpContext.Items.Add("SchemeName", authName);//.TryGetValue("SchemeName", "")


                    //HttpContext.Current.Items.Add("SchemeName", authenticationScheme);//.TryGetValue("SchemeName", "")

                    if (tmpOpt.OnTokenValidated != null)
                    {
                        var mp = (IWechatMPApi) context.HttpContext.RequestServices.GetService(typeof(IWechatMPApi));

                        await tmpOpt.OnTokenValidated(context,mp);
                    }
                };

                opt.Events.OnChallenge = async (context) =>
                {
                    var authName = context.Scheme.Name;

                    var t = (OptionsManager<WechatJWTOption>)context.HttpContext.RequestServices.GetService(
                        typeof(OptionsManager<WechatJWTOption>));

                    var tmpOpt = t.Get(authName);

                    context.HttpContext.Items.Remove("SchemeName");
                    context.HttpContext.Items.Add("SchemeName", authName);//.TryGetValue("SchemeName", "")

                    var mp = (IWechatMPApi) context.HttpContext.RequestServices.GetService(typeof(IWechatMPApi));

                    if (tmpOpt.OnChallenge != null)
                    {
                        await tmpOpt.OnChallenge(context,mp);
                    }
                    
                    if(!context.Handled)
                    {
                        var loginUrl = tmpOpt.LoginUrl;

                        if (string.IsNullOrWhiteSpace(loginUrl))
                        {
                            var appID =await options.AppIdFactory?.Invoke(context, mp);

                            loginUrl = $"/Core/MP/Callback/{appID}";
                        }

                        if (loginUrl.StartsWith("http",StringComparison.CurrentCultureIgnoreCase))
                        {
                            loginUrl =
                                $"http{(context.Request.IsHttps ? "s" : "")}://{context.Request.Host.Host}{(context.Request.Host.Port == 80 ? "" : context.Request.Host.Port.ToStringEx())}{(loginUrl.StartsWith('/') ? "" : "/")}{loginUrl}";
                        }

                        context.Response.Redirect(loginUrl);
                        
                        context.HandleResponse();
                    }
                };

                //opt.Events.OnAuthenticationFailed = async (context) =>
                //{
                //    var authName = context.Scheme.Name;

                //    var t = (OptionsManager<WebJWTOption>)context.HttpContext.RequestServices.GetService(
                //        typeof(OptionsManager<WebJWTOption>));

                //    var tmpOpt = t.Get(authName);

                //    if (string.IsNullOrWhiteSpace(tmpOpt.LoginUrl))
                //    {
                //        context.Response.Redirect($"/AdminCore/Logout/{authenticationScheme}?backurl=" + context.Request.GetDisplayUrl());
                //    }
                //    else
                //    {
                //        context.Response.Redirect($"{tmpOpt.LoginUrl}?backurl={context.Request.GetDisplayUrl()}");
                //    }
                //};
            });

            return builder;
        }

        public static AuthenticationBuilder AddWchatMPJWT(this AuthenticationBuilder builder,
            string authenticationScheme,
            WechatJWTOption options) => AddWchatMPJWT(builder,authenticationScheme, authenticationScheme, options);
    }

    public class WechatJWTOption
    {
        private static TimeSpan _defaultExpireTimeSpan=TimeSpan.FromDays(30);
        private TimeSpan _expireTimeSpan= _defaultExpireTimeSpan;
        private static readonly string _defaultToken= "0O9W6eOHVmooTnYT";
        private static readonly byte[] _defaultActualEncKey = Encoding.UTF8.GetBytes(_defaultToken.PadRight(128, '0'));


        /// <summary>
        /// 构建cookie的配置
        /// </summary>
        public CookieBuilder Cookie { set; get; } = new CookieBuilder();

        /// <summary>
        /// 用于登录验证的服务接口
        /// </summary>
        public IWebJWTLoginService LoginService { get; set; }

        /// <summary>
        /// 授权名称
        /// </summary>
        public string AuthenticationScheme {internal set; get; } = "web";

        /// <summary>
        /// 过期时间,默认为30天
        /// </summary>
        public TimeSpan ExpireTimeSpan
        {
            set => _expireTimeSpan = value;
            get => _expireTimeSpan;
        }

        /// <summary>
        /// token校验成功后,触发该回调,如果回调中,需要登录失败,调用context的Fail函数,会触发OnChallenge回调
        /// </summary>
        public Func<TokenValidatedContext,IWechatMPApi, Task> OnTokenValidated { set; get; }

        /// <summary>
        /// 登录失败时,触发该回调,如需要触发跳转,使用context.Response.Redirect,后使用context.HandleResponse()中止后续处理
        /// </summary>
        public Func<JwtBearerChallengeContext,IWechatMPApi, Task> OnChallenge { set; get; }

        public AfterMPOAuthCallback AfterOAuth { set; get; }

        public MPAppIDFactory AppIdFactory { set; get; }

        private string _tokenEncKey= _defaultToken;
        private byte[] _actualEncKey= _defaultActualEncKey;


        public string Issuer { set; get; } = "wechatlogin";

        public string Audience { set; get; } = "web";

        /// <summary>
        /// token加密的秘钥,默认为 0O9W6eOHVmooTnYT 的固定密码,请尽量修改该值
        /// </summary>
        public string TokenEncKey
        {
            set
            {
                _tokenEncKey = value;
                _actualEncKey = Encoding.UTF8.GetBytes(value.PadRight(128, '0'));
            }
            get => _tokenEncKey;
        }

        /// <summary>
        /// 登录地址,如需设置登陆跳转界面,这需要设置该跳转地址,如不设置,授权失败后,会跳转 /Core/MP/Callback/{AppID}
        /// </summary>
        public string LoginUrl { set; get; }

        /// <summary>
        /// 实际使用的EncKey,不直接使用
        /// </summary>
        public byte[] ActualEncKey
        {
            get => _actualEncKey;
            private set => _actualEncKey = value;
        }
    }

    public delegate Task AfterMPOAuthCallback(JwtBearerChallengeContext context, string appID,
        WxUserInfo_Result userInfo, IWechatMPApi mpApi);

    public delegate Task<string> MPAppIDFactory(JwtBearerChallengeContext context, IWechatMPApi mpApi);
}
