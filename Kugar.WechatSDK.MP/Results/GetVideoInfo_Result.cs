using System;
using System.Collections.Generic;
using System.Text;

namespace Kugar.WechatSDK.MP.Results
{
    public class GetVideoInfo_Result
    {
        /// <summary>
        /// 视频标题
        /// </summary>
        public string Title { set; get; }

        /// <summary>
        /// 视频描述
        /// </summary>
        public string Description { set; get; }

        /// <summary>
        /// 视频下载链接
        /// </summary>
        public string DownloadUrl { set; get; }
    }
}
