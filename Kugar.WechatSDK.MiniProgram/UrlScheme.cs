using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Kugar.Core.BaseStruct;
using Kugar.Core.ExtMethod;
using Kugar.WechatSDK.Common;
using Newtonsoft.Json.Linq;

namespace Kugar.WechatSDK.MiniProgram
{
    public interface IUrlScheme
    {
        /// <summary>
        /// 用于从H5跳入小程序
        /// </summary>
        /// <param name="query">跳转地址的查询字符串,最大128个字符,可以为空字符串</param>
        /// <param name="expire_time">到期时间,null=不过期 不为null为到期时间</param>
        /// <param name="path">跳转地址</param>
        /// <returns></returns>
        Task<ResultReturn<string>> Generate(string appID,string path,string query="", DateTime? expire_time=null);
    }

    /// <summary>
    /// UrlScheme 功能,用于从H5跳入小程序
    /// </summary>
    public class UrlScheme:BaseService, IUrlScheme
    {
        public UrlScheme(ICommonApi api) : base(api)
        {
        }

        /// <summary>
        /// 用于从H5跳入小程序
        /// </summary>
        /// <param name="query">跳转地址的查询字符串,最大128个字符,可以为空字符串</param>
        /// <param name="expire_time">到期时间,null=不过期 不为null为到期时间</param>
        /// <param name="path">跳转地址</param>
        /// <returns></returns>
        public async Task<ResultReturn<string>> Generate(string appID,string path,string query="", DateTime? expire_time=null)
        {
            var args = new JObject()
            {
                ["jump_wxa"] = new JObject()
                {
                    ["path"] = path,
                    ["query"] = query.ToStringEx()
                },
                ["is_expire"] = expire_time != null
            };

            if (expire_time.HasValue)
            {
                args.Add("expire_time",expire_time.Value.ToFileTimeUtc());    
            }
            
            var ret =await CommonApi.Post(appID, "/wxa/generatescheme?access_token=ACCESS_TOKEN", args);

            return ret.Cast(ret.ReturnData.GetString("openlink"), "");
        }
    }
}
