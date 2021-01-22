using System;
using System.Collections.Generic;
using System.Text;

namespace Kugar.WechatSDK.MP.Results
{
    public class WxUserInfo_Result
    {
        public string OpenID { set; get; }

        public string NickName { set; get; }

        public int Sex { set; get; }

        public string Province { set; get; }

        public string City { set; get; }

        public string Country { set; get; }

        public string HeadImageUrl { set; get; }

        public string[] Privilege { set; get; }

        public string UnionID { set; get; }
    }
}
