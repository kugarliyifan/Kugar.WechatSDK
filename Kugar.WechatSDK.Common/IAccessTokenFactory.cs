using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Kugar.WechatSDK.Common
{
    public interface IAccessTokenFactory
    {
        Task<string> GetAccessToken(string appID);
    }
}
