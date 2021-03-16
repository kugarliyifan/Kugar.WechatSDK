using System;
using System.Collections.Generic;
using System.Text;
using Kugar.WechatSDK.Common;

namespace Kugar.WechatSDK.MiniProgram
{
    /// <summary>
    /// 小程序配置
    /// </summary>
    public class MiniProgramConfiguration:WechatConfigurationBase
    {
        public MiniProgramConfiguration(string appID,string appSerect,string token, bool isManagerAccessToken=true)
        {
            this.AppID = appID;
            this.AppSerect = appSerect;
            this.Token = token;
            this.ManagerAccessToken = isManagerAccessToken;
        }

        public string Token { set; get; }

        public override bool Validate()
        {
            return true;
        }
    }
}
