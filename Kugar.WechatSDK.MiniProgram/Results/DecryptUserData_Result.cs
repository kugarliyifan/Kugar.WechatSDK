using System;
using System.Collections.Generic;
using System.Text;

namespace Kugar.WechatSDK.MiniProgram.Results
{
    public class DecryptUserData_Result
    {
        public string OpenId { set; get; }

        public string NickName { set; get; }

        public string Gender { set; get; }

        public string City { set; get; }

        public string Province { set; get; }

        public string Country { set; get; }

        public string AvatarUrl { set; get; }

        public string UnionId { set; get; }
    }
}
