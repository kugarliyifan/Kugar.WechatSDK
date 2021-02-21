using Kugar.WechatSDK.OpenPlatform.Enums;

namespace Kugar.WechatSDK.OpenPlatform.Results
{
    public class RefreshAccessToken_Result
    {
        public string AccessToken { set; get; }

        public int Expires { set; get; }

        public string RefreshToken { set; get; }

        public string OpenID { set; get; }

        public SnsapiType Type { set; get; }
    }
}
