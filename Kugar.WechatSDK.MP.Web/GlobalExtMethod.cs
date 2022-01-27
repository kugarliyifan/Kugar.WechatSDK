using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Reflection;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Kugar.Core.Exceptions;
using Kugar.Core.ExtMethod;
using Kugar.Core.Web;
using Kugar.Core.Web.Authentications;
using Kugar.WechatSDK.Common.Gateway;
using Kugar.WechatSDK.MP.Attributes;
using Kugar.WechatSDK.MP.Enums;
using Kugar.WechatSDK.MP.Results;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Linq;
using HttpContext = Microsoft.AspNetCore.Http.HttpContext;

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
            options.AuthenticationScheme = authenticationScheme;
            
            builder.Services.AddOptions().Configure<WechatJWTOption>(authenticationScheme, opt =>
             {
                options.CopyValue(opt);
            });

            //builder.Services.AddScoped(options.LoginService);

            builder.AddJwtBearer(authenticationScheme, (opt) =>
            {
                opt.Events = opt.Events ?? new JwtBearerEvents();

                opt.Events.OnMessageReceived = async (context) =>
                {

                    var authName = context.Scheme.Name;

                    var tmp = (OptionsManager<WechatJWTOption>)context.HttpContext.RequestServices.GetService(
                        typeof(OptionsManager<WechatJWTOption>));

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

                    if (context.Token != null && context.Token.StartsWith("Bearer ", StringComparison.CurrentCultureIgnoreCase))
                    {
                        context.Token = context.Token.Substring(7);
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

                    var loginService =
                        (IWechatJWTAuthenticateService) context.HttpContext.RequestServices.GetService(
                            typeof(IWechatJWTAuthenticateService));

                    if (loginService != null)
                    {
                        var mp = (IWechatMPApi) context.HttpContext.RequestServices.GetService(typeof(IWechatMPApi));
                        
#if NETCOREAPP2_1
                        var openID = (context.Principal.FindFirst("OpenID")?.Value).ToStringEx();
                        var oauth = (context.Principal.FindFirst("OAuthType")?.Value).ToStringEx().ToInt();
                        var appID=(context.Principal.FindFirst("AppID")?.Value).ToStringEx();
#endif
#if NETCOREAPP3_1 || NET5_0
                        var openID = (context.Principal.FindFirstValue("OpenID")).ToStringEx();
                        var oauth = (context.Principal.FindFirstValue("OAuthType")).ToStringEx().ToInt();
                        var appID=(context.Principal.FindFirstValue("AppID")).ToStringEx();
#endif
                        //var loginService = tmpOpt.LoginService;
                           // (IWechatJWTLoginService) context.HttpContext.RequestServices.GetService(options.LoginService);

                        var ret = await loginService.Login(context.HttpContext,appID, openID,(SnsapiType)oauth,mp);
                         
                        if (!ret.IsSuccess)
                        {
                            context.Fail("身份校验错误");

                            return;
                        }
                        //else
                        //{
                        //    context.Principal.AddClaim("UserID", ret.ReturnData);
                        //}

                        //context.Principal.AddClaim("OpenID", openID);
                        //context.Principal.AddClaim("OAuthType", oauth.ToString());
                        //context.Principal.AddClaim("AppID", appID);
                    }


                    context.HttpContext.Items.Remove("SchemeName");
                    context.HttpContext.Items.Add("SchemeName", authName);//.TryGetValue("SchemeName", "")
                    
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

                    var loginUrl = tmpOpt.LoginUrl;

                    var appID = "";

                    var appIDFactory =
                        (IWechatMPAppIdFactory)context.HttpContext.RequestServices.GetService(
                            typeof(IWechatMPAppIdFactory));

                    if (appIDFactory != null)
                    {
                        appID = await appIDFactory?.GetAppId(context, mp);

                        if (string.IsNullOrWhiteSpace(appID))
                        {
                            throw new ArgumentOutOfRangeException("appID", "AppIdFactory返回值必须为有效的appID");
                        }
                    }
                    else
                    {
                        var gateway = (IWechatGateway)context.HttpContext.RequestServices.GetService(typeof(IWechatGateway));

                        var config = gateway.Get<MPConfiguration>();

                        appID = config.AppID;
                    }

                    if (string.IsNullOrWhiteSpace(loginUrl))
                    {
                        loginUrl = $"/Core/MP/Callback/{authName}/{appID}";
                    }

                    if (!loginUrl.StartsWith("http", StringComparison.CurrentCultureIgnoreCase))
                    {
                        loginUrl =
                            $"http{(context.Request.IsHttps ? "s" : "")}://{context.Request.Host.Host}{(context.Request.Host.Port == 80 ? "" : context.Request.Host.Port.ToStringEx())}{(loginUrl.StartsWith('/') ? "" : "/")}{loginUrl}";
                    }

                    var state = Guid.NewGuid().ToString("N").Left(16);

                    WechatMPAuthorizeAttribute attr = null;
#if NETCOREAPP2_1
                        attr = _cacheMPAuthorizeAttr.GetOrAdd(context.HttpContext.GetRequestMethodInfo(),
                            getAttrbuteFromMethod);
                        
#endif
#if NETCOREAPP3_1 || NET5_0
                    var endPoint = context.HttpContext.GetEndpoint();

                    attr = endPoint.Metadata.GetMetadata<WechatMPAuthorizeAttribute>();
#endif


                    loginUrl = mp.OAuth.BuildOAuthUrl(appID, loginUrl, attr.OAuthType, state);

                    if (tmpOpt.OnChallenge != null)
                    {
                        await tmpOpt.OnChallenge(context,mp,loginUrl);
                    }
                    
                    if(!context.Handled)
                    {
                        var cache = (IMemoryCache) context.HttpContext.RequestServices.GetService(typeof(IMemoryCache));
                        
                        var loginService= (IWechatJWTAuthenticateService)context.HttpContext.RequestServices.GetService(typeof(IWechatJWTAuthenticateService));

                        var tempData =await loginService.OnBeforeLoginTempData(context.HttpContext, appID, loginUrl, mp);
                         
                        var entity=cache.GetOrCreate<string>(state, x =>
                        {
                            x.AbsoluteExpiration = DateTimeOffset.Now.AddMinutes(5);

                            var value = new JObject()
                            {
                                ["redirectUrl"] = context.Request.GetDisplayUrl(),
                                ["oauthType"] = (int) attr.OAuthType,
                                ["scheme"] = authName,
                                ["tempData"]= tempData
                            }.ToStringEx();

                            x.Value = value;

                            return value;
                        });
                 

                        if (!cache.TryGetValue(state, out var t1))
                        {
                            throw new Exception("无法获取缓存");
                        }

                        context.Response.Redirect(loginUrl);
                        
                        context.HandleResponse();
                    }
                };
                
            });

            return builder;
        }

        public static AuthenticationBuilder AddWchatMPJWT(this AuthenticationBuilder builder,
            string authenticationScheme,
            WechatJWTOption options) => AddWchatMPJWT(builder,authenticationScheme, authenticationScheme, options);

        /// <summary>
        /// 注册一个AppID获取类,用于多AppID的情况,并且AppID为运行时才可以获得的情况,比如从数据库中读取
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection RegisterMPFactory<T>(this IServiceCollection services) where T :class, IWechatMPAppIdFactory
        {
            services.AddScoped<IWechatMPAppIdFactory,T>();

            return services;
        }

        /// <summary>
        /// 注册一个微信公众号JWT用户登录验证的服务
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection RegisterMPJWTLoginService<T>(this IServiceCollection services)
            where T : class, IWechatJWTAuthenticateService
        {
            services.AddScoped<IWechatJWTAuthenticateService,T>();

            services.AddScoped<IHttpContextAccessor>();

            return services;
        }

