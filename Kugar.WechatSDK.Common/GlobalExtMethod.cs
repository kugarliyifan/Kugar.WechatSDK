using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using Kugar.Core.BaseStruct;
using Kugar.WechatSDK.Common.BackgroudTasks;
using Kugar.WechatSDK.Common.Gateway;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.Extensions.Http;

namespace Kugar.WechatSDK.Common
{
    public static class GlobalExtMethod
    {
        public static IServiceCollection AddWechatGateway(this IServiceCollection services,string wechatApiHost="https://api.weixin.qq.com",string mpApiHost="https://mp.weixin.qq.com")
        {
            services.AddHttpClient("MPApi")
                .SetHandlerLifetime(TimeSpan.FromSeconds(10))
                .AddPolicyHandler(GetRetryPolicy());
            services.AddOptions<WechatRequestOption>().Configure(x =>
            {
                x.BaseApiHost = wechatApiHost;
                x.MPApiHost = mpApiHost;
            });
            services.AddScoped<HttpRequestHelper>();
            services.AddSingleton<IAccessTokenContainer, AccessTokenContainer>();
            services.AddSingleton<IWechatGateway,WechatGateway>();
            services.AddScoped<ICommonApi,CommonApi>();

            services.AddHostedService<AccessTokenRefreshTask>();

            return services;
        }

        /// <summary>
        /// 注册一个能从外部获取AccessToken的接口类,如果由本类库自动管理,可无需注册该类,Scoped方式
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection RegisterAccessTokenFactory<T>(this IServiceCollection services)
            where T : class, IAccessTokenFactory
        {
            services.AddScoped<IAccessTokenFactory, T>();

            return services;
        }

        static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
        {
            return HttpPolicyExtensions
                .HandleTransientHttpError()
                .OrResult(msg => msg.StatusCode == System.Net.HttpStatusCode.NotFound)
                .WaitAndRetryAsync(6, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2,
                    retryAttempt)) + + TimeSpan.FromMilliseconds(RandomEx.Next(0,100))
                
                );
        }
    }



}
