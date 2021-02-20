using System;
using System.Collections.Generic;
using System.Text;
using Kugar.WechatSDK.MP.Entities;

namespace Kugar.WechatSDK.MP.Results
{
    public class GetAllMenus_Result
    {
        /// <summary>
        /// 通用菜单按钮
        /// </summary>
        public IReadOnlyList<MPMenuBase> CommonMenus { set; get; }

        /// <summary>
        /// 通用菜单的id
        /// </summary>
        public string CommonMenuId { set; get; }

        /// <summary>
        /// 个性化菜单
        /// </summary>
        public IReadOnlyList<PersonalizedMenu> PersonalizedMenus { set; get; }

        public class PersonalizedMenu
        {
            public IReadOnlyList<MPMenuBase> Menus { set; get; }

            public PersonalizedMenuRule Rule { set; get; }

            public string MenuId { set; get; }
        }
    }
}
