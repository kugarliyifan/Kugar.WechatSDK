using System.Collections.Generic;

namespace Kugar.WechatSDK.Common.Gateway
{
    public interface IWechatGateway
    {
        bool Add(WechatConfigurationBase config);
        WechatConfigurationBase Get(string appID);
        T Get<T>() where T : WechatConfigurationBase;

        T Get<T>(string appID) where T : WechatConfigurationBase;

        IReadOnlyCollection<T> GetList<T>() where T : WechatConfigurationBase;
        IReadOnlyList<WechatConfigurationBase> GetList();
        bool Exists(string appID);

        void Remove(string appID);

        string WechatApiHost { set; get; }
    }
}