using System;
using System.Collections.Generic;
using System.Text;

namespace Kugar.WechatSDK.Common
{
    public abstract class WechatConfigurationBase
    {
        public string AppID { set; get; }

        public string AppSerect { set; get; }

        /// <summary>
        /// 该配置是否需要管理AccessToken
        /// </summary>
        public bool ManagerAccessToken { set; get; }
        
        public abstract bool Validate();
    }
}
