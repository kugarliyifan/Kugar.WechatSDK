# 本类库主要用于微信公众号的Web接口,提供用户授权跳转,消息接收回复等功能

## 使用方法为:

```c#
public void ConfigureServices(IServiceCollection services)
{
	services.AddMemoryCache();
	services.AddWechatGateway()
			.AddWechatMP(new[]
			{
				new MPConfiguration()
				{
					AppID = CustomConfigManager.Default["Wechat:AppID"],
					AppSerect = CustomConfigManager.Default["Wechat:AppSerect"],
					ManagerAccessToken = CustomConfigManager.Default["Wechat:ManageToken"].ToBool()  //是否由系统管理AccessToken
				}
			});

	services.AddAuthentication()
		.AddWchatMPJWT("wechat",new WechatJWTOption()   //wechat的为授权Scheme名称,微信授权回调之后,会将登陆token存入cookie
		{
			LoginService = new WechatLoginHelper,  //用于提供登录验证的功能,该类需实现IWechatJWTAuthenticateService接口,该类使用AddScoped的范围,
			AppIdFactory=null ,                        //如果该参数为空,则取配置内的第一个公众号配置的AppID,否则调用该类的AppIDFactory函数,返回AppID,可用于平台化的多AppID的情况

		})
		;

}


```




在调用的时候:
```c#
public class A
{

	public async Task<IActionnResult> Test1([FromService]IWechatMPApi mpApi)
	{

		//调用mpApi的各个属性使用的接口功能

	}
}
```