using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kugar.Core.BaseStruct;
using Kugar.Core.ExtMethod;
using Kugar.WechatSDK.Common;
using Kugar.WechatSDK.MP.Entities;
using Kugar.WechatSDK.MP.Enums;
using Newtonsoft.Json.Linq;

namespace Kugar.WechatSDK.MP
{
    /// <summary>
    /// 群发消息接口
    /// </summary>
    public interface IBrocastMsgService
    {
        /// <summary>
        /// 上传图片到微信服务器,并返回地址
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="imageData"></param>
        /// <returns></returns>
        Task<ResultReturn<string>> UploadImage(string appId, Stream imageData);

        /// <summary>
        /// 上传图文消息
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="articles">图文消息，一个图文消息支持1到8条图文</param>
        /// <param name="sendIgnoreReprint">指定待群发的文章被判定为转载时，是否继续群发,,默认为false,,,true=判断为转载时,不继续群发</param>
        /// <returns>returnData为上传后微信返回的media_id数据</returns>
        Task<ResultReturn<string>> UpdateNews(string appId, BrocastMessageNewsItem[] articles, bool sendIgnoreReprint = false);

        /// <summary>
        /// 上传视频,使用BrocastMessageXXX系列的函数中,如果需要发送视频,则需要先使用本函数,将素材的视频转换一下
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="media_id">视频素材的MediaID</param>
        /// <param name="title">视频标题</param>
        /// <param name="description">视频描述</param>
        /// <returns></returns>
        Task<ResultReturn<string>> UpdateVideo(string appId, string media_id,string title,string description);

        /// <summary>
        /// 发送到所有订阅用户
        /// </summary>
        /// <returns></returns>
        Task<IResultReturn<(int msg_id,string msg_data_id)>> BrocastMsgToAll(string appId,BrocastMessageParameterBase msg);

        /// <summary>
        /// 按标签ID群发消息
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="tagId">标签ID</param>
        /// <param name="msg">消息数据</param>
        /// <returns></returns>
        Task<IResultReturn<(int msg_id,string msg_data_id)>> BrocastMsgByTags(string appId, int tagId,BrocastMessageParameterBase msg);

        /// <summary>
        /// 按OpenID群发消息
        /// </summary>
        /// <returns></returns>
        Task<IResultReturn<(int msg_id,string msg_data_id)>> BrocastMsgByOpenIDList(string appId, string[] openIds,BrocastMessageParameterBase msg);

        /// <summary>
        /// 删除已发送的群发消息
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="msgId">消息ID</param>
        /// <param name="index">需要删除的图文消息的顺序,null/0 为全部删除,,起始index为1</param>
        /// <returns></returns>
        Task<ResultReturn> DeleteBrocastNews(string appId, string msgId, int? index =null);

        /// <summary>
        /// 删除一条群发消息,如果是需要删除图文消息中某一个,则使用DeleteBrocastNews,其他情况,请使用本函数
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="msgId"></param>
        /// <returns></returns>
        Task<ResultReturn> DeleteBrocastMsg(string appId, string msgId);

        /// <summary>
        /// 预览群发消息,,发送到指定OpenId
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="media_id"></param>
        /// <returns></returns>
        Task<ResultReturn> PreviewBrocastMsg(string appId,string openId, string media_id,BrocastMessageParameterBase msg);

        /// <summary>
        /// 获取消息发送状态
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="msgId">消息ID</param>
        /// <returns>如果IsSuccess为true,ReturnCode为状态 0=发送成功  1=发送中  2=发送失败  3=消息已删除<br/>
        /// 如果IsSuccess为false,ReturnCode为错误代码
        /// </returns>
        Task<ResultReturn> GetSendStatus(string appId, string msgId);
    }

    /// <summary>
    /// 群发消息接口
    /// </summary>
    [Browsable(false)]
    public class BrocastMsgService : MPBaseService, IBrocastMsgService
    {
        public BrocastMsgService(ICommonApi api) : base(api)
        {
        }

