using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Threading.Tasks;
using Kugar.Core.BaseStruct;
using Kugar.Core.ExtMethod;
using Kugar.WechatSDK.Common;
using Kugar.WechatSDK.Common.Gateway;
using Kugar.WechatSDK.MP.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;

namespace Kugar.WechatSDK.MP
{
    public class TemplateMsg:MPBaseService
    {
        private IOptionsMonitor<MPRequestHostOption> _option = null;
        private IHttpContextAccessor _accessor = null;

        public TemplateMsg(IOptionsMonitor<MPRequestHostOption> option, ICommonApi api,IHttpContextAccessor accessor=null) : base(api)
        {
            _option = option;
            _accessor = accessor;
        }

        /// <summary>
        /// 构建公众号一次性订阅消息的授权跳转链接
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="template_id">模板消息ID</param>
        /// <param name="scene">场景值,0-10000</param>
        /// <param name="redirect_url">授权后回调地址,如果为空,,则为框架提供的默认回调地址</param>
        /// <param name="reserved">用于保持回调数据的回传,如果为空,则为appId加密后的数据,默认使用des加密,密码为appId参数的值</param>
        /// <returns>返回授权跳转的地址</returns>
        public string BuildSubscribeMsgUrl(string appId,
            string template_id,
            [Range(0,10000)]int scene,
            string redirect_url = "",
            string reserved = ""
        )
        {
            if (string.IsNullOrWhiteSpace(redirect_url))
            {
                var host = _accessor.HttpContext.Request.Host;
                
                redirect_url = $"http{(_accessor.HttpContext.Request.IsHttps?"s":"")}://{host.Host}{(host.Port.HasValue?$":{host.Port}":"")}/Core/MPCallback/SubscribeMsgCallback/{appId}";
            }

            if (string.IsNullOrWhiteSpace(reserved))
            {
                reserved = appId.ToString().DesEncrypt(appId);
            }

            return
                $"https://{_option.CurrentValue.MPApiHost}/mp/subscribemsg?action=get_confirm&appid={appId}&scene={scene}&template_id={template_id}&redirect_url={redirect_url}&reserved={reserved}#wechat_redirect";
        }

        /// <summary>
        /// 推送消息模板订阅的消息给用户
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="openId">接收者OpenId</param>
        /// <param name="template_id">模板消息ID</param>
        /// <param name="scene">场景值</param>
        /// <param name="title">消息标题</param>
        /// <param name="content">消息内容</param>
        /// <param name="gotoUrl">跳转的链接</param>
        /// <param name="miniProgramAppId">小程序的OpenId,该小程序必须是已和公众号绑定,不需跳小程序可不用传该数据</param>
        /// <param name="miniProgramPath">小程序跳转路径,支持带参数（示例index?foo=bar）</param>
        /// <remarks>
        /// url和miniprogram都是非必填字段，若都不传则模板无跳转；若都传，会优先跳转至小程序。开发者可根据实际需要选择其中一种跳转方式即可。当用户的微信客户端版本不支持跳小程序时，将会跳转至url。
        /// </remarks>
        /// <returns></returns>
        public async Task<ResultReturn> SendSubscribeTemplateToUser(
            string appId,
            string openId,
            string template_id,
            string scene,
            string title,
            string content,
            string gotoUrl="",
            string miniProgramAppId="",
            string miniProgramPath=""
        )
        {
            var args = new JObject()
            {
                ["touser"] = openId,
                ["template_id"] = template_id,
                //["url"] = gotoUrl,
                ["scene"] = scene,
                ["title"] = title,
                ["data"] = new JObject()
                {
                    ["content"]=new JObject()
                        {
                            ["value"] = content
                        }
                }
            };

            args.AddPropertyIf(!string.IsNullOrWhiteSpace(gotoUrl), "url", gotoUrl);
            args.AddPropertyIf(!string.IsNullOrWhiteSpace(miniProgramAppId), "miniprogram", new JObject()
            {
                ["appid"] = miniProgramAppId,
                ["pagepath"] = miniProgramPath
            });

            var ret =await CommonApi.Post(appId, "/cgi-bin/message/template/subscribe?access_token=ACCESS_TOKEN", args);

            return ret;
        }

        /// <summary>
        /// 向公众号添加一条新的模板消息
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="templateShortId">模板库中模板的编号，有“TM**”和“OPENTMTM**”等形式</param>
        /// <returns></returns>
        public async Task<ResultReturn<string>> AddTemplateMsg(string appId, string templateShortId)
        {
            var args = new JObject()
            {
                ["template_id_short"] = templateShortId
            };

            var ret =await CommonApi.Post(appId, "/cgi-bin/template/api_add_template?access_token=ACCESS_TOKEN", args);

            return ret.Cast(ret.ReturnData.GetString("template_id"),"");
        }

        public async Task<ResultReturn> SendTemplateMsg(
            string appId,
            string openId,
            string template_id,
            TemplateMsgDataItem[] data,
            string gotoUrl="",
            string miniProgramAppId="",
            string miniProgramPath=""
        )
        {
            var args = new JObject()
            {
                ["touser"] = openId,
                ["template_id"] = template_id
            };

            var dataJson = new JObject();

            foreach (var item in data)
            {
                dataJson.Add(item.Key,new JObject()
                {
                    ["value"]=item.Value
                });

                if (item.Color.HasValue)
                {
                    dataJson.Add("color",$"#{item.Color.Value.R:X2}{item.Color.Value.G:X2}{item.Color.Value.B:X2}");
                }
            }

            args.Add("data",dataJson);

            args.AddPropertyIf(!string.IsNullOrWhiteSpace(gotoUrl), "url", gotoUrl);
            args.AddPropertyIf(!string.IsNullOrWhiteSpace(miniProgramAppId), "miniprogram", new JObject()
            {
                ["appid"] = miniProgramAppId,
                ["pagepath"] = miniProgramPath
            });

            var ret =await CommonApi.Post(appId, "/cgi-bin/message/template/subscribe?access_token=ACCESS_TOKEN", args);

            return ret;
        }

        public async Task<ResultReturn> SendTemplateMsg(
            string appId,
            string openId,
            string template_id,
            (string key,string value)[]  data,
            string gotoUrl="",
            string miniProgramAppId="",
            string miniProgramPath=""
        )
        {
            var args = new JObject()
            {
                ["touser"] = openId,
                ["template_id"] = template_id
            };

            var dataJson = new JObject();

            foreach (var item in data)
            {
                dataJson.Add(item.key,new JObject()
                {
                    ["value"]=item.value
                });
            }

            args.Add("data",dataJson);

            args.AddPropertyIf(!string.IsNullOrWhiteSpace(gotoUrl), "url", gotoUrl);
            args.AddPropertyIf(!string.IsNullOrWhiteSpace(miniProgramAppId), "miniprogram", new JObject()
            {
                ["appid"] = miniProgramAppId,
                ["pagepath"] = miniProgramPath
            });

            var ret =await CommonApi.Post(appId, "/cgi-bin/message/template/subscribe?access_token=ACCESS_TOKEN", args);

            return ret;
        }
    }
}
