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
        public static IServiceCollection AddWechatGateway(this IServiceCollection services,string wechatApiHost="https://api.weixin.qq.com")
        {
            services.AddHttpClient("MPApi")
                .SetHandlerLifetime(TimeSpan.FromSeconds(10))
                .AddPolicyHandler(GetRetryPolicy());
            services.AddOptions<WechatRequestOption>().Configure(x=>new WechatRequestOption(){BaseApiHost = wechatApiHost});
            services.AddSingleton<HttpRequestHelper>();
            services.AddSingleton<IAccessTokenContainer, AccessTokenContainer>();
            services.AddSingleton<IWechatGateway,WechatGateway>();
            services.AddSingleton<ICommonApi,CommonApi>();

            services.AddHostedService<AccessTokenRefreshTask>();

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
