using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Kugar.Core.BaseStruct;
using Kugar.Core.ExtMethod;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Kugar.WechatSDK.Common
{
    public interface ICommonApi
    {
        Task<ResultReturn<TResponseData>> Get<TResponseData>(string appID, string url);

        Task<ResultReturn<JObject>> Get(string appID, string url);

        Task<(string contentType, Stream data)> GetRaw(string appId, string url);

        Task<ResultReturn<TResponseData>> Post<TResponseData>(string appID, string url,JObject args);

        Task<ResultReturn<JObject>> Post(string appID, string url,JObject args);

        Task<(string contentType, Stream data)> PostRaw(string appID, string url, JObject args);

        Task<ResultReturn<JObject>> PostByForm(string appID, string url,params (string key, object data)[] dic);

        Task<ResultReturn<JObject>> PostFileByForm(string appID, string url, string key, string filename, Stream data);
    }

    public class CommonApi : ICommonApi
    {
        private readonly IAccessTokenContainer _accessTokenContainer = null;
        private readonly HttpRequestHelper _request = null;
        private readonly IAccessTokenFactory _accessTokenFactory = null;

        public CommonApi(IAccessTokenContainer accessTokenContainer,IAccessTokenFactory accessTokenFactory, HttpRequestHelper request)
        {
            _accessTokenContainer = accessTokenContainer;
            _accessTokenFactory = accessTokenFactory;
            _request = request;
        }

        public async Task<ResultReturn<TResponseData>> Get<TResponseData>(string appID, string url)
        {
            var newUrl = await replaceUrlAccessToken(appID, url);
            
            var reTryCount = 3;
            
            var s =await _request.Get(newUrl);

            var errorCode = s.GetInt("errcode");
 
            while (errorCode==40001 || reTryCount>0)
            {
                s =await _request.Get(newUrl);

                if (errorCode==0)
                {
                    break; 
                }
                errorCode = s.GetInt("errcode");
                reTryCount--;
            }

            if (errorCode==0)
            {
                return new SuccessResultReturn<TResponseData>(s.ToObject<TResponseData>());
            }
            else
            {

                return new FailResultReturn<TResponseData>(s.GetString("errmsg"), errorCode);
            }
        }

        public async Task<ResultReturn<JObject>> Get(string appID, string url)
        {
            var newUrl = await replaceUrlAccessToken(appID, url);
            
            var reTryCount = 3;
            
            var s =await _request.Get(newUrl);

            var errorCode = s.GetInt("errcode");
 
            while (errorCode==4001 || reTryCount<=0)
            {
                s =await _request.Get(newUrl);

                if (errorCode==0)
                {
                    break; 
                }
                errorCode = s.GetInt("errcode");
                reTryCount--;
            }

            if (errorCode==0)
            {
                return new SuccessResultReturn<JObject>(s);
            }
            else
            {

                return new FailResultReturn<JObject>(s.GetString("errmsg"), errorCode);
            }
        }

        public async Task<(string contentType, Stream data)> GetRaw(string appId, string url)
        {
            var newUrl = await replaceUrlAccessToken(appId, url);
            
            //var reTryCount = 3;
            
            var s =await _request.GetRaw(newUrl);

            return s;

            //var errorCode = s.GetInt("errcode");
 
            //while (errorCode==4001 || reTryCount<=0)
            //{
            //    s =await _request.Get(newUrl);

            //    if (errorCode==0)
            //    {
            //        break; 
            //    }
            //    errorCode = s.GetInt("errcode");
            //    reTryCount--;
            //}

            //if (errorCode==0)
            //{
            //    return new SuccessResultReturn<JObject>(s);
            //}
            //else
            //{

            //    return new FailResultReturn<JObject>(s.GetString("errmsg"), errorCode);
            //}
        }

        public async Task<ResultReturn<TResponseData>> Post<TResponseData>(string appID, string url,JObject args)
        {
            var newUrl = await replaceUrlAccessToken(appID, url);
            
            var reTryCount = 3;
            
            var s =await _request.Post(newUrl,args);

            var errorCode = s.GetInt("errcode");
 
            while (errorCode==4001 || reTryCount<=0)
            {
                s =await _request.Post(newUrl,args);

                if (errorCode==0)
                {
                    break; 
                }
                errorCode = s.GetInt("errcode");
                reTryCount--;
            }

            if (errorCode==0)
            {
                return new SuccessResultReturn<TResponseData>(s.ToObject<TResponseData>());
            }
            else
            {

                return new FailResultReturn<TResponseData>(s.GetString("errmsg"), errorCode);
            }
        }

        public async Task<ResultReturn<JObject>> Post(string appID, string url, JObject args)
        {
            var newUrl = await replaceUrlAccessToken(appID, url);
            
            var reTryCount = 3;
            
            var s =await _request.Post(newUrl,args);

            var errorCode = s.GetInt("errcode");
 
            while (errorCode==4001 || reTryCount<=0)
            {
                s =await _request.Post(newUrl,args);

                if (errorCode==0)
                {
                    break; 
                }
                errorCode = s.GetInt("errcode");
                reTryCount--;
            }

            if (errorCode==0)
            {
                return new SuccessResultReturn<JObject>(s);
            }
            else
            {

                return new FailResultReturn<JObject>(s.GetString("errmsg"), errorCode);
            }
        }

        public async Task<(string contentType, Stream data)> PostRaw(string appID, string url, JObject args)
        {
            var newUrl = await replaceUrlAccessToken(appID, url);
            
            var s =await _request.PostRaw(newUrl,args);

            return s;
        }

        public async Task<ResultReturn<JObject>> PostByForm(string appID, string url,params (string key, object data)[] dic)
        {
            var newUrl = await replaceUrlAccessToken(appID, url);


            var form = new MultipartFormDataContent();

            foreach (var item in dic)
            {
                if (item.data is Stream stream)
                {
                    form.Add(new StreamContent(stream),item.key);    
                }
                else if(item.data is byte[] b)
                {
                    form.Add(new ByteArrayContent(b),item.key);    
                }
                else
                {
                    form.Add(new StringContent(item.data.ToStringEx(),Encoding.UTF8),item.key);
                }
            }

            var reTryCount = 3;
            
            var s =await _request.Get(newUrl);

            var errorCode = s.GetInt("errcode");
 
            while (errorCode==4001 || reTryCount<=0)
            {
                s =await _request.PostByForm(newUrl,form);

                if (errorCode==0)
                {
                    break; 
                }
                errorCode = s.GetInt("errcode");
                reTryCount--;
            }

            if (errorCode==0)
            {
                return new SuccessResultReturn<JObject>(s);
            }
            else
            {

                return new FailResultReturn<JObject>(s.GetString("errmsg"), errorCode);
            }
        }

        public async Task<ResultReturn<JObject>> PostFileByForm(string appID, string url,string key, string filename, Stream data)
        {
            var newUrl = await replaceUrlAccessToken(appID, url);


            var form = new MultipartFormDataContent();

            form.Add(new StreamContent(data),key,filename);
            
            var reTryCount = 3;
            
            var s =await _request.Get(newUrl);

            var errorCode = s.GetInt("errcode");
 
            while (errorCode==4001 || reTryCount<=0)
            {
                s =await _request.PostByForm(newUrl,form);

                if (errorCode==0)
                {
                    break; 
                }
                errorCode = s.GetInt("errcode");
                reTryCount--;
            }

            if (errorCode==0)
            {
                return new SuccessResultReturn<JObject>(s);
            }
            else
            {

                return new FailResultReturn<JObject>(s.GetString("errmsg"), errorCode);
            }
        }

        private async Task<string> replaceUrlAccessToken(string appID, string url)
        {
            if (url.Contains("ACCESS_TOKEN"))
            {
                var accessToken = "";

                if (_accessTokenContainer.Exists(appID))
                {
                    accessToken =  await _accessTokenContainer.GetAccessToken(appID);
                }
                else
                {
                    accessToken = await _accessTokenFactory.GetAccessToken(appID);
                }

                var newUrl = url.Replace("ACCESS_TOKEN",accessToken);

                return newUrl;
            }
            else
            {
                return url;
            }
        }
    }
    
}
