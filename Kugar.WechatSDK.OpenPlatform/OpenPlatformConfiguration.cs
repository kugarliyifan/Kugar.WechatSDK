using System;
using Kugar.WechatSDK.Common;

namespace Kugar.WechatSDK.OpenPlatform
{
    public class OpenPlatformConfiguration:WechatConfigurationBase
    {
        public string Token { set; get; }

        /// <summary>
        /// 加密秘钥
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
}
