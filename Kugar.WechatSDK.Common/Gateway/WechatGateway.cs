using System;
using System.Collections.Generic;
using System.Linq;
using Kugar.Core.ExtMethod;

namespace Kugar.WechatSDK.Common.Gateway
{
    public class WechatGateway : IWechatGateway
    {
        public List<WechatConfigurationBase> _configs = new List<WechatConfigurationBase>();
        private IAccessTokenContainer _accessTokenContainer = null;

        public WechatGateway(IAccessTokenContainer accessTokenContainer)
        {
            _accessTokenContainer = accessTokenContainer;
        }

        public bool Add(WechatConfigurationBase config)
        {
            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            if (!config.Validate())
            {
                throw new Exception("配置信息校验错误");
            }

            if (!Exists(config.AppID))
            {
                _configs.Add(config);

                //if (config.ManagerAccessToken)
                //{
                //    _accessTokenContainer.Register(config.AppID, config.AppSerect);
                //}

                return true;
            }
            else
            {
                return false;
            }
        }

        public WechatConfigurationBase Get(string appID)
        {
            return _configs.FirstOrDefault(x => x.AppID == appID);
        }

        public T Get<T>() where T : WechatConfigurationBase
        {
            var tmp = _configs.Where(x => x.GetType() == typeof(T)).ToList();

            var count = tmp.Count;

            if (count == 1)
            {
                return tmp[0] as T;
            }
            else if (count <= 0)
            {
                throw new ArgumentOutOfRangeException("指定类型配置不存在");
            }
            else
            {
                return tmp[0] as T;
            }
        }

        public T Get<T>(string appID) where T : WechatConfigurationBase
        {
            var tmp = _configs.Where(x => x.GetType() == typeof(T) && x.AppID==appID).ToList();

            var count = tmp.Count;

            if (count == 1)
            {
                return tmp[0] as T;
            }
            else if (count <= 0)
            {
                throw new ArgumentOutOfRangeException("指定类型配置不存在");
            }
            else
            {
                return tmp[0] as T;
            }
        }

        public IReadOnlyCollection<T> GetList<T>() where T : WechatConfigurationBase
        {
            return _configs.Where(x => x.GetType() == typeof(T)).Select(x => x as T).ToList();
        }

        public IReadOnlyList<WechatConfigurationBase> GetList()
        {
            return _configs;
        }

        public void Remove(string appID)
        {
            var removeItems=_configs.Remove(x => x.AppID == appID);

            if (removeItems.HasData())
            {
                foreach (var item in removeItems)
                {
                    if (item.ManagerAccessToken)
                    {
                        _accessTokenContainer.Remove(appID);
                    }
                }
            }
        }

        public string WechatApiHost { get; set; } = "https://api.weixin.qq.com";

        public bool Exists(string appID)
        {
            return _configs.Any(x => x.AppID == appID);
        }
    }
}
