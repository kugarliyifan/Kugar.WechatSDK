using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Kugar.WechatSDK.Common;

namespace Kugar.WechatSDK.MP
{
    public class MPConfiguration:WechatConfigurationBase
    {
        

        public string Token { set; get; }

        /// <summary>
        /// 微信公众号消息推送的加密秘钥
        /// </summary>
        public string EncTokenKey { set; get; }
        
        public override bool Validate()
        {
            return true;
        }
        
    }
}
