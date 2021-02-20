using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Kugar.Core.BaseStruct;
using Kugar.Core.ExtMethod;
using Kugar.WechatSDK.Common;
using Kugar.WechatSDK.MP.Results;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;

namespace Kugar.WechatSDK.MP
{
    /// <summary>
    /// 推广二维码接口
    /// </summary>
    public interface IQrCodeService
    {
        /// <summary>
        /// 创建一个临时推广二维码,使用字符串的场景值
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="sceneStr">场景值</param>
        /// <param name="expireSeconds">过期时间,默认为30天,最大为30天</param>
        /// <returns></returns>
        Task<ResultReturn<GenerateTemporaryQrCode_Result>> GenerateTemporaryQrCode(string appId,
            string sceneStr,
            long expireSeconds=2592000
        );

        /// <summary>
        /// 创建一个临时推广二维码,使用数字的场景值
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="sceneId">场景值</param>
        /// <param name="expireSeconds">过期时间,默认为30天,最大为30天</param>
        /// <returns></returns>
        Task<ResultReturn<GenerateTemporaryQrCode_Result>> GenerateTemporaryQrCode(string appId,
            int sceneId,
            long expireSeconds=2592000
        );

        /// <summary>
        /// 创建一个永久推广二维码,使用字符串的场景值,最大为10万个
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="sceneStr">场景值</param>
        /// <param name="expireSeconds">过期时间,默认为30天,最大为30天</param>
        /// <returns></returns>
        Task<ResultReturn<GenerateLimitQrCode_Result>> GenerateLimitQrCode(string appId,
            string sceneStr
        );

        /// <summary>
        /// 创建一个临时永久二维码,使用数字的场景值,最大为10万个
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="sceneId">场景值</param>
        /// <param name="expireSeconds">过期时间,默认为30天,最大为30天</param>
        /// <returns></returns>
        Task<ResultReturn<GenerateLimitQrCode_Result>> GenerateLimitQrCode(string appId,
            int sceneId
        );

        /// <summary>
        /// 获取指定ticket的二维码图片数据流
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="ticket"></param>
        /// <returns></returns>
        Task<ResultReturn<Stream>> DownloadQrCode(string appId, string ticket);

        /// <summary>
        /// 生成前端使用的二维码显示的链接,使用ticket
        /// </summary>
        /// <param name="ticket"></param>
        /// <returns></returns>
        Task<string> GenerateQrCodeImageUrl(string ticket);
    }

    /// <summary>
    /// 推广二维码接口
    /// </summary>
    public class QrCodeService:MPBaseService, IQrCodeService
    {
        private IOptionsMonitor<MPRequestHostOption> _option = null;

        public QrCodeService(ICommonApi api,IOptionsMonitor<MPRequestHostOption> option) : base(api)
        {
            _option = option;
        }

        /// <summary>
        /// 创建一个临时推广二维码,使用字符串的场景值
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="sceneStr">场景值</param>
        /// <param name="expireSeconds">过期时间,默认为30天,最大为30天</param>
        /// <returns></returns>
        public async Task<ResultReturn<GenerateTemporaryQrCode_Result>> GenerateTemporaryQrCode(string appId,
            string sceneStr,
            long expireSeconds=2592000
        )
        {
            if (expireSeconds>2592000)
            {
                throw new ArgumentOutOfRangeException(nameof(expireSeconds), "最大过期时间不能超过2592000秒(30天)");
            }

            var args = new JObject()
            {
                ["action_name"] = "QR_STR_SCENE",
                ["expire_seconds"] = expireSeconds,
                ["action_info"] = new JObject()
                {
                    ["scene"] = new JObject()
                    {
                        ["scene_str"] = sceneStr
                    }
                }
            };

            var ret = await CommonApi.Post(appId, "/cgi-bin/qrcode/create?access_token=TOKEN", args);

            if (ret.IsSuccess)
            {
                var data = ret.ReturnData;

                return new SuccessResultReturn<GenerateTemporaryQrCode_Result>(new GenerateTemporaryQrCode_Result()
                {
                    Ticket = data.GetString("ticket"),
                    Url = data.GetString("url"),
                    ExpireSeconds = data.GetLong("expire_seconds")
                });
            }
            else
            {
                return ret.Cast<GenerateTemporaryQrCode_Result>(null);
            }
        }