        /// <summary>
        /// 上传图片到微信服务器,并返回地址
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="imageData"></param>
        /// <returns></returns>
        public async Task<ResultReturn<string>> UploadImage(string appId, Stream imageData)
        {
            var ret = await CommonApi.PostFileByForm(appId,
                "/cgi-bin/media/uploadimg?access_token=ACCESS_TOKEN",
                "media", "corefile.jpg", imageData)
            ;

            if (ret.IsSuccess)
            {
                return new SuccessResultReturn<string>(ret.ReturnData.GetString("url"));
            }
            else
            {
                return ret.Cast("", "");
            }
        }

        /// <summary>
        /// 上传图文消息
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="articles">图文消息，一个图文消息支持1到8条图文</param>
        /// <param name="sendIgnoreReprint">指定待群发的文章被判定为转载时，是否继续群发,,默认为false,,,true=判断为转载时,不继续群发</param>
        /// <returns>returnData为上传后微信返回的media_id数据</returns>
        public async Task<ResultReturn<string>> UpdateNews(string appId, BrocastMessageNewsItem[] articles, bool sendIgnoreReprint = false)
        {
            var json = new JObject();

            var jarray = new JArray();

            foreach (var item in articles)
            {
                var tmp = new JObject();

                tmp.Add("thumb_media_id", item.ThumbMediaId);
                tmp.Add("title", item.Title);
                tmp.Add("content", item.Content);

                tmp.AddPropertyIf(!string.IsNullOrWhiteSpace(item.Author), "author", item.Author);
                tmp.AddPropertyIf(!string.IsNullOrWhiteSpace(item.ContentSourceUrl), "content_source_url", item.ContentSourceUrl);
                tmp.AddPropertyIf(!string.IsNullOrWhiteSpace(item.Digest), "digest", item.Digest);
                tmp.AddPropertyIf(item.ShowCoverPic.HasValue, "show_cover_pic", item.ShowCoverPic);
                tmp.AddPropertyIf(item.NeedOpenComment.HasValue, "need_open_comment", item.NeedOpenComment);
                tmp.AddPropertyIf(item.OnlyFansCanComment.HasValue, "only_fans_can_comment", item.OnlyFansCanComment);
                tmp.AddPropertyIf(sendIgnoreReprint == true, "send_ignore_reprint", 1);



                jarray.Add(item);
            }

            json.Add("articles", jarray);

            var ret = await CommonApi.Post(appId, "/cgi-bin/media/uploadnews?access_token=ACCESS_TOKEN", json);

            if (ret.IsSuccess)
            {
                return new SuccessResultReturn<string>(ret.ReturnData.GetString("media_id"));
            }
            else
            {
                return ret.Cast("");
            }
        }
        
        /// <summary>
        /// 上传视频,使用BrocastMessageXXX系列的函数中,如果需要发送视频,则需要先使用本函数,将素材的视频转换一下
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="media_id">视频素材的MediaID</param>
        /// <param name="title">视频标题</param>
        /// <param name="description">视频描述</param>
        /// <returns></returns>
        public async Task<ResultReturn<string>> UpdateVideo(string appId, string media_id,string title,string description)
        {
            var json = new JObject()
            {
                ["media_id"]=media_id,
                ["title"]=title,
                ["description"]=description
            };

            var ret = await CommonApi.Post(appId, "/cgi-bin/media/uploadnews?access_token=ACCESS_TOKEN", json);

            if (ret.IsSuccess)
            {
                return new SuccessResultReturn<string>(ret.ReturnData.GetString("media_id"));
            }
            else
            {
                return ret.Cast("");
            }
        }



        /// <summary>
        /// 发送到所有订阅用户
        /// </summary>
        /// <returns></returns>
        public async Task<IResultReturn<(int msg_id,string msg_data_id)>> BrocastMsgToAll(string appId,BrocastMessageParameterBase msg)
        {
            if (msg==null)
            {
                throw new ArgumentNullException(nameof(msg));
            }

            if (string.IsNullOrWhiteSpace(appId))
            {
                throw new ArgumentNullException(nameof(appId));
            }

            var args = convertBrocastMessageToJson(msg);

            args.Add("filter",new JObject()
            {
                ["is_to_all"]=true
            });

            return await sendBrocastMessage(appId, "/cgi-bin/message/mass/sendall?access_token=ACCESS_TOKEN", args);
        }

