using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Kugar.Core.BaseStruct;
using Kugar.Core.ExtMethod;
using Kugar.WechatSDK.Common;
using Kugar.WechatSDK.Common.Helpers;
using Kugar.WechatSDK.MP.Results;

namespace Kugar.WechatSDK.MP
{
    public interface IUIService
    {
        /// <summary>
        /// 获取UI注入所使用的参数
        /// </summary>
        /// <param name="appID"></param>
        /// <param name="url">当前页链接</param>
        /// <returns></returns>
        Task<ResultReturn<JsUIArgument>> CreateJsUIArgument(string appID,string url);

        /// <summary>
        /// 生成Js配置调用配置的js脚本wx.config({注入参数}) 的脚本
        /// </summary>
        /// <param name="appID"></param>
        /// <param name="url">当前页面url,需带域名</param>
        /// <param name="functions">可使用的接口,参考:https://developers.weixin.qq.com/doc/offiaccount/OA_Web_Apps/JS-SDK.html#4 的附录2</param>
        /// <returns></returns>
        Task<string> CreateJsUIConfigScript(string appID,string url,string[] functions,bool isDebug=false);
    }

    public class UIService:MPBaseService, IUIService
    {
        private IJsTicketContainer _jsTicketContainer = null;

        private string _wxConfigScriptTemplate =
            "wx.config({{\r\n  debug: {0}, \r\n  appId: '{1}', \r\n  timestamp: {2}, \r\n  nonceStr: '{3}',\r\n  signature: '{4}', \r\n  jsApiList: [{5}] \r\n}});";
        public UIService(ICommonApi api,IJsTicketContainer jsTicket) : base(api)
        {
            _jsTicketContainer = jsTicket;
        }

        /// <summary>
        /// 获取UI注入所使用的参数
        /// </summary>
        /// <param name="appID"></param>
        /// <param name="url">当前页链接</param>
        /// <returns></returns>
        public async Task<ResultReturn<JsUIArgument>> CreateJsUIArgument(string appID,string url)
        {
            var jsTicket =await _jsTicketContainer.GetJsTicket(appID);
            var nonce = Guid.NewGuid().ToString("N");
            var timestamp = DateTimeHelper.GetUnixDateTime(DateTime.Now);
            
            var waitSignStr =
                $"jsapi_ticket={jsTicket}&noncestr={nonce}&timestamp={timestamp.ToStringEx()}&url={url}";

            var signStr = EncryptHelper.GetSha1(waitSignStr);

            return new SuccessResultReturn<JsUIArgument>(new JsUIArgument()
            {
                AppId = appID,
                NonceStr = nonce,
                Signature = signStr,
                Timestamp = timestamp
            });
        }

        /// <summary>
        /// 生成Js配置调用配置的js脚本wx.config({注入参数}) 的脚本
        /// </summary>
        /// <param name="appID"></param>
        /// <param name="url">当前页面url,需带域名</param>
        /// <param name="functions">可使用的接口,参考:https://developers.weixin.qq.com/doc/offiaccount/OA_Web_Apps/JS-SDK.html#4 的附录2</param>
        /// <returns></returns>
        public async Task<string> CreateJsUIConfigScript(string appID,string url,string[] functions,bool isDebug=false)
        {
            var args =await CreateJsUIArgument(appID, url);

            if (!args.IsSuccess)
            {
                throw new ArgumentException(nameof(appID), $"获取jsTicket失败:code:{args.ReturnCode},msg:{args.Message}");
            }

            var v=args.ReturnData;

            return string.Format(_wxConfigScriptTemplate, isDebug.ToString().ToLower(), v.AppId, v.Timestamp.ToString(),
                v.NonceStr, v.Signature, functions.JoinToString(",", "\'", "\'"));

        }


    }
}
