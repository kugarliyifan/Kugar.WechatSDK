using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using Kugar.Core.BaseStruct;
using Kugar.Core.ExtMethod;
using Kugar.WechatSDK.Common;
using Kugar.WechatSDK.Common.Gateway;
using Kugar.WechatSDK.Common.Helpers;
using Kugar.WechatSDK.MP.Entities;
using Polly;
using Tencent;

namespace Kugar.WechatSDK.MP
{
    public class MessageService:MPBaseService
    {
        private IWechatGateway _gateway = null;

        public MessageService(ICommonApi api,IWechatGateway gateway) : base(api)
        {
            _gateway = gateway;
        }

        /// <summary>
        /// 用于解密从微信服务器中,发送过来的加密信息
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public  ResultReturn<string> DecryptMessage(string appId,string encryptMsg)
        {
            var config = _gateway.Get<MPConfiguration>(appId);

            var ret = WXBizMsgCrypt.DecryptMsg(encryptMsg,config.EncryptAESKey);

            if (ret.code==0)
            {
                return new SuccessResultReturn<string>(ret.msg);
            }
            else
            {
                return new FailResultReturn<string>("解密失败",ret.code);
            }
        }

        /// <summary>
        /// 加密回复微信信息
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="repsonseData"></param>
        /// <returns></returns>
        public ResultReturn<string> EncryptMessage(string appId, string repsonseData)
        {
            var config = _gateway.Get<MPConfiguration>(appId);

            var nonce = Guid.NewGuid().ToString("N");
            var timestamp = DateTimeHelper.GetUnixDateTime(DateTime.Now);

            var encMsgData = "";

            var ret = WXBizMsgCrypt.EncryptMsg(repsonseData,config.EncryptAESKey,appId,timestamp.ToStringEx(),nonce,config.Token,ref encMsgData);

            if (ret==0)
            {
                return new SuccessResultReturn<string>(encMsgData);
            }
            else
            {
                return new FailResultReturn<string>("加密失败", ret);
            }
        }

        /// <summary>
        /// 解码微信服务器推送过来的消息
        /// </summary>
        /// <param name="xmlData">必须是解密后的xml数据</param>
        /// <returns></returns>
        public ResultReturn<WechatMPRequestBase> DecodeMPRequestMsg(string xmlData)
        {
            var xml = new XmlDocument();

            try
            {
                xml.LoadXml(xmlData);
            }
            catch (Exception e)
            {
                return new FailResultReturn<WechatMPRequestBase>("解析xml错误");
            }

            var msgType = xml.GetFirstNodeByTagName("MsgType").InnerText;

            WechatMPRequestBase result = null;

            switch (msgType)
            {
                case "text":
                {
                    result = new WechatMPRequestText();
                    break;
                }
                case "image":
                {
                    result = new WechatMPRequestImage();
                    break;
                }
                case "voice":
                {
                    result = new WechatMPRequestVoice();
                    break;
                }
                case "video":
                {
                    result = new WechatMPRequestVideo();
                    break;
                }
                case "shortvideo":
                {
                    result = new WechatMPRequestShortVideo();
                    break;
                }
                case "location":
                {
                    result = new WechatMPRequestLocation();
                    break;
                }
                case "link":
                {
                    result = new WechatMPRequestLink();
                    break;
                }
                case "event":
                {
                    var eventType = xml.GetFirstNodeByTagName("Event").InnerText;

                    switch (eventType)
                    {
                        case "subscribe":
                        {
                            result = new WechatMPRequestSubscribe();
                            break;
                        }
                        case "SCAN":
                        {
                            result = new WechatMPRequestScan();
                            break;
                        }
                        case "LOCATION":
                        {
                            result = new WechatMPRequestReportLocation();
                            break;
                        }
                        case "CLICK":
                        {
                            result = new WechatMPRequestClick();
                            break;
                        }
                        case "VIEW":
                        {
                            result = new WechatMPRequestView();
                            break;
                        }
                        case "TEMPLATESENDJOBFINISH":
                        {
                            result = new WechatMPRequestTemplateMsgFinish();
                            break;
                        }
                        case "subscribe_msg_popup_event":
                        {
                            result = new WechatMPRequestSubscribeMsgResult();
                            break;
                        }
                    }
                    break;
                }
            }

            if (result!=null)
            {
                result.LoadFromXml(xml);

                return new SuccessResultReturn<WechatMPRequestBase>(result);
            }
            else
            {
                return new FailResultReturn<WechatMPRequestBase>($"未知消息类型:{msgType}");
            }
        }


    }
}
