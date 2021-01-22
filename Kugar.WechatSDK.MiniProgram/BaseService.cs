using System;
using System.Collections.Generic;
using System.Text;
using Kugar.WechatSDK.Common;

namespace Kugar.WechatSDK.MiniProgram
{
    public abstract class BaseService
    {
        protected BaseService(CommonApi api)
        {
            CommonApi = api;
        }

        protected CommonApi CommonApi { get; }
    }
}
