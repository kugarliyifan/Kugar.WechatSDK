using System;
using System.Collections.Generic;
using System.Text;
using Kugar.WechatSDK.Common;

namespace Kugar.WechatSDK.MP
{
    public abstract class MPBaseService
    {
        protected MPBaseService(ICommonApi api)
        {
            CommonApi = api;
        }

        protected ICommonApi CommonApi { get; }
    }
}
