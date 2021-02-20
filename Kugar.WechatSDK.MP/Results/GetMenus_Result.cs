using System;
using System.Collections.Generic;
using System.Text;
using Kugar.WechatSDK.MP.Entities;

namespace Kugar.WechatSDK.MP.Results
{
    public class GetMenus_Result
    {
        public bool IsMenuOpen { set; get; }

        public IReadOnlyList<MPMenuBase> Menus { set; get; }
    }
}
