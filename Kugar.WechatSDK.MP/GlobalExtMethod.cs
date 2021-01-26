using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Kugar.WechatSDK.Common;
using Kugar.WechatSDK.Common.Gateway;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Kugar.WechatSDK.MP
{
    public static class GlobalExtMethod
    {
        public static IServiceCollection AddWechatMP(this IServiceCollection services,params MPConfiguration[] configurations)
        {
            Debugger.Break();
            
            services.AddSingleton(typeof(OptionsManager<>));
            services.AddSingleton<IOAuthService, OAuthService>();
            services.AddSingleton<IJsTicketContainer, JsTicketContainer>();
            //services.AddSingleton<MenuService>();
            services.AddSingleton<IUIService,UIService>();

            services.AddSingleton<IWechatMPApi, WechatMPApi>(x =>
            {
                var gateWay = (IWechatGateway) x.GetService(typeof(IWechatGateway));
                var accessTokenContainer = (IAccessTokenContainer) x.GetService(typeof(IAccessTokenContainer));
                var jsTicketContainer = (IJsTicketContainer) x.GetService(typeof(IJsTicketContainer));

                Debugger.Break();
                
                foreach (var item in configurations)
                {
                    gateWay.Add(item);

                    if (item.ManagerAccessToken)
                    {
                        accessTokenContainer.Register(item.AppID, item.AppSerect);    
                        jsTicketContainer.Register(item.AppID);
                    }
                }

                return new WechatMPApi(null,/*(MenuService) x.GetService(typeof(MenuService)),*/
                    (IOAuthService) x.GetService(typeof(IOAuthService)),
                    (IUIService) x.GetService(typeof(IUIService))
                );
            });
            

            return services;
        }
    }
}
