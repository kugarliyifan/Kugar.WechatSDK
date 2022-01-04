using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Kugar.Core.BaseStruct;
using Kugar.Core.ExtMethod;
using Kugar.WechatSDK.Common;
using Kugar.WechatSDK.MiniProgram.Enums;
using Newtonsoft.Json.Linq;

namespace Kugar.WechatSDK.MiniProgram
{
    /// <summary>
    /// 对图片或其他媒体文件的内容进行安全审查,检查是否有违法违规内容
    /// </summary>
    public interface IContentSecurityService
    {
        /// <summary>
        /// 校验一张图片是否含有违法违规内容。详见内容安全解决方案
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="imageData">要检测的图片文件，格式支持PNG、JPEG、JPG、GIF，图片尺寸不超过 750px x 1334px</param>
        /// <returns> isSuccess=true 表示合法 <br/> isSuccess=false 并且 returnCode=87014 表示含有违法内容;<br/> </returns>
        Task<ResultReturn> ImageSecCheck(string appId, Stream imageData);

        /// <summary>
        /// 检查一段文本是否含有违法违规内容
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="content">要检测的文本内容，长度不超过 500KB</param>
        /// <returns>isSuccess=true 表示合法 <br/> isSuccess=false 并且 returnCode=87014 表示含有违法内容;<br/> </returns>
        Task<ResultReturn> MsgSecCheck(string appId, string content);

        /// <summary>
        /// 异步校验图片/音频是否含有违法违规内容。
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="mediaUrl"></param>
        /// <param name="mediaType"></param>
        /// <returns>isSuccess=true 表示内容提交成功,等待审核,returnData为任务id，用于匹配异步推送结果</returns>
        Task<ResultReturn<string>> MediaSecCheck(string appId, string mediaUrl,SecCheckMediaType mediaType);
    }

    public class ContentSecurityService:BaseService, IContentSecurityService
    {
        public ContentSecurityService(ICommonApi api) : base(api)
        {
        }

        /// <summary>
        /// 校验一张图片是否含有违法违规内容。详见内容安全解决方案
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="imageData">要检测的图片文件，格式支持PNG、JPEG、JPG、GIF，图片尺寸不超过 750px x 1334px</param>
        /// <returns> isSuccess=true 表示合法 <br/> isSuccess=false 并且 returnCode=87014 表示含有违法内容;<br/> </returns>
        public async Task<ResultReturn> ImageSecCheck(string appId, Stream imageData)
        {
            if (imageData==null)
            {
                return new FailResultReturn<(string url, string mediaId)>("数据流不能为空");
            }

            var ret=await CommonApi.PostFileByForm(appId, 
                $"/wxa/img_sec_check?access_token=ACCESS_TOKEN", 
                "media", 
                "temp", 
                imageData);

            return ret;
        }

        /// <summary>
        /// 检查一段文本是否含有违法违规内容
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="content">要检测的文本内容，长度不超过 500KB</param>
        /// <returns>isSuccess=true 表示合法 <br/> isSuccess=false 并且 returnCode=87014 表示含有违法内容;<br/> </returns>
        public async Task<ResultReturn> MsgSecCheck(string appId, string content)
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                return new FailResultReturn<(string url, string mediaId)>("文本内容不能为空");
            }

            var ret=await CommonApi.Post(appId, 
                $"/wxa/msg_sec_check?access_token=ACCESS_TOKEN", 
                new JObject()
                {
                    ["content"]=content
                });

            return ret;
        }

        /// <summary>
        /// 异步校验图片/音频是否含有违法违规内容。
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="mediaUrl"></param>
        /// <param name="mediaType"></param>
        /// <returns>isSuccess=true 表示内容提交成功,等待审核,returnData为任务id，用于匹配异步推送结果</returns>
        public async Task<ResultReturn<string>> MediaSecCheck(string appId, string mediaUrl,SecCheckMediaType mediaType)
        {
            if (string.IsNullOrWhiteSpace(mediaUrl))
            {
                return new FailResultReturn<string>("文本内容不能为空");
            }

            var ret=await CommonApi.Post(appId, 
                $"/wxa/media_check_async?access_token=ACCESS_TOKEN", 
                new JObject()
                {
                    ["media_url"]=mediaUrl,
                    ["media_type"]=(int)mediaType
                });

            return ret.Cast(ret.ReturnData.GetString("trace_id"), "");
        }
    }
}
