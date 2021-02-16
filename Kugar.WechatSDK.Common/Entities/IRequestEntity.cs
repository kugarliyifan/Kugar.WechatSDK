using System;
using System.Collections.Generic;
using System.Text;
using Kugar.Core.BaseStruct;

namespace Kugar.WechatSDK.Common.Entities
{
    public interface IRequestEntity
    {
        /// <summary>
        /// 校验微信请求参数是否正确
        /// </summary>
        /// <returns></returns>
        ResultReturn Validate();
    }
}
