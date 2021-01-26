using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Kugar.Core.BaseStruct;
using Kugar.WechatSDK.MP.Enums;
using Microsoft.AspNetCore.Http;

namespace Kugar.WechatSDK.MP.Web
{
    public interface IWechatJWTLoginService
    {
        Task<ResultReturn<string>> Login(HttpContext context,string appID, string openID, SnsapiType oauthType, IWechatMPApi mp);
    }
}
