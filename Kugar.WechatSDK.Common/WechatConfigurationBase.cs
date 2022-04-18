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

        /// <summary>
        /// 当ManagerAccessToken为false时,调用该属性触发获取指定AppId的AccessToken,,一般用于当一个公众号需要对接多个站点时,由独立的程序管理AccessToken,并提供给其他程序使用
        /// </summary>
        public AccessTokenFactory AccessTokenFactory { set; get; }



        public abstract bool Validate();
    }

    
}
