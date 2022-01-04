using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Kugar.Core.BaseStruct;
using Kugar.WechatSDK.MiniProgram.Entities;

namespace Kugar.WechatSDK.MiniProgram
{
    public interface IMiniProgramMessageExecutor
    {
        Task<ResultReturn> Execute(MiniProgramMsgBase msg);

    }
}
