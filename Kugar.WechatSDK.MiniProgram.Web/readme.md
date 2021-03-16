# 本类库主要用于微信小程序的Web接口,Url验证等功能


## Token的验证地址Url为: Core/MiniProgram/Service/{appID}  //将{appID} 替换为你的小程序AppID



## 使用方法为:

```c#
public void ConfigureServices(IServiceCollection services)
{
	services.AddMemoryCache();
	services.AddWechatGateway()
			.AddWechatMiniProgram(new[]
			{
				new MiniProgramConfiguration()
				{
					AppID = CustomConfigManager.Default["Wechat:AppID"],
					AppSerect = CustomConfigManager.Default["Wechat:AppSerect"],
					Token=CustomConfigManager.Default["Wechat:Token"],
					ManagerAccessToken = CustomConfigManager.Default["Wechat:ManageToken"].ToBool()  //是否由系统管理AccessToken
				}
			});

	services.AddAuthentication()
		.AddWebJWT("api",new WebJWTOption()  //添加授权验证
                {
                    LoginService = new ApiLoginService(),
                    TokenEncKey = "Wk95Yh5gIl$OLL#@",
                    Audience = "api",
                    Issuer = "ss",
                    OnChallenge = async (context) =>
                    {
                        context.Response.ContentType = "application/json";
                        context.Response.StatusCode = 200;
                        await context.Response.WriteAsync(JsonConvert.SerializeObject(new FailResultReturn("用户登录无效")  //返回登录要求给小程序
                            {ReturnCode = 401}));

                        context.HandleResponse();
                    }
                });
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