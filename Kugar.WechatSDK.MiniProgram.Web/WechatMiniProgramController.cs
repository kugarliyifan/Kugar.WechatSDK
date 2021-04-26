using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Xml;
using Kugar.Core.BaseStruct;
using Kugar.Core.ExtMethod;
using Kugar.Core.Log;
using Kugar.WechatSDK.Common;
using Kugar.WechatSDK.Common.Gateway;
using Kugar.WechatSDK.MiniProgram;
using Kugar.WechatSDK.MiniProgram.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ILoggerFactory = Microsoft.Extensions.Logging.ILoggerFactory;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace Kugar.WechatSDK.MP.Web
{

    public class WechatMiniProgramController : ControllerBase
    {
        [ApiExplorerSettings(IgnoreApi = true)]
        [Route("Core/MiniProgram/Service/{appID}")]
        [HttpGet]
        public async Task<IActionResult> Service([FromServices] IWechatGateway gateway,
            [FromQuery] string signature,
            [FromQuery] string timestamp,
            [FromQuery] string nonce,
            [FromQuery] string echostr,
            [FromRoute] string appID = "",
            [FromServices] ILoggerFactory logger=null
            )
        {
            if (gateway == null)
            {
                return Content("请先注册微信小程序");
            }

            logger?.CreateLogger("miniprogram")?.Log(LogLevel.Trace, $"微信调用:signature={signature},timestamp={timestamp},nonce={nonce},echostr={echostr}");

            if (string.IsNullOrWhiteSpace(appID))
            {
                return Content("AppID不能为空");
            }

            var config = gateway.Get<MiniProgramConfiguration>(appID);

            if (config == null)
            {
                return Content("该AppID非小程序配置");
            }

            if (CheckSignature.Check(signature, timestamp, nonce, config.Token))
            {
                return Content(echostr); //返回随机字符串则表示验证通过
            }
            else
            {
                return Content($"failed:{signature},{CheckSignature.GetSignature(timestamp, nonce, config.Token).ToString()}。如果你在浏览器中看到这句话，说明此地址可以被作为微信公众账号后台的Url，请注意保持Token一致。");
            }

        }

        [ApiExplorerSettings(IgnoreApi = true)]
        [Route("Core/MiniProgram/Service/{appID}")]
        [HttpPost]
        public async Task<IActionResult> ServicePost([FromServices] IWechatGateway gateway,
            [FromQuery] string signature,
            [FromQuery] string timestamp,
            [FromQuery] string nonce,
            [FromQuery] string echostr,
            [FromRoute] string appID = "",
            [FromServices] ILoggerFactory logger=null,
            [FromServices]IMiniProgramMessageExecutor executor=null
            )
        {
            Request.EnableBuffering();


            var jsonStr = Request.Body.ReadToEnd();

            if (Request.ContentType.Contains("json"))
            {
                var json = JObject.Parse(jsonStr);

                var eventType = json.GetString("Event");

                MiniProgramMsgBase msg = null;

                switch (eventType)
                {
                    case "wxa_media_check":
                    {
                        msg = new MiniProgramEvent_MediaCheck();
                        break;
                    }
                }

                if (msg==null)
                {
                    return NotFound();
                }

                msg.FromJson(json);

                if (executor != null)
                {
                    await executor.Execute(msg);
                }
            }

            return Content("");

            //if (gateway == null)
            //{
            //    return Content("请先注册微信公众号服务");
            //}

            //logger?.CreateLogger("weixin")?.Log(LogLevel.Trace, $"微信调用:signature={signature},timestamp={timestamp},nonce={nonce},echostr={echostr}");

            //if (string.IsNullOrWhiteSpace(appID))
            //{
            //    return Content("AppID不能为空");
            //}

            //var config = gateway.Get(appID) as MPConfiguration;

            //if (config == null)
            //{
            //    return Content("该AppID非公众号配置");
            //}

            //if (CheckSignature.Check(signature, timestamp, nonce, config.Token))
            //{
            //    return Content("校验无效,请检查token");
            //}

            //// v4.2.2之后的版本，可以设置每个人上下文消息储存的最大数量，防止内存占用过多，如果该参数小于等于0，则不限制
            //var maxRecordCount = 10;

            //Request.EnableBuffering();

            ////自定义MessageHandler，对微信请求的详细判断操作都在这里面。
            //var inputStream = Request.Body;
            //inputStream.Position = 0;

            //var xml = new XmlDocument();
            //xml.Load(inputStream);



            //return null;
        }
    }
}
