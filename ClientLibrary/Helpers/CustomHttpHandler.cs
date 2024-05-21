
using BaseLibrary.Dtos;
using ClientLibrary.Services.Contracts;

namespace ClientLibrary.Helpers
{

    /// <summary>
    /// Class to update the token whe expire
    /// </summary>
    /// <param name="getHttpClient"></param>
    /// <param name="localStorageService"></param>
    public class CustomHttpHandler(GetHttpClient getHttpClient, LocalStorageService localStorageService, IUserAccountService accountService) : DelegatingHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellation)
        {
            bool login = request.RequestUri!.AbsoluteUri.Contains("login");
            bool registerUrl = request!.RequestUri!.AbsoluteUri.Contains("register");
            bool refreshTokenUrl = request.RequestUri!.AbsoluteUri.Contains("refresh-token");

            if(login && registerUrl && refreshTokenUrl) return await base.SendAsync(request, cancellation);
            var result = await base.SendAsync(request, cancellation);
            if (result.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                //Get token from localStorage
                var stringToken = await localStorageService.GetToken();
                if (stringToken == null) return result;

                //Check if the header contains token
                string token = string.Empty;
                try
                {
                    token = request.Headers.Authorization!.Parameter!;
                }
                catch { }

                var deserializedToken = Serializations.DeserializeJsonString<UserSession>(stringToken);
                if(deserializedToken is null) return result;

                if(string.IsNullOrEmpty(token))
                {
                    request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", deserializedToken.Token);
                    return await base.SendAsync(request, cancellation);
                }

                //Call for refresh token
                var newJwtToken = await GetRefreshToken(deserializedToken.RefreshToken!);
                if(string.IsNullOrEmpty(newJwtToken)) return result;

                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", newJwtToken);
                return await base.SendAsync(request, cancellation);
            }
            return result;
        }

        private async Task<string> GetRefreshToken(string refreshToken)
        {
            var result = await accountService.RefreshTokenAsyn(new RefreshToken() { Token = refreshToken });
            string serializedToken = Serializations.SerializeObj(new UserSession()
            {
                Token = result.Token,
                RefreshToken = result.RefreshToken
            });
            await localStorageService.SetToken(serializedToken);
            return result.RefreshToken;

        }
    }
}
