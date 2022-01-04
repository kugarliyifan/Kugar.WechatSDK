using System;
using System.Collections.Generic;
using System.Text;
using Kugar.Core.ExtMethod;
using Newtonsoft.Json.Linq;

namespace Kugar.WechatSDK.MiniProgram.Entities
{
    public abstract class MiniProgramMsgBase
    {
        /// <summary>
        /// 小程序的username
        /// </summary>
        public string ToUserName { set; get; }
        
        /// <summary>
        /// 平台推送服务UserName
        /// </summary>
        public string FromUserName { set; get; }

        /// <summary>
        /// 发送时间
        /// </summary>
        public DateTime CreateDt { set; get; }

        public abstract string MsgType {  get; }

        public virtual void FromJson(JObject json)
        {
            ToUserName = json.GetString("ToUserName");
            FromUserName = json.GetString("FromUserName");
            CreateDt = json.GetInt("CreateTime").ToLocalDatetimeFromUTCSeconds();

        }
        
    }

    public abstract class MiniProgramEventMsgBase:MiniProgramMsgBase
    {
        public abstract string EventType { get; }

        public override string MsgType => "event";
    }

    public class MiniProgramEvent_MediaCheck:MiniProgramEventMsgBase
    {
        public override string EventType => "wxa_media_check";

        /// <summary>
        /// 检测结果，false：暂未检测到风险，true：风险
        /// </summary>
        public bool IsRisky { get; set; }

        /// <summary>
        /// 小程序的appid
        /// </summary>
        public string AppId { set; get; }

        /// <summary>
        /// 任务id
        /// </summary>
        public string TraceId { set; get; }

        /// <summary>
        /// 状态码: 0=正常  -1008=无法下载
        /// </summary>
        public int StatusCode { set;get; }

        /// <summary>
        /// 附加信息，默认为空
        /// </summary>
        public string ExtraInfoJson { set;get; }

        public override void FromJson(JObject json)
        {
            base.FromJson(json);

            IsRisky = json.GetInt("isrisky") == 1;
            AppId = json.GetString("appid");
            TraceId = json.GetString("trace_id");
            StatusCode = json.GetInt("status_code");
            ExtraInfoJson = json.GetString("extra_info_json");
        }
    }
}
