using System;
using System.Collections.Generic;
using System.Text;
using Kugar.WechatSDK.Common;
using Kugar.WechatSDK.Common.Gateway;
using Microsoft.Extensions.DependencyInjection;

namespace Kugar.WechatSDK.MP
{
    public static class GlobalExtMethod
    {
        public static IServiceCollection AddWechatMP(this IServiceCollection services,params MPConfiguration[] configurations)
        {
            services.AddSingleton<IOAuthService, OAuthService>();
            services.AddSingleton<IJsTicketContainer, JsTicketContainer>();
            services.AddSingleton<MenuService>();
            services.AddSingleton<UIService>();

            services.AddSingleton<IWechatMPApi, WechatMPApi>(x =>
            {
                var gateWay = (IWechatGateway) x.GetService(typeof(IWechatGateway));
                var accessTokenContainer = (IAccessTokenContainer) x.GetService(typeof(IAccessTokenContainer));
                var jsTicketContainer = (JsTicketContainer) x.GetService(typeof(JsTicketContainer));


                foreach (var item in configurations)
                {
                    gateWay.Add(item);

                    if (item.ManagerAccessToken)
                    {
                        accessTokenContainer.Register(item.AppID, item.AppSerect);    
                        jsTicketContainer.Remove(item.AppID);
                    }
                }

                return new WechatMPApi((MenuService) x.GetService(typeof(MenuService)),
                    (IOAuthService) x.GetService(typeof(OAuthService)),
                    (UIService) x.GetService(typeof(UIService))
                );
            });
            

            return services;
        }
    }
}
