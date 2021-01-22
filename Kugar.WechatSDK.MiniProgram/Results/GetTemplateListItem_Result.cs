using System;
using System.Collections.Generic;
using System.Text;

namespace Kugar.WechatSDK.MiniProgram.Results
{
    public class GetTemplateListItem_Result
    {
        /// <summary>
        /// 添加至帐号下的模板 id，发送小程序订阅消息时所需
        /// </summary>
        public string PriTmplId { set; get; }

        /// <summary>
        /// 模版标题
        /// </summary>
        public string Title { set;get; }

        /// <summary>
        /// 模版内容
        /// </summary>
        public string Content { set; get; }

        /// <summary>
        /// 模板内容示例
        /// </summary>
        public string Example { set; get; }

        /// <summary>
        /// 模版类型，2 为一次性订阅，3 为长期订阅
        /// </summary>
        public int Type { set; get; }
    }
}
