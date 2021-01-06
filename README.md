# MrHuo.OAuth

> ǰ��������.net core ��Ŀ�� .net framework 4.6 ����

���ٽ��� OAuth ��¼��

������ַ��[https://oauthlogin.net/](https://oauthlogin.net/)

#### ��֧��ƽ̨

- [x] Github
- [x] QQ
- [x] ΢��
- [x] ��Ϊ
- [x] Gitee
- [x] �ٶ�
- [ ] ֧����
- [ ] CSDN
- [ ] ����
- [ ] Microsoft
...����ƽ̨������

1.`Startup.cs`

```
public void ConfigureServices(IServiceCollection services)
{
    services.AddSession();
    services.AddHttpContextAccessor();

    services.AddSingleton<GithubOAuth>();
    services.AddSingleton<WechatOAuth>();
    //... ������¼��ʽ
}
```

```
public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
{
    //...
    app.UseSession();
    //...
}
```

2.`OAuthController.cs` ����ʵ����Ҫ��������

```
public class OAuthController : Controller
{
    private readonly Github.GithubOAuth githubOauth = null;
    private readonly Wechat.WechatOAuth wechatOAuth = null;

    public OAuthController(
        Github.GithubOAuth githubOauth,
        Wechat.WechatOAuth wechatOAuth
    )
    {
        this.githubOauth = githubOauth;
        this.wechatOAuth = wechatOAuth;
    }

    [HttpGet("oauth/{type}")]
    public void Index(string type)
    {
        switch (type.ToLower())
        {
            case "github":
                {
                    githubOauth.Authorize();
                    break;
                }
            case "wechat":
                {
                    wechatOAuth.Authorize();
                    break;
                }
            default:
                HttpContext.Response.StatusCode = 404;
                break;
        }
    }

    [HttpGet("oauth/{type}callback")]
    public IActionResult LoginCallback(string type)
    {
        switch (type.ToLower())
        {
            case "github":
                {
                    var accessToken = githubOauth.GetAccessToken(Request.Query["code"], Request.Query["state"]);
                    var userInfo = githubOauth.GetUserInfo(accessToken);
                    return Json(new
                    {
                        accessToken,
                        userInfo
                    });
                }
            case "wechat":
                {
                    var accessToken = wechatOAuth.GetAccessToken(Request.Query["code"], Request.Query["state"]);
                    var userInfo = wechatOAuth.GetUserInfo(accessToken);
                    return Json(new
                    {
                        accessToken,
                        userInfo
                    });
                }
        }
        return Content($"û��ʵ�֡�{type}����¼��");
    }
}
```

3.`Views`

```
<!--�ڴ����з�����Ȩ��ť-->
<a href="/oauth/github" class="btn btn-primary">Github ��¼</a>
<a href="/oauth/wechat" class="btn btn-primary">Wechat ɨ���¼</a>
<!-- //������¼��ʽ����������д -->
```