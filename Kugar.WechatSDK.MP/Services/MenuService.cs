using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fasterflect;
using Kugar.Core.BaseStruct;
using Kugar.Core.ExtMethod;
using Kugar.WechatSDK.Common;
using Kugar.WechatSDK.MP.Entities;
using Kugar.WechatSDK.MP.Results;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Kugar.WechatSDK.MP
{
    /// <summary>
    /// 公众号菜单接口
    /// </summary>
    public interface IMenuService
    {
        /// <summary>
        /// 创建通用菜单
        /// </summary>
        /// <param name="appID"></param>
        /// <param name="menus"></param>
        /// <returns></returns>
        Task<ResultReturn> CreateMenu(string appID,MPMenuBase[] buttons);

        /// <summary>
        /// 获取通用菜单
        /// </summary>
        /// <param name="appId"></param>
        /// <returns></returns>
        Task<ResultReturn<GetMenus_Result>> GetMenus(string appId);

        /// <summary>
        /// 删除所有菜单,包括个性化菜单
        /// </summary>
        /// <param name="appId"></param>
        /// <returns></returns>
        Task<ResultReturn> DeleteMenu(string appId);

        /// <summary>
        /// 获取所有菜单,包括个性化菜单和通用菜单
        /// </summary>
        /// <param name="appId"></param>
        /// <returns></returns>
        Task<ResultReturn<GetAllMenus_Result>> GetAllMenus(string appId);

        /// <summary>
        /// 创建个性化菜单,并返回menuid
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="menus">个性化菜单按钮列表</param>
        /// <param name="rule">个性化菜单判断规则</param>
        /// <returns>返回中的ReturnData为菜单ID</returns>
        Task<ResultReturn<string>> CreatePersonalizedMenu(string appId,IReadOnlyList<MPMenuBase> buttons,PersonalizedMenuRule rule);

        /// <summary>
        /// 根据menuid删除个性化菜单
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="menuId"></param>
        /// <returns></returns>
        Task<ResultReturn> DeletePersonalizedMenu(string appId, string menuId);

        /// <summary>
        /// 测试个性化菜单匹配,
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="userOpenIdOrAccount">可以是粉丝的OpenID，也可以是粉丝的微信号</param>
        /// <returns></returns>
        Task<ResultReturn<IReadOnlyList<MPMenuBase>>> TryMatchPersonalizedMenu(string appId, string userOpenIdOrAccount);
    }

    public class MenuService:MPBaseService, IMenuService
    {
        public MenuService(ICommonApi api) : base(api)
        {
        }

        /// <summary>
        /// 创建通用菜单
        /// </summary>
        /// <param name="appID"></param>
        /// <param name="menus"></param>
        /// <returns></returns>
        public async Task<ResultReturn> CreateMenu(string appID,MPMenuBase[] buttons)
        {
            var ret = await CommonApi.Post(appID,
                "/cgi-bin/menu/create?access_token=ACCESS_TOKEN",
                new JObject()
                {
                    ["button"]=new JArray(buttons.Select(x=>JObject.FromObject(x)))
                });

            return ret;
        }

        /// <summary>
        /// 获取通用菜单
        /// </summary>
        /// <param name="appId"></param>
        /// <returns></returns>
        public async Task<ResultReturn<GetMenus_Result>> GetMenus(string appId)
        {
            var ret = await CommonApi.Get(appId, "/cgi-bin/get_current_selfmenu_info?access_token=ACCESS_TOKEN");

            if (ret.IsSuccess)
            {
                var isMenuOpen = ret.ReturnData.GetInt("is_menu_open");

                var result = new GetMenus_Result();
                result.IsMenuOpen = isMenuOpen == 1;

                var menuInfoJson = ret.ReturnData.GetJObject("selfmenu_info");

                var buttons = menuInfoJson.GetJObjectArray("button");

                var lst = new List<MPMenuBase>();

                foreach (var button in buttons)
                {
                    lst.Add(button.ToObject<MPMenuBase>());
                }

                result.Menus = lst;

                return new SuccessResultReturn<GetMenus_Result>(result);
            }
            else
            {
                return ret.Cast<GetMenus_Result>(null);
            }
        }

        /// <summary>
        /// 删除所有菜单,包括个性化菜单
        /// </summary>
        /// <param name="appId"></param>
        /// <returns></returns>
        public async Task<ResultReturn> DeleteMenu(string appId)
        {
            return await CommonApi.Get(appId, "/cgi-bin/menu/delete?access_token=ACCESS_TOKEN");
        }

        /// <summary>
        /// 获取所有菜单,包括个性化菜单和通用菜单
        /// </summary>
        /// <param name="appId"></param>
        /// <returns></returns>
        public async Task<ResultReturn<GetAllMenus_Result>> GetAllMenus(string appId)
        {
            var data = await CommonApi.Get(appId, "/cgi-bin/menu/get?access_token=ACCESS_TOKEN");

            if (data.IsSuccess)
            {
                var result = new GetAllMenus_Result();

                var commonMenusJson = data.ReturnData.GetJObject("menu");

                var commonMenuButtonsJson = commonMenusJson.GetJObjectArray("button");

                var commonButtons = commonMenuButtonsJson.Select(x => x.ToObject<MPMenuBase>()).ToArrayEx();

                result.CommonMenus = commonButtons;
                result.CommonMenuId = commonMenusJson.GetString("menuid");

                if (data.ReturnData.ContainsKey("conditionalmenu"))
                {
                    var conditionalMenusJson = data.ReturnData.GetJObjectArray("conditionalmenu");

                    if (conditionalMenusJson.HasData())
                    {
                        var conditionalMenus = new List<GetAllMenus_Result.PersonalizedMenu>();

                        foreach (var item in conditionalMenusJson)
                        {
                            var buttonsJson = item.GetJObjectArray("button");
                            var rulsJson = item.GetJObject("matchrule");

                            var tmp = new GetAllMenus_Result.PersonalizedMenu();

                            tmp.MenuId = item.GetString("menuid");
                            tmp.Menus = buttonsJson.Select(x => x.ToObject<MPMenuBase>()).ToList();
                            tmp.Rule = rulsJson.ToObject<PersonalizedMenuRule>();

                            conditionalMenus.Add(tmp);
                        }

                        result.PersonalizedMenus = conditionalMenus;
                    }
                }
                
                return new SuccessResultReturn<GetAllMenus_Result>(result);
            }
            else
            {
                return data.Cast<GetAllMenus_Result>(default);
            }

        }

        /// <summary>
        /// 创建个性化菜单,并返回menuid
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="menus">个性化菜单按钮列表</param>
        /// <param name="rule">个性化菜单判断规则</param>
        /// <returns>返回中的ReturnData为菜单ID</returns>
        public async Task<ResultReturn<string>> CreatePersonalizedMenu(string appId,IReadOnlyList<MPMenuBase> buttons,PersonalizedMenuRule rule)
        {
            var args = new JObject()
            {
                ["button"] = new JArray(buttons.Select(x => JObject.FromObject(x))),
                ["matchrule"] = JObject.FromObject(rule)
            };

            var ret = await CommonApi.Post(appId, "/cgi-bin/menu/addconditional?access_token=ACCESS_TOKEN", args);

            if (ret.IsSuccess)
            {
                return new SuccessResultReturn<string>(ret.ReturnData.GetString("menuid"));
            }
            else
            {
                return ret.Cast<string>(default);
            }
        }

        /// <summary>
        /// 根据menuid删除个性化菜单
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="menuId"></param>
        /// <returns></returns>
        public async Task<ResultReturn> DeletePersonalizedMenu(string appId, string menuId)
        {
            return await CommonApi.Post(appId, "/cgi-bin/menu/delconditional?access_token=ACCESS_TOKEN", new JObject()
            {
                ["menuid"] = menuId
            });
        }

        /// <summary>
        /// 测试个性化菜单匹配,
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="userOpenIdOrAccount">可以是粉丝的OpenID，也可以是粉丝的微信号</param>
        /// <returns></returns>
        public async Task<ResultReturn<IReadOnlyList<MPMenuBase>>> TryMatchPersonalizedMenu(string appId, string userOpenIdOrAccount)
        {
            var ret = await CommonApi.Post(appId, "/cgi-bin/menu/trymatch?access_token=ACCESS_TOKEN",
                new JObject()
                {
                    ["user_id"] = userOpenIdOrAccount
                }
            );

            if (ret.IsSuccess)
            {
                var buttons = ret.ReturnData.GetJObjectArray("button");

                if (buttons.HasData())
                {
                    return new SuccessResultReturn<IReadOnlyList<MPMenuBase>>(buttons
                        .Select(x => x.ToObject<MPMenuBase>()).ToList());
                }
                else
                {
                    return new SuccessResultReturn<IReadOnlyList<MPMenuBase>>(default);
                }
            }
            else
            {
                return ret.Cast<IReadOnlyList<MPMenuBase>>(default);
            }
        }
    }
}
