using System;
using System.Collections.Generic;
using System.Text;
using Kugar.WechatSDK.Common;
using Kugar.WechatSDK.Common.Gateway;
using Kugar.WechatSDK.OpenPlatform.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Kugar.WechatSDK.OpenPlatform
{
    public static class GlobalExtMethod
    {
        public static IServiceCollection AddOpenPlatform(this IServiceCollection services,
            params OpenPlatformConfiguration[] configurations)
        {
            services.AddSingleton<IOAuthService, OAuthService>()
                .AddSingleton<ISubscriptionMsgService, SubscriptionMsgService>();

            //services.AddOptions<WechatRequestOption>().Configure(x => x.MPApiHost = mpApiHost);

            services.AddSingleton<IWechatOpenPlatformApi, WechatOpenPlatformApi>(x =>
            {
                var gateWay = (IWechatGateway) x.GetService(typeof(IWechatGateway));
                var accessTokenContainer = (IAccessTokenContainer) x.GetService(typeof(IAccessTokenContainer));
                var sdkTicketContainer = (ISdkTicketContrainer) x.GetService(typeof(ISdkTicketContrainer));
                
                foreach (var item in configurations)
                {
                    gateWay.Add(item);

                    if (item.ManagerAccessToken)
                    {
                        accessTokenContainer.Register(item.AppID, item.AppSerect);    
                        sdkTicketContainer.Register(item.AppID);
                    }
                }
                
                return new WechatOpenPlatformApi((IOAuthService) x.GetService(typeof(IOAuthService)),
                    (ISubscriptionMsgService) x.GetService(typeof(ISubscriptionMsgService))
                    );
                
            });

            return services;
        }
    }
}
