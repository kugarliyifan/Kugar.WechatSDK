using System.Threading.Tasks;

namespace Kugar.WechatSDK.MP.Web
{
    public interface IWechatMPAppIdFactory
    {
        Task<string> GetAppId(Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerChallengeContext context,
            IWechatMPApi mpApi);
    }
}