using System;
using System.Collections.Generic;
using System.Text;
using Kugar.Core.Collections;
using Kugar.WechatSDK.MP.Entities;

namespace Kugar.WechatSDK.MP.Web
{
    public class MessageQueue
    {
        private HashQueue<WechatMPRequestBase> _hashQueue =
            new HashQueue<WechatMPRequestBase>(new WechatMPRequestCompare());

        public 
    }
    
}
