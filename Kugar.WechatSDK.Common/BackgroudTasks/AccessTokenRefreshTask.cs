using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Kugar.Core.Services;
using Microsoft.Extensions.Logging;

namespace Kugar.WechatSDK.Common.BackgroudTasks
{
    /// <summary>
    /// 定时自动检查AccessToken是否有效,无效时,自动刷新
    /// </summary>
    public class AccessTokenRefreshTask:TimerHostedService
    {
        public AccessTokenRefreshTask(IServiceProvider provider) : base(provider)
        {
        }

        protected override async Task Run(IServiceProvider serviceProvider, CancellationToken stoppingToken)
        {
            var accessTokenContainer=(IAccessTokenContainer)serviceProvider.GetService(typeof(IAccessTokenContainer));

            try
            {
                foreach (var item in accessTokenContainer.GetAllTokens())
                {
                    if (!await accessTokenContainer.CheckAccessToken(item.appID))
                    {
                        await accessTokenContainer.RefreshAccessToken(item.appID);
                    }    
                }
                
            }
            catch (Exception e)
            {
                var logger = (ILoggerFactory) serviceProvider.GetService(typeof(ILoggerFactory));

                logger?.CreateLogger("wechat").Log(LogLevel.Error,e.Message);
            }
            
        }

        protected override int Internal { get; } = 20000;
    }
}
