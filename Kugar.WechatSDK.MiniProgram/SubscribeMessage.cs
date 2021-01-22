using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kugar.Core.BaseStruct;
using Kugar.Core.ExtMethod;
using Kugar.WechatSDK.Common;
using Kugar.WechatSDK.MiniProgram.Results;
using Newtonsoft.Json.Linq;

namespace Kugar.WechatSDK.MiniProgram
{
    /// <summary>
    /// 订阅消息功能
    /// </summary>
    public interface ISubscribeMessage
    {
        /// <summary>
        /// 组合模板并添加至帐号下的个人模板库
        /// </summary>
        /// <param name="appID"></param>
        /// <param name="tid"></param>
        /// <param name="kidList"></param>
        /// <param name="sceneDesc"></param>
        /// <returns>返回添加结果,returnData为模板id,send函数时,传入template_id</returns>
        Task<ResultReturn<string>> AddTemplate(
            string appID,
            string tid,
            int[] kidList,
            string sceneDesc=""
        );

        /// <summary>
        /// 删除指定的模板消息
        /// </summary>
        /// <param name="appID"></param>
        /// <param name="template_id">模板id</param>
        /// <returns></returns>
        Task<ResultReturn> DeleteTemplate(string appID, string template_id);

        /// <summary>
        /// 获取当前小程序的模板消息列表
        /// </summary>
        /// <param name="appID"></param>
        /// <returns></returns>
        Task<ResultReturn<GetTemplateListItem_Result[]>> GetTemplateList(string appID);

        /// <summary>
        /// 发送订阅消息
        /// </summary>
        /// <param name="toUserOpenID">接受者的openid</param>
        /// <param name="template_id">模板id</param>
        /// <param name="page">点击模板卡片后的跳转页面，仅限本小程序内的页面。支持带参数,（示例index?foo=bar）。该字段不填则模板无跳转</param>
        /// <param name="data">模板内容</param>
        /// <returns></returns>
        Task<ResultReturn> Send(string appID, string toUserOpenID, 
            string template_id,
            string page,
            object data
        );

        /// <summary>
        /// 发送订阅消息
        /// </summary>
        /// <param name="toUserOpenID">接受者的openid</param>
        /// <param name="template_id">模板id</param>
        /// <param name="page">点击模板卡片后的跳转页面，仅限本小程序内的页面。支持带参数,（示例index?foo=bar）。该字段不填则模板无跳转</param>
        /// <param name="data">模板内容</param>
        /// <returns></returns>
        Task<ResultReturn> Send(string appID, string toUserOpenID,
            string template_id,
            string page,
            JObject data
        );
    }

    /// <summary>
    /// 订阅消息功能
    /// </summary>
    public class SubscribeMessage:BaseService, ISubscribeMessage
    {
        public SubscribeMessage(CommonApi api) : base(api)
        {
        }

        /// <summary>
        /// 组合模板并添加至帐号下的个人模板库
        /// </summary>
        /// <param name="appID"></param>
        /// <param name="tid"></param>
        /// <param name="kidList"></param>
        /// <param name="sceneDesc"></param>
        /// <returns>返回添加结果,returnData为模板id,send函数时,传入template_id</returns>
        public async Task<ResultReturn<string>> AddTemplate(
            string appID,
            string tid,
            int[] kidList,
            string sceneDesc=""
            )
        {
            var args = new JObject()
            {
                ["tid"] = tid,
                ["kidList"] = JArray.FromObject(kidList),
                ["sceneDesc"] = sceneDesc
            };

            var result = await CommonApi.Post(appID, "/wxaapi/newtmpl/addtemplate?access_token=ACCESS_TOKEN", args);
            
            return result.Cast(result.ReturnData.GetString("priTmplId"),"");
        }

        /// <summary>
        /// 删除指定的模板消息
        /// </summary>
        /// <param name="appID"></param>
        /// <param name="template_id">模板id</param>
        /// <returns></returns>
        public async Task<ResultReturn> DeleteTemplate(string appID, string template_id)
        {
            var args = new JObject()
            {
                ["priTmplId"] = template_id
            };

            var result = await CommonApi.Post(appID, "/wxaapi/newtmpl/deltemplate?access_token=ACCESS_TOKEN", args);
            
            return result;
        }

