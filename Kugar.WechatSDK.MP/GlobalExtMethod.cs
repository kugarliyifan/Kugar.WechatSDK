using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Kugar.WechatSDK.Common;
using Kugar.WechatSDK.Common.Gateway;
using Kugar.WechatSDK.MP.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Kugar.WechatSDK.MP
{
    public static class GlobalExtMethod
    {
        public static IServiceCollection AddWechatMP(
            this IServiceCollection services,
            params MPConfiguration[] configurations)
        {
            services.AddSingleton(typeof(OptionsManager<>));
            services.AddScoped<IHttpContextAccessor>();
            //services.AddOptions<MPRequestHostOption>().Configure(x => x.MPApiHost = mpApiHost);
            services.AddSingleton<IOAuthService, OAuthService>();
            services.AddSingleton<IJsTicketContainer, JsTicketContainer>();
            services.AddSingleton<IMPMessageCache, DefaultMPMessageCache>();
            //services.AddSingleton<MenuService>();
            services.AddSingleton<IUIService,UIService>();
            services.AddSingleton<IMaterialService, MaterialService>();
            services.AddSingleton<IBrocastMsgService, BrocastMsgService>();
            services.AddSingleton<ITemplateMsgService, TemplateMsgService>();
            services.AddSingleton<ISubscriptionMsgService, SubscriptionMsgService>();
            services.AddSingleton<IUserTagManagementService, UserTagManagementService>();
            services.AddSingleton<IUserManagementService, UserManagementService>();
            services.AddSingleton<IQrCodeService, QrCodeService>();
            services.AddSingleton<IUrlService, UrlService>();
            services.AddSingleton<IMenuService, MenuService>();
            services.AddSingleton<IMessageService,MessageService>();
            services.AddSingleton<KFManagementService>();
            services.AddSingleton<MessageQueue>();

            services.AddSingleton<IWechatMPApi, WechatMPApi>(x =>
            {
                var gateWay = (IWechatGateway) x.GetService(typeof(IWechatGateway));
                var accessTokenContainer = (IAccessTokenContainer) x.GetService(typeof(IAccessTokenContainer));
                var jsTicketContainer = (IJsTicketContainer) x.GetService(typeof(IJsTicketContainer));
                
                foreach (var item in configurations)
                {
                    gateWay.Add(item);

                    if (item.ManagerAccessToken)
                    {
                        accessTokenContainer.Register(item.AppID, item.AppSerect);    
                        jsTicketContainer.Register(item.AppID);
                    }
                }
                
                return new WechatMPApi((IMenuService) x.GetService(typeof(IMenuService)),
                    (IOAuthService) x.GetService(typeof(IOAuthService)),
                    (IUIService) x.GetService(typeof(IUIService)),
                    (IMessageService)x.GetService(typeof(IMessageService)),
                    (IMaterialService)x.GetService(typeof(IMaterialService)),
                    (IBrocastMsgService)x.GetService(typeof(IBrocastMsgService)),
                    (ITemplateMsgService)x.GetService(typeof(ITemplateMsgService)),
                    (ISubscriptionMsgService)x.GetService(typeof(ISubscriptionMsgService)),
                    (IUserManagementService)x.GetService(typeof(IUserManagementService)),
                    (IQrCodeService)x.GetService(typeof(IQrCodeService)),
                    (IUrlService)x.GetService(typeof(IUrlService)),
                    (KFManagementService)x.GetService(typeof(KFManagementService))
                );
            });
            

            return services;
        }
    }
}
