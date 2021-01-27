using System.Collections.Generic;
using System.Threading.Tasks;

namespace Kugar.WechatSDK.Common
{
    public interface IAccessTokenContainer
    {
        Task<string> GetAccessToken(string appID);

        bool Register(string appId, string appSerect);

        bool Exists(string appId);

        void Remove(string appID);

        IEnumerable<(string appID, string token)> GetAllTokens();

        Task<string> RefreshAccessToken(string appID);

        Task<bool> CheckAccessToken(string appID);
    }
}