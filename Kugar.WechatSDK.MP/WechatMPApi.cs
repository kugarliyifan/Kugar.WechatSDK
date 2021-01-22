using System;
using System.Collections.Generic;
using System.Text;

namespace Kugar.WechatSDK.MP
{
    public interface IWechatMPApi
    {
        MenuService Menu { get; }
        IOAuthService OAuth { get; }
        UIService JsUI { get; }
    }

    public class WechatMPApi : IWechatMPApi
    {
        public WechatMPApi(
            MenuService menu,
            IOAuthService oauth,
            UIService ui
            )
        {
            Menu = menu;
            OAuth = oauth;
            JsUI = ui;
        }

        public MenuService Menu { get; }

        public IOAuthService OAuth { get; }

        public UIService JsUI { get; }
    }
}
