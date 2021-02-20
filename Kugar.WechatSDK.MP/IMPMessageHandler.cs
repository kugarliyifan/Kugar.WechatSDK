using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Kugar.WechatSDK.MP.Entities;

namespace Kugar.WechatSDK.MP
{
    public interface IMPMessageExecutor
    {
        Task<WechatMPResponseBase> Execute(WechatMPRequestBase msg);

    }
}