        /// <summary>
        /// 按标签ID群发消息
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="tagId">标签ID</param>
        /// <param name="msg">消息数据</param>
        /// <returns></returns>
        public async Task<IResultReturn<(int msg_id,string msg_data_id)>> BrocastMsgByTags(string appId, int tagId,BrocastMessageParameterBase msg)
        {
            if (msg==null)
            {
                throw new ArgumentNullException(nameof(msg));
            }

            var args = convertBrocastMessageToJson(msg);

            args.Add("filter",new JObject()
            {
                ["is_to_all"]=false,
                ["tag_id"]=tagId
            });

            return await sendBrocastMessage(appId, "/cgi-bin/message/mass/sendall?access_token=ACCESS_TOKEN", args);
        }

        /// <summary>
        /// 按OpenID群发消息
        /// </summary>
        /// <returns></returns>
        public async Task<IResultReturn<(int msg_id,string msg_data_id)>> BrocastMsgByOpenIDList(string appId, string[] openIds,BrocastMessageParameterBase msg)
        {
            if (msg==null)
            {
                throw new ArgumentNullException(nameof(msg));
            }

            if (openIds.HasData())
            {
                throw new ArgumentNullException(nameof(openIds), "openIds参数必须为string数组");
            }

            var args = convertBrocastMessageToJson(msg);

            args.Add("touser",new JArray(openIds));

            return await sendBrocastMessage(appId, "/cgi-bin/message/mass/send?access_token=ACCESS_TOKEN", args);
        }

        /// <summary>
        /// 删除已发送的群发消息
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="msgId">消息ID</param>
        /// <param name="index">需要删除的图文消息的顺序,null/0 为全部删除,,起始index为1</param>
        /// <returns></returns>
        public async Task<ResultReturn> DeleteBrocastNews(string appId, string msgId, int? index =null)
        {
            if (string.IsNullOrWhiteSpace(appId))
            {
                throw new ArgumentNullException(nameof(appId));
            }

            var args = new JObject()
            {
                ["msg_id"]=msgId
            };

            args.AddPropertyIf(index > 0, "article_idx", index.GetValueOrDefault(0));

            var ret = await CommonApi.Post(appId,"/cgi-bin/message/mass/delete",args);

            if (ret.IsSuccess)
            {
                return new SuccessResultReturn<(int msg_id,string msg_data_id)>((ret.ReturnData.GetInt("msg_id"),ret.ReturnData.GetString("msg_data_id")));
            }
            else
            {
                return ret.Cast((-1,""));
            }
        }

        /// <summary>
        /// 删除一条群发消息,如果是需要删除图文消息中某一个,则使用DeleteBrocastNews,其他情况,请使用本函数
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="msgId"></param>
        /// <returns></returns>
        public async Task<ResultReturn> DeleteBrocastMsg(string appId, string msgId)
        {
            if (string.IsNullOrWhiteSpace(appId))
            {
                throw new ArgumentNullException(nameof(appId));
            }

            var args = new JObject()
            {
                ["msg_id"]=msgId
            };
            
            var ret = await CommonApi.Post(appId,"/cgi-bin/message/mass/delete",args);

            if (ret.IsSuccess)
            {
                return new SuccessResultReturn<(int msg_id,string msg_data_id)>((ret.ReturnData.GetInt("msg_id"),ret.ReturnData.GetString("msg_data_id")));
            }
            else
            {
                return ret.Cast((-1,""));
            }
        }

        /// <summary>
        /// 预览群发消息,,发送到指定OpenId
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="media_id"></param>
        /// <returns></returns>
        public async Task<ResultReturn> PreviewBrocastMsg(string appId,string openId, string media_id,BrocastMessageParameterBase msg)
        {
            if (msg==null)
            {
                throw new ArgumentNullException(nameof(msg));
            }

            if (string.IsNullOrWhiteSpace(openId))
            {
                throw new ArgumentNullException(nameof(openId), "openId参数必须为用户openID");
            }

            var args = convertBrocastMessageToJson(msg);

            args.Add("touser",openId);

            return await sendBrocastMessage(appId, "/cgi-bin/message/mass/preview?access_token=ACCESS_TOKEN", args);
        }

