using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Kugar.Core.BaseStruct;
using Kugar.Core.ExtMethod;
using Kugar.WechatSDK.Common;
using Kugar.WechatSDK.MP.Results;
using Newtonsoft.Json.Linq;

namespace Kugar.WechatSDK.MP
{
    /// <summary>
    /// 推广相关的短连接及短key相关模块
    /// </summary>
    public interface IUrlService
    {
        /// <summary>
        /// 长连接转换为短连接,支持:http://、https://、weixin://wxpay
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="longUrl"></param>
        /// <returns></returns>
        Task<ResultReturn<string>> LongUrlToShortUrl(string appId, string longUrl);

        /// <summary>
        /// 将长信息数据转为短key
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="longData">长数据,最大不能超过4K</param>
        /// <param name="expire_seconds">过期时间,秒,,最大为30天</param>
        /// <returns></returns>
        Task<ResultReturn<string>> LongDataToShortKey(string appId, string longData,long expire_seconds=2592000);

        /// <summary>
        /// 通过短key获取长数据
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="shortKey">短key</param>
        /// <returns></returns>
        Task<ResultReturn<ShortKeyToLongData_Result>> ShortKeyToLongData(string appId, string shortKey);
    }

    public class UrlService:MPBaseService, IUrlService
    {
        public UrlService(ICommonApi api) : base(api)
        {
        }

        /// <summary>
        /// 长连接转换为短连接,支持:http://、https://、weixin://wxpay
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="longUrl"></param>
        /// <returns></returns>
        public async Task<ResultReturn<string>> LongUrlToShortUrl(string appId, string longUrl)
        {
            var data = await CommonApi.Post(appId, "/cgi-bin/shorturl?access_token=ACCESS_TOKEN",
                new JObject()
                {
                    ["action"] = "long2short",
                    ["long_url"] = longUrl
                }
            );

            if (data.IsSuccess)
            {
                return new SuccessResultReturn<string>(data.ReturnData.GetString("short_url"));
            }
            else
            {
                return data.Cast<string>("");
            }
        }

        /// <summary>
        /// 将长信息数据转为短key
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="longData">长数据,最大不能超过4K</param>
        /// <param name="expire_seconds">过期时间,秒,,最大为30天</param>
        /// <returns></returns>
        public async Task<ResultReturn<string>> LongDataToShortKey(string appId, string longData,long expire_seconds=2592000)
        {
            var data = await CommonApi.Post(appId, "/cgi-bin/shorten/gen?access_token=ACCESS_TOKEN",
                new JObject()
                {
                    ["expire_seconds"] = expire_seconds,
                    ["long_data"] = longData
                }
            );

            if (data.IsSuccess)
            {
                return new SuccessResultReturn<string>(data.ReturnData.GetString("short_key"));
            }
            else
            {
                return data.Cast<string>("");
            }
        }

        /// <summary>
        /// 通过短key获取长数据
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="shortKey">短key</param>
        /// <returns></returns>
        public async Task<ResultReturn<ShortKeyToLongData_Result>> ShortKeyToLongData(string appId, string shortKey)
        {
            var data = await CommonApi.Post(appId, "/cgi-bin/shorten/fetch?access_token=ACCESS_TOKEN",
                new JObject()
                {
                    ["short_key"] = shortKey
                }
            );

            if (data.IsSuccess)
            {
                return new SuccessResultReturn<ShortKeyToLongData_Result>(
                    new ShortKeyToLongData_Result()
                    {
                        LongData = data.ReturnData.GetString("long_data"),
                        ExpireSecond = data.ReturnData.GetLong("expire_seconds")
                    });
            }
            else
            {
                return data.Cast<ShortKeyToLongData_Result>(null);
            }
        }
    }
}
