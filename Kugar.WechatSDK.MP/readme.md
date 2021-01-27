# 本类库主要用于微信公众号接口封装

## 使用方法为:

```c#
			services.AddMemoryCache();
​            services.AddWechatGateway()
​                .AddWechatMP(new[]
​                {
​                    new MPConfiguration()
​                    {
​                        AppID = CustomConfigManager.Default["Wechat:赠券:AppID"],
​                        AppSerect = CustomConfigManager.Default["Wechat:赠券:AppSerect"],
​                        ManagerAccessToken = CustomConfigManager.Default["Wechat:赠券:ManageToken"].ToBool()
​                    }
​                });

```



在调用的时候:

public class A

{

​		public async Task<IActionnResult> Test1([FromService]IWechatMPApi mpApi)

​		{

​				//调用mpApi的各个属性使用的接口功能

​		}

}