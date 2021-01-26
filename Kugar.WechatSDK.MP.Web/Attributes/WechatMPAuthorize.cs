using System;
using System.Collections.Generic;
using System.Text;
using Kugar.WechatSDK.MP.Enums;
using Microsoft.AspNetCore.Authorization;

namespace Kugar.WechatSDK.MP.Attributes
{
    public class WechatMPAuthorizeAttribute:AuthorizeAttribute
    {
        public WechatMPAuthorizeAttribute(string scheme = "wechat",SnsapiType oauthType= SnsapiType.Base) : base()
        {
            this.AuthenticationSchemes = scheme;

            OAuthType = oauthType;
        }

        public SnsapiType OAuthType { get; }
        
    }

}
