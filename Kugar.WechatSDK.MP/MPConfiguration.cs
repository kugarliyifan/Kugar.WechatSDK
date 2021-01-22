using System;
using System.Collections.Generic;
using System.Text;
using Kugar.WechatSDK.Common;

namespace Kugar.WechatSDK.MP
{
    public class MPConfiguration:WechatConfigurationBase
    {
        

        public string Token { set; get; }

        public string EncTokenKey { set; get; }


        public override bool Validate()
        {
            return true;
        }
    }
}
