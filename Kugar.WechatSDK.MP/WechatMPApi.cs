using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Kugar.WechatSDK.MP
{
    public interface IWechatMPApi
    {
        MenuService Menu { get; }
        IOAuthService OAuth { get; }
        IUIService JsUI { get; }

        MessageService Message { get; }


    }

    public class WechatMPApi : IWechatMPApi
    {
        public WechatMPApi(
            MenuService menu,
            IOAuthService oauth,
            IUIService ui,
            MessageService message
            )
        {
            Debugger.Break();
            Menu = menu;
            OAuth = oauth;
            JsUI = ui;
            Message = message;
        }

        public MenuService Menu { get; }

        public IOAuthService OAuth { get; }

        public IUIService JsUI { get; }
        public MessageService Message { get; }
    }
}
