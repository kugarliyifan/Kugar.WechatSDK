using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Kugar.WechatSDK.Common;
using Microsoft.AspNetCore.Http;

namespace Kugar.WechatSDK.MP
{
    public class MPConfiguration:WechatConfigurationBase
    {
        public string Token { set; get; }

        /// <summary>
        /// 当ManagerAccessToken为false时,调用该属性触发获取指定AppId的AccessToken,,一般用于当一个公众号需要对接多个站点时,由独立的程序管理AccessToken,并提供给其他程序使用
        /// </summary>
        public AccessTokenFactory JsTicketFactory { set; get; }

        /// <summary>
        /// 微信公众号消息推送的加密秘钥
        /// </summary>
        public string EncryptAESKey { set; get; }
        
        public override bool Validate()
        {
            return true;
        }
    }

    //public class MPRequestHostOption
    //{
    //    public string MPApiHost { set; get; }

        
    //}

    //public class WchatWebConfiguration
    //{
    //    public Func<IHttpContextAccessor> HttpContxtGetter { set; get; }
    //}
}
