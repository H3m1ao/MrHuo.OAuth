# MrHuo.OAuth

> ǰ��������.net core ��Ŀ�� .net framework 4.6 ����

���ٽ��� OAuth ��¼��

������ַ��[https://oauthlogin.net/](https://oauthlogin.net/)

#### ��֧��ƽ̨

- [x] �ٶ�
- [x] ΢��
- [x] Gitlab
- [x] Gitee
- [x] Github
- [x] ��Ϊ
...����ƽ̨����������

1.`Startup.cs`

```
public void ConfigureServices(IServiceCollection services)
{
    //����������¼���ע���ȥ
    services.AddSingleton(new Baidu.BaiduOAuth(OAuthConfig.LoadFrom(Configuration, "oauth:baidu")));
    services.AddSingleton(new Wechat.WechatOAuth(OAuthConfig.LoadFrom(Configuration, "oauth:wechat")));
    services.AddSingleton(new Gitlab.GitlabOAuth(OAuthConfig.LoadFrom(Configuration, "oauth:gitlab")));
    services.AddSingleton(new Gitee.GiteeOAuth(OAuthConfig.LoadFrom(Configuration, "oauth:gitee")));
    //... ������¼��ʽ
}
```

2.`OAuthController.cs` ����ʵ����Ҫ��������

```
public class OAuthController : Controller
{
    [HttpGet("oauth/{type}")]
    public IActionResult Index(
        string type,
        [FromServices] BaiduOAuth baiduOAuth,
        [FromServices] WechatOAuth wechatOAuth
    )
    {
        var redirectUrl = "";
        switch (type.ToLower())
        {
            case "baidu":
                {
                    redirectUrl = baiduOAuth.GetAuthorizeUrl();
                    break;
                }
            case "wechat":
                {
                    redirectUrl = wechatOAuth.GetAuthorizeUrl();
                    break;
                }
            default:
                return ReturnToError($"û��ʵ�֡�{type}����¼��ʽ��");
        }
        return Redirect(redirectUrl);
    }

    [HttpGet("oauth/{type}callback")]
    public async Task<IActionResult> LoginCallback(
        string type,
        [FromServices] BaiduOAuth baiduOAuth,
        [FromServices] WechatOAuth wechatOAuth,
        [FromQuery] string code,
        [FromQuery] string state)
    {
        try
        {
            switch (type.ToLower())
            {
                case "baidu":
                    {
                        var authorizeResult = await baiduOAuth.AuthorizeCallback(code, state);
                        if (!authorizeResult.IsSccess)
                        {
                            throw new Exception(authorizeResult.ErrorMessage);
                        }
                        return Json(authorizeResult);
                    }
                case "wechat":
                    {
                        var authorizeResult = await wechatOAuth.AuthorizeCallback(code, state);
                        if (!authorizeResult.IsSccess)
                        {
                            throw new Exception(authorizeResult.ErrorMessage);
                        }
                        return Json(authorizeResult);
                    }
                default:
                    throw new Exception($"û��ʵ�֡�{type}����¼�ص���");
            }
        }
        catch (Exception ex)
        {
            return Content(ex.Message);
        }
    }
}
```

3.`Views`

```
<!--�ڴ����з�����Ȩ��ť-->
<a href="/oauth/baidu">Baidu ��¼</a>
<a href="/oauth/wechat">Wechat ɨ���¼</a>
<!-- //������¼��ʽ����������д -->
```


#### ��չ

��չ����ƽ̨�ǳ����ף��� `Gitee` ƽ̨�Ĵ�����˵��[https://github.com/mrhuo/MrHuo.OAuth/tree/main/MrHuo.OAuth.Gitee](https://github.com/mrhuo/MrHuo.OAuth/tree/main/MrHuo.OAuth.Gitee)

##### ��һ������ƽ̨��Ӧ OAuth �ĵ����ҵ���ȡ�û���Ϣ�ӿڷ���JSON��ת��Ϊ C# ʵ���ࡣ���£�

> �����Լ���Ҫ�ͽӿڱ�׼����չ�û�����

```
public class GiteeUserModel : IUserInfoModel
{
    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("avatar_url")]
    public string Avatar { get; set; }

    [JsonPropertyName("message")]
    public string ErrorMessage { get; set; }

    [JsonPropertyName("email")]
    public string Email { get; set; }

    [JsonPropertyName("blog")]
    public string Blog { get; set; }

    //...����������������
}
```

##### �ڶ�����д��Ӧƽ̨����Ȩ�ӿ�

```
/// <summary>
/// https://gitee.com/api/v5/oauth_doc#/
/// </summary>
public class GiteeOAuth : OAuthLoginBase<GiteeUserModel>
{
    public GiteeOAuth(OAuthConfig oauthConfig) : base(oauthConfig) { }
    protected override string AuthorizeUrl => "https://gitee.com/oauth/authorize";
    protected override string AccessTokenUrl => "https://gitee.com/oauth/token";
    protected override string UserInfoUrl => "https://gitee.com/api/v5/user";
}
```

����ע�ͣ��ܹ�ʮ�У������������ǳ����㡣�����ƽ̨Э����ѭ OAuth2 ��׼��������ô����ô���оͺ��ˡ�

�����޸��ֶε�΢�ŵ�¼ʵ�֣�Ҳ�������ӣ�ֻ��Ҫ�������������OK���������£�

```
/// <summary>
/// Wechat OAuth ����ĵ��ο���
/// <para>https://developers.weixin.qq.com/doc/offiaccount/OA_Web_Apps/Wechat_webpage_authorization.html</para>
/// </summary>
public class WechatOAuth : OAuthLoginBase<WechatAccessTokenModel, WechatUserInfoModel>
{
    public WechatOAuth(OAuthConfig oauthConfig) : base(oauthConfig) { }
    protected override string AuthorizeUrl => "https://open.weixin.qq.com/connect/oauth2/authorize";
    protected override string AccessTokenUrl => "https://api.weixin.qq.com/sns/oauth2/access_token";
    protected override string UserInfoUrl => "https://api.weixin.qq.com/sns/userinfo";
    protected override Dictionary<string, string> BuildAuthorizeParams(string state)
    {
        return new Dictionary<string, string>()
        {
            ["response_type"] = "code",
            ["appid"] = oauthConfig.AppId,
            ["redirect_uri"] = System.Web.HttpUtility.UrlEncode(oauthConfig.RedirectUri),
            ["scope"] = oauthConfig.Scope,
            ["state"] = state
        };
    }
    public override string GetAuthorizeUrl(string state = "")
    {
        return $"{base.GetAuthorizeUrl(state)}#wechat_redirect";
    }
    protected override Dictionary<string, string> BuildGetAccessTokenParams(Dictionary<string, string> authorizeCallbackParams)
    {
        return new Dictionary<string, string>()
        {
            ["grant_type"] = "authorization_code",
            ["appid"] = $"{oauthConfig.AppId}",
            ["secret"] = $"{oauthConfig.AppKey}",
            ["code"] = $"{authorizeCallbackParams["code"]}"
        };
    }
    protected override Dictionary<string, string> BuildGetUserInfoParams(WechatAccessTokenModel accessTokenModel)
    {
        return new Dictionary<string, string>()
        {
            ["access_token"] = accessTokenModel.AccessToken,
            ["openid"] = accessTokenModel.OpenId,
            ["lang"] = "zh_CN",
        };
    }
}
```