﻿using System;
using System.Collections.Generic;
using System.IO;
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

        Task<ResultReturn<TResponseData>> Post<TResponseData>(string appID, string url,JObject args);

        Task<ResultReturn<JObject>> Post(string appID, string url,JObject args);

        Task<Stream> PostRaw(string appID, string url, JObject args);
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

        public async Task<Stream> PostRaw(string appID, string url, JObject args)
        {
            var newUrl = await replaceUrlAccessToken(appID, url);
            
            var s =await _request.PostRaw(newUrl,args);

            return s;
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
