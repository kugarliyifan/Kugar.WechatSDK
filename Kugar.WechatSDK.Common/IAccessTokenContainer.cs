using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Kugar.WechatSDK.Common
{
    /// <summary>
    /// AccessToken管理器
    /// </summary>
    public interface IAccessTokenContainer
    {
        /// <summary>
        /// 获取指定AppId的AccessToken
        /// </summary>
        /// <param name="appID"></param>
        /// <returns></returns>
        Task<string> GetAccessToken(string appID);

        /// <summary>
        /// 注册token
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="appSerect"></param>
        /// <returns></returns>
        bool Register(string appId, string appSerect);

        /// <summary>
        /// 通过factory返回指定appId的Accesstoken,一般用于从第三方直接获取
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="factory"></param>
        /// <returns></returns>
        bool Register(string appId, AccessTokenFactory factory);

        /// <summary>
        /// 是否存在指定AppId
        /// </summary>
        /// <param name="appId"></param>
        /// <returns></returns>
        bool Exists(string appId);

        /// <summary>
        /// 移除AppId
        /// </summary>
        /// <param name="appID"></param>
        void Remove(string appID);

        IEnumerable<(string appID, string token)> GetAllTokens();

        /// <summary>
        /// 强制刷新AccessToken
        /// </summary>
        /// <param name="appID"></param>
        /// <returns></returns>
        Task  RefreshAccessToken(string appID);

        /// <summary>
        /// 检查指定AppId的AccessToken是否可用
        /// </summary>
        /// <param name="appID"></param>
        /// <returns></returns>
        Task<bool> CheckAccessToken(string appID);

        event EventHandler<TokenRefreshEventArgs> TokenRefresh;
    }
     
    public class TokenRefreshEventArgs : EventArgs
    {
        public TokenRefreshEventArgs(string appId )
        {
            AppId = appId; 
        }

        public string AppId { get; }
         
    }
}