        /// <summary>
        /// 创建一个临时推广二维码,使用数字的场景值
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="sceneId">场景值</param>
        /// <param name="expireSeconds">过期时间,默认为30天,最大为30天</param>
        /// <returns></returns>
        public async Task<ResultReturn<GenerateTemporaryQrCode_Result>> GenerateTemporaryQrCode(string appId,
            int sceneId,
            long expireSeconds=2592000
        )
        {
            if (expireSeconds>2592000)
            {
                throw new ArgumentOutOfRangeException(nameof(expireSeconds), "最大过期时间不能超过2592000秒(30天)");
            }

            var args = new JObject()
            {
                ["action_name"] = "QR_SCENE",
                ["expire_seconds"] = expireSeconds,
                ["action_info"] = new JObject()
                {
                    ["scene"] = new JObject()
                    {
                        ["scene_id"] = sceneId
                    }
                }
            };

            var ret = await CommonApi.Post(appId, "/cgi-bin/qrcode/create?access_token=TOKEN", args);

            if (ret.IsSuccess)
            {
                var data = ret.ReturnData;

                return new SuccessResultReturn<GenerateTemporaryQrCode_Result>(new GenerateTemporaryQrCode_Result()
                {
                    Ticket = data.GetString("ticket"),
                    Url = data.GetString("url"),
                    ExpireSeconds = data.GetLong("expire_seconds")
                });
            }
            else
            {
                return ret.Cast<GenerateTemporaryQrCode_Result>(null);
            }
        }

        /// <summary>
        /// 创建一个永久推广二维码,使用字符串的场景值,最大为10万个
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="sceneStr">场景值</param>
        /// <param name="expireSeconds">过期时间,默认为30天,最大为30天</param>
        /// <returns></returns>
        public async Task<ResultReturn<GenerateLimitQrCode_Result>> GenerateLimitQrCode(string appId,
            string sceneStr
        )
        {
            var args = new JObject()
            {
                ["action_name"] = "QR_LIMIT_STR_SCENE",
                ["action_info"] = new JObject()
                {
                    ["scene"] = new JObject()
                    {
                        ["scene_str"] = sceneStr
                    }
                }
            };

            var ret = await CommonApi.Post(appId, "/cgi-bin/qrcode/create?access_token=TOKEN", args);

            if (ret.IsSuccess)
            {
                var data = ret.ReturnData;

                return new SuccessResultReturn<GenerateLimitQrCode_Result>(new GenerateLimitQrCode_Result()
                {
                    Ticket = data.GetString("ticket"),
                    Url = data.GetString("url")
                });
            }
            else
            {
                return ret.Cast<GenerateLimitQrCode_Result>(null);
            }
        }

        /// <summary>
        /// 创建一个临时永久二维码,使用数字的场景值,最大为10万个
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="sceneId">场景值</param>
        /// <param name="expireSeconds">过期时间,默认为30天,最大为30天</param>
        /// <returns></returns>
        public async Task<ResultReturn<GenerateLimitQrCode_Result>> GenerateLimitQrCode(string appId,
            int sceneId
        )
        {
            var args = new JObject()
            {
                ["action_name"] = "QR_LIMIT_SCENE",
                ["action_info"] = new JObject()
                {
                    ["scene"] = new JObject()
                    {
                        ["scene_id"] = sceneId
                    }
                }
            };

            var ret = await CommonApi.Post(appId, "/cgi-bin/qrcode/create?access_token=TOKEN", args);

            if (ret.IsSuccess)
            {
                var data = ret.ReturnData;

                return new SuccessResultReturn<GenerateLimitQrCode_Result>(new GenerateLimitQrCode_Result()
                {
                    Ticket = data.GetString("ticket"),
                    Url = data.GetString("url")
                });
            }
            else
            {
                return ret.Cast<GenerateLimitQrCode_Result>(null);
            }
        }

        /// <summary>
        /// 获取指定ticket的二维码图片数据流
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="ticket"></param>
        /// <returns></returns>
        public async Task<ResultReturn<Stream>> DownloadQrCode(string appId, string ticket)
        {
            try
            {
                var stream = await CommonApi.GetRaw(appId,
                    $"http://{_option.CurrentValue.MPApiHost}/cgi-bin/showqrcode?ticket={WebUtility.UrlEncode(ticket) }");

                return new SuccessResultReturn<Stream>(stream.data);
            }
            catch (WebException e)
            {
                return new FailResultReturn<Stream>(e);
            }
            
        }

        /// <summary>
        /// 生成前端使用的二维码显示的链接,使用ticket
        /// </summary>
        /// <param name="ticket"></param>
        /// <returns></returns>
        public async Task<string> GenerateQrCodeImageUrl(string ticket)
        {
            return $"http://{_option.CurrentValue.MPApiHost}/cgi-bin/showqrcode?ticket={WebUtility.UrlEncode(ticket) }";
        }
    }
}