#if NETCOREAPP2_1

        private static ConcurrentDictionary<MethodInfo, WechatMPAuthorizeAttribute> _cacheMPAuthorizeAttr =
            new ConcurrentDictionary<MethodInfo, WechatMPAuthorizeAttribute>();

        public static WechatMPAuthorizeAttribute getAttrbuteFromMethod(MethodInfo method)
        {
            Debugger.Break();
            var c = method.GetCustomAttribute<WechatMPAuthorizeAttribute>();

            if (c!=null)
            {
                return c;
            }

            var c1=getAttributeFromController(method.DeclaringType);

            return c1;
        }

        private static WechatMPAuthorizeAttribute getAttributeFromController(Type controllerType)
        {
            var c = controllerType.GetCustomAttribute<WechatMPAuthorizeAttribute>();

            if (c!=null)
            {
                return c;
            }
            else if(controllerType==typeof(Controller) || controllerType==typeof(ControllerBase) || controllerType==typeof(object))
            {
                return null;
            }
            else
            {
                return getAttributeFromController(controllerType);
            }
        }

#endif

        /// <summary>
        /// 生成微信授权使用的Token,只能是配合WechatSDK使用
        /// </summary>
        /// <param name="controller"></param>
        /// <param name="appId"></param>
        /// <param name="openId"></param>
        /// <param name="type"></param>
        /// <param name="values"></param>
        /// <returns></returns>
        public static string BuildWechatJWtToken(this ControllerBase controller, string appId, string openId,SnsapiType type, params (string key, string value)[] values)
        {
            var optManager =
                (OptionsManager<WechatJWTOption>)controller.HttpContext.RequestServices.GetService(
                    typeof(OptionsManager<WechatJWTOption>));

            //var provider = (IAuthenticationSchemeProvider)Request.HttpContext.RequestServices.GetService(typeof(IAuthenticationSchemeProvider));

            //var scheme = provider.GetAllSchemesAsync().Result;

            //var scheme= (string)HttpContext.Current.Items.TryGetValue("SchemeName");

            var option = optManager.Get(GetCurrentSchemeName(controller));

            return BuildWechatJWtToken(controller, appId, openId,type, values, option, null);
        }

        /// <summary>
        /// 生成微信授权使用的Token,只能是配合WechatSDK使用
        /// </summary>
        /// <param name="controller"></param>
        /// <param name="appId"></param>
        /// <param name="openId"></param>
        /// <param name="type"></param>
        /// <param name="expiredSpan">过期时间,默认30天</param>
        /// <param name="values"></param>
        /// <returns></returns>
        public static string BuildWechatJWtToken(this ControllerBase controller, string appId, string openId, SnsapiType type, TimeSpan? expiredSpan = null, params (string key, string value)[] values)
        {
            var optManager =
                (OptionsManager<WechatJWTOption>)controller.HttpContext.RequestServices.GetService(
                    typeof(OptionsManager<WechatJWTOption>));

            //var provider = (IAuthenticationSchemeProvider)Request.HttpContext.RequestServices.GetService(typeof(IAuthenticationSchemeProvider));

            //var scheme = provider.GetAllSchemesAsync().Result;

            //var scheme= (string)HttpContext.Current.Items.TryGetValue("SchemeName");

            var option = optManager.Get(GetCurrentSchemeName(controller));

            return BuildWechatJWtToken(controller, appId, openId, type, values, option, expiredSpan);
        }

        /// <summary>
        /// 生成微信授权使用的Token,只能是配合WechatSDK使用
        /// </summary>
        /// <param name="controller"></param>
        /// <param name="appId"></param>
        /// <param name="openId"></param>
        /// <param name="type"></param>
        /// <param name="values"></param>
        /// <param name="option"></param>
        /// <param name="expiredSpan"></param>
        /// <returns></returns>
        public static string BuildWechatJWtToken(this ControllerBase controller, string appId, string openId, SnsapiType type, (string key, string value)[] values,
            WechatJWTOption option, TimeSpan? expiredSpan = null)
        {
            var tokenHandler = new JwtSecurityTokenHandler();

            var authTime = DateTime.UtcNow;
            var expiresAt = authTime.Add(expiredSpan ?? option.ExpireTimeSpan);

            var lst = new List<Claim>()
            {
                new Claim("aud", option.Audience),
                new Claim("iss", option.Issuer),
                //new Claim(ClaimTypes.NameIdentifier, openId.DesEncrypt(option.TokenEncKey.Left(8))),
                new Claim("AppID",appId),
                new Claim("OpenID",openId),
                new Claim("OAuthType",((int)type).ToStringEx())
                //new Claim("k", password.DesEncrypt(option.TokenEncKey.Left(8))),
            };

            if (values.HasData())
            {
                foreach (var item in values)
                {
                    lst.Add(new Claim(item.key, item.value));
                }
            }

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(lst),
                Expires = expiresAt,
                SigningCredentials =
                    new SigningCredentials(new SymmetricSecurityKey(option.ActualEncKey),
                        SecurityAlgorithms.HmacSha256Signature),
                EncryptingCredentials = new EncryptingCredentials(new SymmetricSecurityKey(option.ActualEncKey),
                    JwtConstants.DirectKeyUseAlg, SecurityAlgorithms.Aes256CbcHmacSha512)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = tokenHandler.WriteToken(token);

            return tokenString;
        }

        public static string GetCurrentSchemeName(this ControllerBase controller) =>
            (string)controller.HttpContext.Items.TryGetValue("SchemeName");
    }



    public class WechatJWTOption :WebJWTOptionBase
    {
        private static TimeSpan _defaultExpireTimeSpan=TimeSpan.FromDays(30);
        private TimeSpan _expireTimeSpan= _defaultExpireTimeSpan;
        private static readonly string _defaultToken= "0O9W6eOHVmooTnYT";
        private static readonly byte[] _defaultActualEncKey = Encoding.UTF8.GetBytes(_defaultToken.PadRight(128, '0'));

        public WechatJWTOption()
        {
            this.AuthenticationScheme = "wechat";
        }

        ///// <summary>
        ///// 用于登录验证的服务接口,必须是 IWechatJWTLoginService
        ///// </summary>
        //public IWechatJWTAuthenticateService LoginService { set; get; }

        /// <summary>
        /// 授权名称
        /// </summary>
        public override string AuthenticationScheme { set; get; } = "wechat";
        
        /// <summary>
        /// token校验成功后,触发该回调,如果回调中,需要登录失败,调用context的Fail函数,会触发OnChallenge回调
        /// </summary>
        public Func<Microsoft.AspNetCore.Authentication.JwtBearer.TokenValidatedContext,IWechatMPApi, Task> OnTokenValidated { set; get; }

        /// <summary>
        /// 登录失败时,触发该回调,如需要触发跳转,使用context.Response.Redirect,后使用context.HandleResponse()中止后续处理
        /// </summary>
        public ChallengeHandler OnChallenge { set; get; } 
        public override string Issuer { set; get; } = "wechatlogin";

        public override string Audience { set; get; } = "web";
        
        /// <summary>
        /// 登录失败时,回调的委托
        /// </summary>
        /// <param name="context">登录失败的Context</param>
        /// <param name="mp">微信公众号Api接口</param>
        /// <param name="loginUrl">默认生成的登录地址</param>
        /// <returns></returns>
        public delegate Task ChallengeHandler(JwtBearerChallengeContext context, IWechatMPApi mp, string loginUrl);
    }
     

    ///// <summary>
    ///// 授权返回后自动回调该函数,用于自动添加用户时可用
    ///// </summary>
    ///// <param name="context"></param>
    ///// <param name="appID"></param>
    ///// <param name="refresh_token">用于刷新用户accesstoken的refreshToken</param>
    ///// <param name="accessToken">用户的accesstoken</param>
    ///// <param name="userInfo">用户信息,只有在授权方式为userinfo的时候,才可用,否则为null</param>
    ///// <param name="mpApi">注入的IWechatMPApi接口</param>
    ///// <returns></returns>
    //public delegate Task AfterMPOAuthCallback(HttpContext context, string appID,string openID,string refresh_token,string accessToken,
    //    WxUserInfo_Result userInfo, IWechatMPApi mpApi);

    ///// <summary>
    ///// 用于根据context,返回对应的appid
    ///// </summary>
    ///// <param name="context"></param>
    ///// <param name="mpApi"></param>
    ///// <returns></returns>
    //public delegate Task<string> MPAppIDFactory(Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerChallengeContext context, IWechatMPApi mpApi);
}
