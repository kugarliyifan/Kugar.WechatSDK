using Kugar.WechatSDK.OpenPlatform.Enums;

namespace Kugar.WechatSDK.OpenPlatform.Results
{
    public class GetAccessToken_Result
    {
        public string AccessToken { set; get; }

        public int Expires { set; get; }

        public string RefreshToken { set; get; }

        public string OpenId { set; get; }

        public string UionId { set; get; }

        public SnsapiType Type { set; get; }
    }
}
