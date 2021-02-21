using Kugar.WechatSDK.Common;

namespace Kugar.WechatSDK.OpenPlatform.Services
{
    public abstract class OpenPlatformBaseService
    {
        protected OpenPlatformBaseService(ICommonApi api)
        {
            CommonApi = api;
        }

        protected ICommonApi CommonApi { get; }
    }
}
