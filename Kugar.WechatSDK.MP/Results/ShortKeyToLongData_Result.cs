using System;
using System.Collections.Generic;
using System.Text;

namespace Kugar.WechatSDK.MP.Results
{
    public class ShortKeyToLongData_Result
    {
        /// <summary>
        /// 长数据
        /// </summary>
        public string LongData { set;get; }

        /// <summary>
        /// 剩余过期秒数
        /// </summary>
        public long ExpireSecond { set;get; }
    }
}
