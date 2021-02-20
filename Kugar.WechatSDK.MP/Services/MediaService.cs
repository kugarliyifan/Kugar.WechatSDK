using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Kugar.Core.BaseStruct;
using Kugar.Core.ExtMethod;
using Kugar.WechatSDK.Common;
using Kugar.WechatSDK.MP.Entities;
using Kugar.WechatSDK.MP.Enums;
using Kugar.WechatSDK.MP.Results;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Kugar.WechatSDK.MP
{
    public interface IMaterialService
    {
        /// <summary>
        /// 上传临时媒体素材,有效期为3天
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="data"></param>
        /// <param name="mediaType"></param>
        /// <returns>返回上传成功后的MediaId</returns>
        Task<ResultReturn<string>> AddTemporaryMedia(string appId, Stream data,MediaType mediaType);

        /// <summary>
        /// 获取临时素材数据,并返回数据流
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="mediaId"></param>
        /// <returns></returns>
        Task<ResultReturn<Stream>> GetTemporaryMedia(string appId, string mediaId);

        /// <summary>
        /// 上传永久图文素材,返回media_id
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="articles">图文消息</param>
        /// <returns>返回图文素材的MeidaId</returns>
        Task<ResultReturn<string>> AddNews(string appId,MaterialNewItem[] articles);

        /// <summary>
        /// 上传永久图片素材,并返回url和mediaId
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="data">图片数据流</param>
        /// <returns></returns>
        Task<ResultReturn<(string url, string mediaId)>> AddImage(
            string appId,
            Stream data
        );

        /// <summary>
        /// 上传永久性的语音文件素材
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="data">语音文件数据流</param>
        /// <returns></returns>
        Task<ResultReturn<(string url, string mediaId)>> AddVoice(
            string appId,
            Stream data);

        /// <summary>
        /// 上传永久性的视频素材
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="data">视频素材数据流</param>
        /// <param name="title">视频标题</param>
        /// <param name="introduction">视频介绍</param>
        /// <returns></returns>
        Task<ResultReturn<(string url, string mediaId)>> AddVideo(
            string appId,
            Stream data,
            string title,
            string introduction
        );

        /// <summary>
        /// 获取永久性视频的数据流
        /// </summary>
        /// <param name="appID"></param>
        /// <param name="mediaId"></param>
        /// <returns></returns>
        Task<ResultReturn<Stream>> GetVideoStream(string appID, string mediaId);

        /// <summary>
        /// 获取永久性视频的信息
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="mediaId"></param>
        /// <returns></returns>
        Task<ResultReturn<GetVideoInfo_Result>> GetVideoInfo(string appId, string mediaId);

        Task<ResultReturn<Stream>> GetImageStream(
            string appId, 
            string mediaId);

        /// <summary>
        /// 获取永久性语音文件的
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="mediaId"></param>
        /// <returns></returns>
        Task<ResultReturn<Stream>> GetVoiceStream(
            string appId, 
            string mediaId);

        /// <summary>
        /// 获取永久性图文消息数据
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="mediaId"></param>
        /// <returns></returns>
        Task<ResultReturn<IReadOnlyList<MaterialNewItem>>> GetNews(string appId,
            string mediaId);

        /// <summary>
        /// 修改永久性图文信息
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="index">图文序号,,从1开始</param>
        /// <param name="mediaId">素材id</param>
        /// <param name="newItem">用于更新数据的新的图文项信息</param>
        /// <returns></returns>
        Task<ResultReturn> UpdateNewItem(string appId,string mediaId,  int index, MaterialNewItem newItem);

        /// <summary>
        /// 删除永久性素材
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="mediaId">素材id</param>
        /// <returns></returns>
        Task<ResultReturn> DeleteMedia(string appId, string mediaId);

        /// <summary>
        /// 获取素材总数
        /// </summary>
        /// <param name="appId"></param>
        /// <returns></returns>
        Task<ResultReturn<GetMediaCount_Result>> GetMediaCount(string appId);
    }

    /// <summary>
    /// 素材管理
    /// </summary>
    public class MaterialService:MPBaseService, IMaterialService
    {
        public MaterialService(ICommonApi api) : base(api)
        {
        }

        /// <summary>
        /// 上传临时媒体素材,有效期为3天
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="data"></param>
        /// <param name="mediaType"></param>
        /// <returns>返回上传成功后的MediaId</returns>
        public async Task<ResultReturn<string>> AddTemporaryMedia(string appId, Stream data,MediaType mediaType)
        {
            if (data.CanSeek)
            {
                switch (mediaType)
                {
                    case MediaType.image:
                    {
                        if (data.Length>10485760)
                        {
                            return new FailResultReturn<string>("图片最大不能超过10M");
                        }
                    }
                        break;
                    case MediaType.video:
                        if (data.Length>10485760)
                        {
                            return new FailResultReturn<string>("视频最大不能超过10M");
                        }
                        break;
                    case MediaType.voice:
                        if (data.Length>2097152)
                        {
                            return new FailResultReturn<string>("语音最大不能超过2M");
                        }
                        break;
                    case MediaType.thumb:
                        if (data.Length>65536)
                        {
                            return new FailResultReturn<string>("缩略图最大不能超过64K");
                        }
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(mediaType), mediaType, null);
                }
            }

            var mediaTypeStr = mediaType.ToString();

            var ret=await CommonApi.PostFileByForm(appId, 
                $"/cgi-bin/media/upload?access_token=ACCESS_TOKEN&type={mediaTypeStr}", 
                "media", 
                "temp", 
                data);

            return ret.Cast(ret.ReturnData.GetString("media_id"));
        }

        /// <summary>
        /// 获取临时素材数据,并返回数据流
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="mediaId"></param>
        /// <returns></returns>
        public async Task<ResultReturn<Stream>> GetTemporaryMedia(string appId, string mediaId)
        {
            var stream= await CommonApi.GetRaw(appId,$"/cgi-bin/media/get/jssdk?access_token=ACCESS_TOKEN&media_id={mediaId}");

            return new SuccessResultReturn<Stream>(stream.data);
        }

        /// <summary>
        /// 上传永久图文素材,返回media_id
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="articles">图文消息</param>
        /// <returns>返回图文素材的MeidaId</returns>
        public async Task<ResultReturn<string>> AddNews(string appId,MaterialNewItem[] articles)
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
                
                jarray.Add(item);
            }

            json.Add("articles", jarray);

            var ret = await CommonApi.Post(appId, "/cgi-bin/material/add_news?access_token=ACCESS_TOKEN", json);

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
        /// 上传永久图片素材,并返回url和mediaId
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="data">图片数据流</param>
        /// <returns></returns>
        public async Task<ResultReturn<(string url, string mediaId)>> AddImage(
            string appId,
            Stream data
        )
        {
            if (data==null)
            {
                return new FailResultReturn<(string url, string mediaId)>("数据流不能为空");
            }

            var ret=await CommonApi.PostFileByForm(appId, 
                $"/cgi-bin/media/uploadimg?access_token=ACCESS_TOKEN", 
                "media", 
                "temp", 
                data);

            return ret.Cast((ret.ReturnData.GetString("url"),ret.ReturnData.GetString("media_id")));
        }

        /// <summary>
        /// 上传永久性的语音文件素材
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="data">语音文件数据流</param>
        /// <returns></returns>
        public async Task<ResultReturn<(string url, string mediaId)>> AddVoice(
            string appId,
            Stream data)
        {
            if (data==null)
            {
                return new FailResultReturn<(string url, string mediaId)>("数据流不能为空");
            }

            var ret=await CommonApi.PostFileByForm(appId, 
                $"/cgi-bin/material/add_material?access_token=ACCESS_TOKEN&type=voice", 
                "media", 
                "temp", 
                data);
            
            return ret.Cast((ret.ReturnData.GetString("url"),ret.ReturnData.GetString("media_id")));
        }

        /// <summary>
        /// 上传永久性的视频素材
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="data">视频素材数据流</param>
        /// <param name="title">视频标题</param>
        /// <param name="introduction">视频介绍</param>
        /// <returns></returns>
        public async Task<ResultReturn<(string url, string mediaId)>> AddVideo(
            string appId,
            Stream data,
            string title,
            string introduction
            )
        {
            if (!string.IsNullOrWhiteSpace(title))
            {
                return new FailResultReturn<(string url, string mediaId)>("标题必填");
            }

            if (!string.IsNullOrWhiteSpace(introduction))
            {
                return new FailResultReturn<(string url, string mediaId)>("简介必填");
            }

            if (data==null)
            {
                return new FailResultReturn<(string url, string mediaId)>("数据流不能为空");
            }

            var ret=await CommonApi.PostByForm(appId, 
                "/cgi-bin/material/add_material?access_token=ACCESS_TOKEN&type=video", 
                ("media",data),
                ("description",new JObject()
                {
                    ["title"]=title,
                    ["introduction"]=introduction
                }.ToStringEx(Formatting.None)));

            return ret.Cast((ret.ReturnData.GetString("url"),ret.ReturnData.GetString("media_id")));
        }

        /// <summary>
        /// 获取永久性视频的数据流
        /// </summary>
        /// <param name="appID"></param>
        /// <param name="mediaId"></param>
        /// <returns></returns>
        public async Task<ResultReturn<Stream>> GetVideoStream(string appID, string mediaId)
        {
            var mediaInifo = await CommonApi.Post(appID,
                "/cgi-bin/material/get_material?access_token=ACCESS_TOKEN",
                new JObject()
                {
                    ["media_id"] = mediaId
                }
            );

            if (mediaInifo.IsSuccess)
            {
                var downloadUrl = mediaInifo.ReturnData.GetString("down_url");

                var stream= await CommonApi.GetRaw(appID, downloadUrl);

                return new SuccessResultReturn<Stream>(stream.data);
            }
            else
            {
                return mediaInifo.Cast((Stream)null);
            }
        }

        /// <summary>
        /// 获取永久性视频的信息
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="mediaId"></param>
        /// <returns></returns>
        public async Task<ResultReturn<GetVideoInfo_Result>> GetVideoInfo(string appId, string mediaId)
        {
            var mediaInifo = await CommonApi.Post(appId,
                "/cgi-bin/material/get_material?access_token=ACCESS_TOKEN",
                new JObject()
                {
                    ["media_id"] = mediaId
                }
            );

            if (mediaInifo.IsSuccess)
            {
                return new SuccessResultReturn<GetVideoInfo_Result>(
                    new GetVideoInfo_Result()
                    {
                        Title = mediaInifo.ReturnData.GetString("title"),
                        Description = mediaInifo.ReturnData.GetString("description"),
                        DownloadUrl = mediaInifo.ReturnData.GetString("down_url")
                    }
                );
            }
            else
            {
                return mediaInifo.Cast<GetVideoInfo_Result>(default);
            }
        }

        public async Task<ResultReturn<Stream>> GetImageStream(
            string appId, 
            string mediaId)
        {
            var stream= await CommonApi.PostRaw(appId, 
                "/cgi-bin/material/get_material?access_token=ACCESS_TOKEN",
                new JObject()
                {
                    ["media_id"]=mediaId
                });

            return new SuccessResultReturn<Stream>(stream.data);
        }

        /// <summary>
        /// 获取永久性语音文件的
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="mediaId"></param>
        /// <returns></returns>
        public async Task<ResultReturn<Stream>> GetVoiceStream(
            string appId, 
            string mediaId)
        {
            var stream= await CommonApi.PostRaw(appId, 
                "/cgi-bin/material/get_material?access_token=ACCESS_TOKEN",
                new JObject()
                {
                    ["media_id"]=mediaId
                });

            return new SuccessResultReturn<Stream>(stream.data);
        }

        /// <summary>
        /// 获取永久性图文消息数据
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="mediaId"></param>
        /// <returns></returns>
        public async Task<ResultReturn<IReadOnlyList<MaterialNewItem>>> GetNews(string appId,
            string mediaId)
        {
            var json= await CommonApi.Post(appId, 
                "/cgi-bin/material/get_material?access_token=ACCESS_TOKEN",
                new JObject()
                {
                    ["media_id"]=mediaId
                });

            if (json.IsSuccess)
            {
                var jsonItems = json.ReturnData.GetJObjectArray("news_item");

                var lst = new List<MaterialNewItem>();

                foreach (var jsonItem in jsonItems)
                {
                    var tmp = new MaterialNewItem()
                    {
                        Title = jsonItem.GetString("title"),
                        Author = jsonItem.GetString("author"),
                        Content = jsonItem.GetString("content"),
                        ContentSourceUrl = jsonItem.GetString("content_source_url"),
                        Digest = jsonItem.GetString("digest"),
                        NeedOpenComment = jsonItem.GetInt("need_open_comment") == 1,
                        ShowCoverPic = jsonItem.GetInt("show_cover_pic") == 1,
                        ThumbMediaId = jsonItem.GetString("thumb_media_id"),
                        OnlyFansCanComment = jsonItem.GetInt("only_fans_can_comment") == 1
                    };

                    lst.Add(tmp);
                }

                return new SuccessResultReturn<IReadOnlyList<MaterialNewItem>>(lst);
            }
            else
            {
                return json.Cast((IReadOnlyList<MaterialNewItem>)null);
            }
        }

        /// <summary>
        /// 修改永久性图文信息
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="index">图文序号,,从1开始</param>
        /// <param name="mediaId">素材id</param>
        /// <param name="newItem">用于更新数据的新的图文项信息</param>
        /// <returns></returns>
        public async Task<ResultReturn> UpdateNewItem(string appId,string mediaId,  int index, MaterialNewItem newItem)
        {
            var tmp = new JObject
            {
                {"thumb_media_id", newItem.ThumbMediaId},
                {"title", newItem.Title},
                {"content", newItem.Content},
                {"author", newItem.Author},
                {"content_source_url", newItem.ContentSourceUrl},
                {"digest", newItem.Digest},
                {"show_cover_pic", newItem.ShowCoverPic},
                {"only_fans_can_comment" ,newItem.OnlyFansCanComment==true?1:0},
                {"need_open_comment" ,newItem.NeedOpenComment==true?1:0},
            };

            var json = new JObject()
            {
                ["media_id"]=mediaId,
                ["index"]=index,
                ["articles"]=new JArray(){tmp}
            };

            var ret = await CommonApi.Post(appId, "/cgi-bin/material/update_news?access_token=ACCESS_TOKEN", json);

            if (ret.IsSuccess)
            {
                return SuccessResultReturn.Default;
            }
            else
            {
                return ret;
            }
        }

        /// <summary>
        /// 删除永久性素材
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="mediaId">素材id</param>
        /// <returns></returns>
        public async Task<ResultReturn> DeleteMedia(string appId, string mediaId)
        {
            var json= await CommonApi.Post(appId, 
                "/cgi-bin/material/del_material?access_token=ACCESS_TOKEN",
                new JObject()
                {
                    ["media_id"]=mediaId
                });

            return json;
        }

        /// <summary>
        /// 获取素材总数
        /// </summary>
        /// <param name="appId"></param>
        /// <returns></returns>
        public async Task<ResultReturn<GetMediaCount_Result>> GetMediaCount(string appId)
        {
            var json= await CommonApi.Get(appId, 
                "/cgi-bin/material/get_materialcount?access_token=ACCESS_TOKEN");

            if (json.IsSuccess)
            {
                var data = json.ReturnData;

                return new SuccessResultReturn<GetMediaCount_Result>(
                    new GetMediaCount_Result()
                    {
                        VoiceCount = data.GetInt("voice_count"),
                        VideoCount = data.GetInt("video_count"),
                        ImageCount = data.GetInt("image_count"),
                        NewsCount = data.GetInt("news_count")
                    }
                );
            }
            else
            {
                return json.Cast<GetMediaCount_Result>(default);
            }
        }


    }
}
