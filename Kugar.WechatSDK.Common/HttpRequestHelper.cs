using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Kugar.Core.ExtMethod;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Kugar.WechatSDK.Common
{
    public class HttpRequestHelper
    {
        private IHttpClientFactory  _clientFactory = null;
        private IOptions<WechatRequestOption> _option = null;

        public HttpRequestHelper(IHttpClientFactory  client,IOptions<WechatRequestOption> option)
        {
            _clientFactory = client;
            _option = option;
        }

        public async Task<JObject> Get(string url)
        {
            var json = await SendApiJson(url, HttpMethod.Get);

            return json;
        }

        public async Task<(string contentType,IReadOnlyList<byte> data)> GetRaw(string url)
        {
            var raw = await SendApiRaw(url, HttpMethod.Get);

            return raw;
        }


        public async Task<JObject> Post(string url, JObject args)
        {
            var json = await SendApiJson(url, HttpMethod.Post, args);

            return json;
        }

        public async Task<(string contentType,IReadOnlyList<byte> data)> PostRaw(string url, JObject args)
        {
            var stream = await SendApiRaw(url, HttpMethod.Post, args);

            return stream;
        }

        public async Task<JObject> PostByForm(string url,MultipartFormDataContent formData)
        {
            Debugger.Break();

            if (!url.StartsWith("http",StringComparison.CurrentCultureIgnoreCase))
            {
                url = _option.Value.BaseApiHost + url;
            }

            using (var response = await _clientFactory.CreateClient("MPApi").SendAsync(
                new HttpRequestMessage(HttpMethod.Post, url)
                {
                    Content = formData
                }))
            {
                if (response.IsSuccessStatusCode)
                {
                    var jsonStream = await response.Content.ReadAsStringAsync();

                    return JObject.Parse(jsonStream);
                }
                else
                {
                    throw new WebException("接口访问错误") ;
                }
            }

            
        }

        public async Task<JObject> SendApiJson(string url, HttpMethod httpMethod, JObject args=null)
        {
            if (!url.StartsWith("http",StringComparison.CurrentCultureIgnoreCase))
            {
                url = _option.Value.BaseApiHost + url;
            }

            HttpResponseMessage response;
            if (httpMethod== HttpMethod.Get)
            {
                response = await _clientFactory.CreateClient("MPApi").SendAsync(new HttpRequestMessage(httpMethod,url));
            }
            else
            {
                response = await _clientFactory.CreateClient("MPApi").SendAsync(new HttpRequestMessage(httpMethod,url)
                {
                    Content = new StringContent(args.ToStringEx(Formatting.None), Encoding.UTF8,"application/json")
                });
            }

            using (response)
            {
                if (response.IsSuccessStatusCode)
                {
                    var jsonStream = await response.Content.ReadAsStringAsync();

                    return JObject.Parse(jsonStream);
                }
                else
                {
                    throw new WebException("接口访问错误") ;
                }
            }

            
        }

        public async Task<(string contentType,IReadOnlyList<byte> data)> SendApiRaw(string url, HttpMethod httpMethod, JObject args=null)
        {
            HttpResponseMessage response;

            if (httpMethod== HttpMethod.Get)
            {
                response = await _clientFactory.CreateClient("MPApi").SendAsync(new HttpRequestMessage(httpMethod,_option.Value.BaseApiHost + url));
            }
            else
            {
                response = await _clientFactory.CreateClient("MPApi").SendAsync(new HttpRequestMessage(httpMethod,_option.Value.BaseApiHost + url)
                {
                    Content = new StringContent(args.ToStringEx(Formatting.None), Encoding.UTF8,"application/json")
                });
            }

            using (response)
            {
                if (response.IsSuccessStatusCode)
                {
                    var contentType = "";

                    if (response.Headers.TryGetValues("content-type", out var v))
                    {
                        if (v.HasData())
                        {
                            contentType = v.JoinToString(',');
                        }
                    }

                    if (contentType=="")
                    {
                        if (response.Content.Headers.TryGetValues("content-type", out var v1))
                        {
                            if (v1.HasData())
                            {
                                contentType = v1.JoinToString(',');
                            }
                        }
                    }
                    

                    return (contentType,await response.Content.ReadAsByteArrayAsync());
                
                }
                else
                {
                    throw new WebException("接口访问错误") ;
                }
            }

            
        }
    }

    public class WechatRequestOption
    {
        public string BaseApiHost { set; get; }

        public string MPApiHost { set; get; }
    }
}