        /// <summary>
        /// 获取消息发送状态
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="msgId">消息ID</param>
        /// <returns>如果IsSuccess为true,ReturnCode为状态 0=发送成功  1=发送中  2=发送失败  3=消息已删除<br/>
        /// 如果IsSuccess为false,ReturnCode为错误代码
        /// </returns>
        public async Task<ResultReturn> GetSendStatus(string appId, string msgId)
        {
            if (string.IsNullOrWhiteSpace(msgId))
            {
                throw new ArgumentNullException(nameof(msgId));
            }

            var args = new JObject()
            {
                ["msg_id"] = msgId
            };

            var ret= await CommonApi.Post(appId, "/cgi-bin/message/mass/get?access_token=ACCESS_TOKEN", args);

            

            if (ret.IsSuccess)
            {
                var status = ret.ReturnData.GetString("msg_status");

                if (status=="SEND_SUCCESS")
                {
                    return new SuccessResultReturn()
                    {
                        ReturnCode = 0,
                    };
                }
                else if (status == "SENDING")
                {
                    return new SuccessResultReturn()
                    {
                        ReturnCode = 1
                    };
                }
                else if (status == "SEND_FAIL")
                {
                    return new SuccessResultReturn(status)
                    {
                        ReturnCode = 2
                    };
                }
                else if (status == "DELETE")
                {
                    return new SuccessResultReturn()
                    {
                        ReturnCode = 3
                    };
                }
            }

            return ret;
        }

        private async Task<ResultReturn<(int msg_id,string msg_data_id)>> sendBrocastMessage(string appId, string url, JObject args)
        {
            if (string.IsNullOrWhiteSpace(appId))
            {
                throw new ArgumentNullException(nameof(appId));
            }

            var ret = await CommonApi.Post(appId,url,args);

            if (ret.IsSuccess)
            {
                return new SuccessResultReturn<(int msg_id,string msg_data_id)>((ret.ReturnData.GetInt("msg_id"),ret.ReturnData.GetString("msg_data_id")));
            }
            else
            {
                return ret.Cast((-1,""));
            }
        }

        private JObject convertBrocastMessageToJson(BrocastMessageParameterBase msg)
        {
            JObject json = null;

            switch (msg.Type)
            {
                case BrocastMessageType.Image:
                    {
                        var tmp = (BrocastMessageParameter_Image)msg;

                        json = new JObject()
                        {
                            ["images"] = new JObject()
                            {
                                ["media_ids"] = new JArray(tmp.MeidaIds),
                                ["recommend"] = tmp.Recommend ?? "分享图片",
                                ["need_open_comment"] = tmp.NeedOpenComment ? 1 : 0,
                                ["only_fans_can_comment"] = tmp.OnlyFansCanComment ? 1 : 0
                            },
                            ["msgtype"] = "image"
                        };
                    }
                    break;
                case BrocastMessageType.Video:
                    {
                        var tmp = (BrocastMessageParameter_Video)msg;

                        json = new JObject()
                        {
                            ["mpvideo"] = new JObject()
                            {
                                ["media_id"] = tmp.MeidaId,
                                ["title"]=tmp.Title,
                                ["description"]=tmp.Description
                            },
                            ["msgtype"] = "mpvideo"
                        };
                    }
                    break;
                case BrocastMessageType.AudioOrVoice:
                {
                    var tmp = (BrocastMessageParameter_AudioOrVoice) msg;

                    json = new JObject()
                    {
                        ["voice"] = new JObject()
                        {
                            ["media_id"] = tmp.MeidaId
                        },
                        ["msgtype"] = "voice"
                    };
                }
                    break;
                case BrocastMessageType.News:
                {
                    var tmp = (BrocastMessageParameter_News) msg;

                    json = new JObject()
                    {
                        ["mpnews"] = new JObject()
                        {
                            ["media_id"] = tmp.MeidaId
                        },
                        ["msgtype"] = "mpnews",
                        ["send_ignore_reprint"]=tmp.SendIgnoreReprint?1:0
                    };
                }
                    break;
                case BrocastMessageType.Text:
                {
                    var tmp = (BrocastMessageParameter_Text) msg;

                    json = new JObject()
                    {
                        ["text"] = new JObject()
                        {
                            ["content"] = tmp.Content
                        },
                        ["msgtype"] = "text"
                    };
                }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return json;
        }
    }
}
