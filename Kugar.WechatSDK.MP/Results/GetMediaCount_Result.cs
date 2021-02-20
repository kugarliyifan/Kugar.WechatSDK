using System;
using System.Collections.Generic;
using System.Text;

namespace Kugar.WechatSDK.MP.Results
{
    public class GetMediaCount_Result
    {
        public int VideoCount { set; get; }

        public int VoiceCount { set; get; }

        public int ImageCount { set; get; }

        public int NewsCount { set; get; }
    }
}
