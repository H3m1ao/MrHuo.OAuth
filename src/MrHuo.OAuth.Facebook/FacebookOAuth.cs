using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MrHuo.OAuth.Facebook
{
    /// <summary>
    /// Facebook OAuth 相关文档参考：
    /// <para>https://developers.facebook.com/docs/apis-and-sdks</para>
    /// <para></para>
    /// </summary>
    public class FacebookOAuth : OAuthLoginBase<FacebookUserInfoModel>
    {
        public FacebookOAuth(OAuthConfig oauthConfig) : base(oauthConfig) { }
        protected override string AuthorizeUrl => "https://www.facebook.com/v9.0/dialog/oauth";
        protected override string AccessTokenUrl => "https://graph.facebook.com/v9.0/oauth/access_token";
        protected override string UserInfoUrl => "https://graph.facebook.com/me";

        public override async Task<FacebookUserInfoModel> GetUserInfoAsync(DefaultAccessTokenModel accessTokenModel)
        {
            var userInfoModel = await HttpRequestApi.GetAsync<FacebookUserInfoModel>(
                UserInfoUrl,
                new Dictionary<string, string>()
                {
                    ["access_token"] = accessTokenModel.AccessToken,
                    ["fields"] = string.IsNullOrEmpty(accessTokenModel.Scope) ? "id,name,email,picture" : accessTokenModel.Scope
                }
            );
            if (userInfoModel.HasError())
            {
                throw new Exception(userInfoModel.ErrorMessage);
            }
            return userInfoModel;
        }
    }
}
