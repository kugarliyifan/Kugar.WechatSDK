using System;
using System.Collections.Generic;
using System.Text;
using Kugar.WechatSDK.Common;
using Kugar.WechatSDK.Common.Gateway;
using Microsoft.Extensions.DependencyInjection;

namespace Kugar.WechatSDK.MiniProgram
{
    public static class GlobalExtMethod
    {
        public static IServiceCollection AddWechatMiniProgram(this IServiceCollection services,params MiniProgramConfiguration[] configurations)
        {
            services.AddSingleton<IUrlScheme, UrlScheme>();
            services.AddSingleton<ISNS, SNS>();
            services.AddSingleton<IQrCode, QrCode>();
            services.AddSingleton<ISubscribeMessage, SubscribeMessage>();
            services.AddSingleton<IMiniProgramApi, MiniProgramApi>(x =>
            {
                var gateWay = (IWechatGateway) x.GetService(typeof(IWechatGateway));
                var accessTokenContainer = (IAccessTokenContainer) x.GetService(typeof(IAccessTokenContainer));

                foreach (var item in configurations)
                {
                    gateWay.Add(item);

                    if (item.ManagerAccessToken)
                    {
                        accessTokenContainer.Register(item.AppID, item.AppSerect);    
                    }
                }

                return new MiniProgramApi((ISubscribeMessage) x.GetService(typeof(ISubscribeMessage)),
                    (IQrCode) x.GetService(typeof(IQrCode)),
                    (ISNS) x.GetService(typeof(ISNS)),
                    (IUrlScheme) x.GetService(typeof(IUrlScheme))
                );
            });
            

            return services;
        }
        
    }
}
