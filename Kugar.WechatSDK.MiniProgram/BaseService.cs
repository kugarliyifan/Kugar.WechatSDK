using System;
using System.Collections.Generic;
using System.Text;
using Kugar.WechatSDK.Common;

namespace Kugar.WechatSDK.MiniProgram
{
    public abstract class BaseService
    {
        protected BaseService(ICommonApi api)
        {
            CommonApi = api;
        }

        protected ICommonApi CommonApi { get; }
    }
}
