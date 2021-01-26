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
    }

    public class WechatMPApi : IWechatMPApi
    {
        public WechatMPApi(
            MenuService menu,
            IOAuthService oauth,
            IUIService ui
            )
        {
            Debugger.Break();
            Menu = menu;
            OAuth = oauth;
            JsUI = ui;
        }

        public MenuService Menu { get; }

        public IOAuthService OAuth { get; }

        public IUIService JsUI { get; }
    }
}
