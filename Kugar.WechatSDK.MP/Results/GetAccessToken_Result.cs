using System;
using System.Collections.Generic;
using System.Text;
using Kugar.WechatSDK.MP.Enums;

namespace Kugar.WechatSDK.MP.Results
{
    public class GetAccessToken_Result
    {
        public string AccessToken { set; get; }

        public int Expires { set; get; }

        public string RefreshToken { set; get; }

        public string OpenId { set; get; }

        public SnsapiType Type { set; get; }
    }
}
