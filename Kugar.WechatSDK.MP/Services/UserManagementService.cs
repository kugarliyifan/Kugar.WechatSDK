using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
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
    /// 用户管理接口
    /// </summary>
    public interface IUserManagementService
    {
        /// <summary>
        /// 设置用户备注名称
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="userOpenId">用户OpenId</param>
        /// <param name="remark">备注名(30个字符内)</param>
        /// <returns></returns>
        Task<ResultReturn> SetUserRemark(string appId, string userOpenId, string remark);

        /// <summary>
        /// 通过OpenId获取订阅用户信息,如果未关注的用户,只能获取到基础信息,已关注用户可以获取到详细信息
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="userOpenId"></param>
        /// <returns></returns>
        Task<ResultReturn<SubscribeWxUserInfo_Result>> GetSubscribeUserInfo(string appId, string userOpenId);

        /// <summary>
        /// 批量通过OpenId列表获取用户信息,如果未关注的用户,只能获取到基础信息,已关注用户可以获取到详细信息
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="userOpenIds"></param>
        /// <returns></returns>
        Task<ResultReturn<IReadOnlyList<SubscribeWxUserInfo_Result>>> BatchGetSubscribeUserInfo(
            string appId, string[] userOpenIds);

        /// <summary>
        /// 分页获取用户OpenId列表
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="nextUserOpenId">上一次最后一条记录的OpenId,如果第一次获取,传空字符串</param>
        /// <returns></returns>
        Task<ResultReturn<GetUserOpenIds_Result>> GetUserOpenIds(
            string appId, string nextUserOpenId="");

        /// <summary>
        /// 获取黑名单用户列表
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="nextUserOpenId">上一次最后一条记录的OpenId,如果第一次获取,传空字符串</param>
        /// <returns></returns>
        Task<ResultReturn<GetUserOpenIds_Result>> GetBlacklistUserOpenIds(string appId,
            string nextUserOpenId="");

        /// <summary>
        /// 批量设置用户到黑名单
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="userOpenIds"></param>
        /// <returns></returns>
        Task<ResultReturn> BatchSetUserToBlacklist(string appId, string[] userOpenIds);

        /// <summary>
        /// 批量取消用户黑名单
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="userOpenIds"></param>
        /// <returns></returns>
        Task<ResultReturn> BatchCancelUserToBlacklist(string appId, string[] userOpenIds);

        IUserTagManagementService Tag { get; }
    }

    public class UserManagementService:MPBaseService, IUserManagementService
    {
        public UserManagementService(ICommonApi api,IUserTagManagementService tag) : base(api)
        {
            Tag = tag;
        }

        /// <summary>
        /// 设置用户备注名称
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="userOpenId">用户OpenId</param>
        /// <param name="remark">备注名(30个字符内)</param>
        /// <returns></returns>
        public async Task<ResultReturn> SetUserRemark(string appId, string userOpenId, string remark)
        {
            if (string.IsNullOrWhiteSpace(userOpenId))
            {
                throw new ArgumentNullException(nameof(userOpenId));
            }

            if (remark!=null && remark.Length>30)
            {
                throw new ArgumentOutOfRangeException(nameof(remark), "备注名不能超过30个字符");
            }

            var json = new JObject()
            {
                ["openid"]=userOpenId,
                ["remark"]=remark
            };
            
            var ret = await CommonApi.Post(appId, "/cgi-bin/user/info/updateremark?access_token=ACCESS_TOKEN", json);

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
        /// 通过OpenId获取订阅用户信息,如果未关注的用户,只能获取到基础信息,已关注用户可以获取到详细信息
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="userOpenId"></param>
        /// <returns></returns>
        public async Task<ResultReturn<SubscribeWxUserInfo_Result>> GetSubscribeUserInfo(string appId, string userOpenId)
        {
            var ret = await CommonApi.Get(appId,
                $"/cgi-bin/user/info?access_token=ACCESS_TOKEN&openid={userOpenId}&lang=zh_CN");

            if (ret.IsSuccess)
            {
                var data = ret.ReturnData;

                return new SuccessResultReturn<SubscribeWxUserInfo_Result>(jsonToSubscribeWxUserInfo(data));
            }
            else
            {
                return ret.Cast<SubscribeWxUserInfo_Result>(null);
            }
        }

        /// <summary>
        /// 批量通过OpenId列表获取用户信息,如果未关注的用户,只能获取到基础信息,已关注用户可以获取到详细信息
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="userOpenIds"></param>
        /// <returns></returns>
        public async Task<ResultReturn<IReadOnlyList<SubscribeWxUserInfo_Result>>> BatchGetSubscribeUserInfo(
            string appId, string[] userOpenIds)
        {
            if (!userOpenIds.HasData())
            {
                throw new ArgumentOutOfRangeException(nameof(userOpenIds));
            }

            var ret = await CommonApi.Post(appId,
                $"/cgi-bin/user/info/batchget?access_token=ACCESS_TOKEN",new JObject()
                {
                    ["user_list"]=new JArray(userOpenIds.Select(x=>new JObject()
                    {
                        ["openid"]=x.ToStringEx()
                    }))
                });

            if (ret.IsSuccess)
            {
                var data = ret.ReturnData.GetJObjectArray("user_info_list");

                var lst = new List<SubscribeWxUserInfo_Result>();

                foreach (var item in data)
                {
                    lst.Add(jsonToSubscribeWxUserInfo(item));
                }

                return new SuccessResultReturn<IReadOnlyList<SubscribeWxUserInfo_Result>>(lst);
            }
            else
            {
                return ret.Cast<IReadOnlyList<SubscribeWxUserInfo_Result>>(null);// new FailResultReturn<IReadOnlyList<SubscribeWxUserInfo_Result>>(null);
            }
        }

        /// <summary>
        /// 分页获取用户OpenId列表
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="nextUserOpenId">上一次最后一条记录的OpenId,如果第一次获取,传空字符串</param>
        /// <returns></returns>
        public async Task<ResultReturn<GetUserOpenIds_Result>> GetUserOpenIds(
            string appId, string nextUserOpenId="")
        {
            var json = await CommonApi.Get(appId,
                $"/cgi-bin/user/get?access_token=ACCESS_TOKEN&next_openid={nextUserOpenId}");

            if (json.IsSuccess)
            {
                var result = new GetUserOpenIds_Result();

                result.TotalCount = json.ReturnData.GetInt("total");
                result.CurrentCount = json.ReturnData.GetInt("count");
                result.NextOpenId = json.ReturnData.GetString("next_openid");
                result.OpenIds = json.ReturnData.GetJArray("openid").Select(x => x.ToStringEx()).ToArrayEx();

                return new SuccessResultReturn<GetUserOpenIds_Result>(result);
            }
            else
            {
                return json.Cast<GetUserOpenIds_Result>(null);
            }

        }

        /// <summary>
        /// 获取黑名单用户列表
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="nextUserOpenId">上一次最后一条记录的OpenId,如果第一次获取,传空字符串</param>
        /// <returns></returns>
        public async Task<ResultReturn<GetUserOpenIds_Result>> GetBlacklistUserOpenIds(string appId,
            string nextUserOpenId="")
        {
            var json = await CommonApi.Post(appId,
                "/cgi-bin/tags/members/getblacklist?access_token=ACCESS_TOKEN",new JObject()
                {
                    ["begin_openid"]=nextUserOpenId
                });

            if (json.IsSuccess)
            {
                var result = new GetUserOpenIds_Result();

                result.TotalCount = json.ReturnData.GetInt("total");
                result.CurrentCount = json.ReturnData.GetInt("count");
                result.NextOpenId = json.ReturnData.GetString("next_openid");
                result.OpenIds = json.ReturnData.GetJArray("openid").Select(x => x.ToStringEx()).ToArrayEx();

                return new SuccessResultReturn<GetUserOpenIds_Result>(result);
            }
            else
            {
                return json.Cast<GetUserOpenIds_Result>(null);
            }
        }

        /// <summary>
        /// 批量设置用户到黑名单
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="userOpenIds"></param>
        /// <returns></returns>
        public async Task<ResultReturn> BatchSetUserToBlacklist(string appId, string[] userOpenIds)
        {
            var json = await CommonApi.Post(appId,
                "/cgi-bin/tags/members/batchblacklist?access_token=ACCESS_TOKEN",new JObject()
                {
                    ["openid_list"]=new JArray(userOpenIds)
                });

            if (json.IsSuccess)
            {
                return SuccessResultReturn.Default;
            }
            else
            {
                return json;
            }
        }

        /// <summary>
        /// 批量取消用户黑名单
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="userOpenIds"></param>
        /// <returns></returns>
        public async Task<ResultReturn> BatchCancelUserToBlacklist(string appId, string[] userOpenIds)
        {
            var json = await CommonApi.Post(appId,
                "/cgi-bin/tags/members/batchunblacklist?access_token=ACCESS_TOKEN",new JObject()
                {
                    ["openid_list"]=new JArray(userOpenIds)
                });

            if (json.IsSuccess)
            {
                return SuccessResultReturn.Default;
            }
            else
            {
                return json;
            }
        }
             
        private SubscribeWxUserInfo_Result jsonToSubscribeWxUserInfo(JObject data)
        {
            var isSubscribe = data.GetInt("subscribe") == 1;

            if (isSubscribe)
            {
                var user = new SubscribeWxUserInfo_Result()
                {
                    City = data.GetString("city"),
                    Country = data.GetString("country"),
                    GroupId = data.GetInt("groupid"),
                    HeadImageUrl = data.GetString("headimgurl"),
                    IsSubscribe = data.GetInt("subscribe")==1,
                    NickName = data.GetString("nickname"),
                    OpenID = data.GetString("openid"),
                    Province = data.GetString("province"),
                    QrScene =data.GetString("qr_scene_str").IfEmptyOrWhileSpace(data.GetString("qr_scene")) ,
                    UnionID=data.GetString("unionid"),
                    SubscribeDt=data.GetLong("subscribe_time"),
                    Sex = data.GetInt("sex"),
                    Tags = data.GetJArray("tagid_list").Select(x=>x.ToInt()).ToArrayEx(),
                    SubscribeScene=data.GetString("subscribe_scene"),
                    Remark=data.GetString("remark"),
                    
                };

                return user;
            }
            else
            {
                return new SubscribeWxUserInfo_Result()
                {
                    OpenID = data.GetString("openid")
                };
            }

            
        }
        
        public IUserTagManagementService Tag { get; }
    }

    public interface IUserTagManagementService
    {
        /// <summary>
        /// 新增标签
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="tagName"></param>
        /// <returns></returns>
        Task<ResultReturn<int>> AddTag(string appId, string tagName);

        /// <summary>
        /// 获取指定公众号的标签列表
        /// </summary>
        /// <param name="appId"></param>
        /// <returns></returns>
        Task<ResultReturn<IReadOnlyList<(int Id, string name, int count)>>> GetTags(
            string appId);

        /// <summary>
        /// 更新标签名称
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="tagId"></param>
        /// <param name="newTagName"></param>
        /// <returns></returns>
        Task<ResultReturn> UpdateTag(string appId, int tagId, string newTagName);

        /// <summary>
        /// 删除指定id的标签
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="tagId"></param>
        /// <returns></returns>
        Task<ResultReturn> DeleteTag(string appId, int tagId);

        /// <summary>
        /// 获取指定分组下的用户信息
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="tagId">标签Id</param>
        /// <param name="lastUserOpenId">上一次获取的用户OpenId,如果是第一次获取,为空字符串,可使用接口返回的lastUserOpenId参数</param>
        /// <returns></returns>
        Task<ResultReturn<(IReadOnlyList<string> userOpenIds,string lastUserOpenId)>> GetUsersByTagId(string appId, int tagId, string lastUserOpenId="");



        /// <summary>
        /// 批量设置用户标签
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="tagId"></param>
        /// <param name="userOpenIds"></param>
        /// <returns></returns>
        Task<ResultReturn> BatchSetUserTag(string appId, int tagId, string[] userOpenIds);

        /// <summary>
        /// 批量取消用户标签
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="tagId"></param>
        /// <param name="userOpenIds"></param>
        /// <returns></returns>
        Task<ResultReturn> BatchCancelUserTag(string appId, int tagId, string[] userOpenIds);
        
    }

    internal class UserTagManagementService :MPBaseService, IUserTagManagementService
    {
        /// <summary>
        /// 新增标签
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="tagName"></param>
        /// <returns></returns>
        public async Task<ResultReturn<int>> AddTag(string appId, string tagName)
        {
            var json = new JObject()
            {
                ["tag"]=new JObject()
                {
                    ["name"]=tagName
                }
            };
            
            var ret = await CommonApi.Post(appId, "/cgi-bin/tags/create?access_token=ACCESS_TOKEN", json);

            if (ret.IsSuccess)
            {
                return new SuccessResultReturn<int>(ret.ReturnData.GetJObject("tag").GetInt("id"));
            }
            else
            {
                return ret.Cast(-1);
            }
        }

        /// <summary>
        /// 获取指定公众号的标签列表
        /// </summary>
        /// <param name="appId"></param>
        /// <returns></returns>
        public async Task<ResultReturn<IReadOnlyList<(int Id, string name, int count)>>> GetTags(
            string appId)
        {
            var data = await CommonApi.Get(appId, "/cgi-bin/tags/get?access_token=ACCESS_TOKEN");

            if (data.IsSuccess)
            {
                var tags = data.ReturnData.GetJObjectArray("tags");

                var lst = new List<(int tagId, string tagName, int count)>();

                foreach (var tag in tags)
                {
                    lst.Add(
                        (
                            tag.GetInt("id"),
                            tag.GetString("name"),
                            tag.GetInt("count")
                        )
                    );
                }

                return new SuccessResultReturn<IReadOnlyList<(int Id, string name, int count)>>(lst);
            }
            else
            {
                return data.Cast<IReadOnlyList<(int Id, string name, int count)>>(null);
            }
        }

        /// <summary>
        /// 更新标签名称
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="tagId"></param>
        /// <param name="newTagName"></param>
        /// <returns></returns>
        public async Task<ResultReturn> UpdateTag(string appId, int tagId, string newTagName)
        {
            var json = new JObject()
            {
                ["tag"]=new JObject()
                {
                    ["id"]=tagId,
                    ["name"]=newTagName
                }
            };
            
            var ret = await CommonApi.Post(appId, "/cgi-bin/tags/update?access_token=ACCESS_TOKEN", json);

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
        /// 删除指定id的标签
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="tagId"></param>
        /// <returns></returns>
        public async Task<ResultReturn> DeleteTag(string appId, int tagId)
        {
            var json = new JObject()
            {
                ["tag"]=new JObject()
                {
                    ["id"]=tagId
                }
            };
            
            var ret = await CommonApi.Post(appId, "/cgi-bin/tags/delete?access_token=ACCESS_TOKEN", json);

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
        /// 获取指定分组下的用户信息
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="tagId">标签Id</param>
        /// <param name="lastUserOpenId">上一次获取的用户OpenId,如果是第一次获取,为空字符串,可使用接口返回的lastUserOpenId参数</param>
        /// <returns></returns>
        public async Task<ResultReturn<(IReadOnlyList<string> userOpenIds,string lastUserOpenId)>> GetUsersByTagId(string appId, int tagId, string lastUserOpenId="")
        {
            var json = new JObject()
            {
                ["tagid"]=tagId
            };

            json.AddPropertyIf(!string.IsNullOrWhiteSpace(lastUserOpenId), "next_openid", lastUserOpenId);
            
            var ret = await CommonApi.Post(appId, "/cgi-bin/user/tag/get?access_token=ACCESS_TOKEN", json);

            if (ret.IsSuccess)
            {
                var data = ret.ReturnData.GetJObject("data");

                var openIds = data.GetJArray("openid").Select(x => (string) x).ToArrayEx();
                var nextOpenId = data.GetString("next_openid");

                return new SuccessResultReturn<(IReadOnlyList<string> userOpenIds, string lastUserOpenId)>((openIds,nextOpenId));
            }
            else
            {
                return ret.Cast<(IReadOnlyList<string> userOpenIds, string lastUserOpenId)>((null,""));
            }
        }

        /// <summary>
        /// 批量设置用户标签
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="tagId"></param>
        /// <param name="userOpenIds"></param>
        /// <returns></returns>
        public async Task<ResultReturn> BatchSetUserTag(string appId, int tagId, string[] userOpenIds)
        {
            if (!userOpenIds.HasData())
            {
                throw new ArgumentNullException(nameof(userOpenIds));
            }

            if (tagId<0)
            {
                throw new ArgumentOutOfRangeException(nameof(tagId));
            }

            var json = new JObject()
            {
                ["openid_list"]=new JArray(userOpenIds),
                ["tagid"]=tagId
            };

            var ret = await CommonApi.Post(appId, "/cgi-bin/tags/members/batchtagging?access_token=ACCESS_TOKEN", json);

            if (ret.IsSuccess)
            {
                return SuccessResultReturn.Default;
            }
            else
            {
                return ret ;
            }
        }

        /// <summary>
        /// 批量取消用户标签
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="tagId"></param>
        /// <param name="userOpenIds"></param>
        /// <returns></returns>
        public async Task<ResultReturn> BatchCancelUserTag(string appId, int tagId, string[] userOpenIds)
        {
            if (!userOpenIds.HasData())
            {
                throw new ArgumentNullException(nameof(userOpenIds));
            }

            if (tagId<0)
            {
                throw new ArgumentOutOfRangeException(nameof(tagId));
            }

            var json = new JObject()
            {
                ["openid_list"]=new JArray(userOpenIds),
                ["tagid"]=tagId
            };

            var ret = await CommonApi.Post(appId, "/cgi-bin/tags/members/batchuntagging?access_token=ACCESS_TOKEN", json);

            if (ret.IsSuccess)
            {
                return SuccessResultReturn.Default;
            }
            else
            {
                return ret ;
            }
        }

        public async Task<ResultReturn<IReadOnlyList<int>>> GetUserTags(string appId, string userOpenId)
        {
            if (string.IsNullOrWhiteSpace(userOpenId))
            {
                throw new ArgumentNullException(nameof(userOpenId));
            }

            var json = new JObject()
            {
                ["openid"]=userOpenId
            };

            var ret = await CommonApi.Post(appId, "/cgi-bin/tags/getidlist?access_token=ACCESS_TOKEN", json);

            if (ret.IsSuccess)
            {
                var tags = ret.ReturnData.GetJArray("tagid_list").Select(x => (x.ToStringEx()).ToInt()).ToArrayEx();

                return new SuccessResultReturn<IReadOnlyList<int>>(tags);
            }
            else
            {
                return ret.Cast<IReadOnlyList<int>>(null) ;
            }
        }

        public UserTagManagementService(ICommonApi api) : base(api)
        {
        }
    }
}