        /// <summary>
        /// 获取当前小程序的模板消息列表
        /// </summary>
        /// <param name="appID"></param>
        /// <returns></returns>
        public async Task<ResultReturn<GetTemplateListItem_Result[]>> GetTemplateList(string appID)
        {
            var data = await CommonApi.Get(appID, "/wxaapi/newtmpl/gettemplate?access_token=ACCESS_TOKEN");

            if (data.IsSuccess)
            {
                var list = data.ReturnData.GetJObjectArray("data");

                return new SuccessResultReturn<GetTemplateListItem_Result[]>(list.Select(x =>
                    new GetTemplateListItem_Result()
                    {
                        PriTmplId = x.GetString("priTmplId"),
                        Content = x.GetString("content"),
                        Title = x.GetString("title"),
                        Example = x.GetString("example"),
                        Type = x.GetInt("type")
                    }).ToArrayEx());
            }
            else
            {
                return data.Cast<GetTemplateListItem_Result[]>(null, null);
            }
        }

        /// <summary>
        /// 发送订阅消息
        /// </summary>
        /// <param name="toUserOpenID">接受者的openid</param>
        /// <param name="template_id">模板id</param>
        /// <param name="page">点击模板卡片后的跳转页面，仅限本小程序内的页面。支持带参数,（示例index?foo=bar）。该字段不填则模板无跳转</param>
        /// <param name="data">模板内容</param>
        /// <returns></returns>
        public async Task<ResultReturn> Send(string appID, string toUserOpenID, 
            string template_id,
            string page,
            object data
        )
        {
            var msgJson = JObject.FromObject(data);

            return await Send(appID, toUserOpenID, template_id, page, msgJson);
        }

        /// <summary>
        /// 发送订阅消息
        /// </summary>
        /// <param name="toUserOpenID">接受者的openid</param>
        /// <param name="template_id">模板id</param>
        /// <param name="page">点击模板卡片后的跳转页面，仅限本小程序内的页面。支持带参数,（示例index?foo=bar）。该字段不填则模板无跳转</param>
        /// <param name="data">模板内容</param>
        /// <returns></returns>
        public async Task<ResultReturn> Send(string appID, string toUserOpenID, 
            string template_id,
            string page,
            params (string key,string value)[] data
        )
        {
            var json = new JObject();

            foreach (var item in data)
            {
                json.Add(item.key,new JObject()
                {
                    ["value"]=item.value
                });
            }

            var args = new JObject()
            {
                ["touser"] = toUserOpenID,
                ["template_id"] = template_id,
                ["page"] = page,
                ["data"] = json
            };

            var result = await CommonApi.Post(appID, "/cgi-bin/message/subscribe/send?access_token=ACCESS_TOKEN", args);

            return result;
        }

        /// <summary>
        /// 发送订阅消息
        /// </summary>
        /// <param name="toUserOpenID">接受者的openid</param>
        /// <param name="template_id">模板id</param>
        /// <param name="page">点击模板卡片后的跳转页面，仅限本小程序内的页面。支持带参数,（示例index?foo=bar）。该字段不填则模板无跳转</param>
        /// <param name="data">模板内容</param>
        /// <returns></returns>
        public async Task<ResultReturn> Send(string appID, string toUserOpenID,
            string template_id,
            string page,
            JObject data
        )
        {
            var json = new JObject();

            foreach (var item in data)
            {
                json.Add(item.Key,new JObject()
                {
                    ["value"]=item.Value
                });
            }

            var args = new JObject()
            {
                ["touser"] = toUserOpenID,
                ["template_id"] = template_id,
                ["page"] = page,
                ["data"] = json
            };

            var result = await CommonApi.Post(appID, "/cgi-bin/message/subscribe/send?access_token=ACCESS_TOKEN", args);

            return result;
        }
    }
}
