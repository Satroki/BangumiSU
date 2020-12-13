using BangumiSU.SharedCode;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using BangumiSU.Models;
using BangumiSU.Pages.Controls;

namespace BangumiSU.ApiClients
{
    public class AccountClient
    {
        HttpClient hc;
        public AccountClient()
        {
            hc = new HttpClient(new HttpClientHandler()
            {
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
            })
            {
                Timeout = TimeSpan.FromSeconds(20)
            };
            hc.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            hc.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("gzip"));
            hc.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("deflate"));
        }

        public async Task<bool> Check()
        {
            var t = AppCache.AppSettings.UserToken;
            var msg = "";
            if (t != null)
            {
                if (t.ExpireDate - DateTimeOffset.Now < TimeSpan.FromDays(2))
                {
                    msg = await RefreshTokenAsync(t.RefreshToken);
                }
                hc.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(t.TokenType, t.AccessToken);
            }
            var resp = await hc.GetAsync($"{AppCache.ApiUrl}account/check");
            if (resp.IsSuccessStatusCode)
                return true;
            else
            {
                var d = new LoginDialog(this);
                var r = await d.ShowAsync();
                return r == Windows.UI.Xaml.Controls.ContentDialogResult.Primary;
            }
        }

        public async Task<string> GetToken(FormUrlEncodedContent form)
        {
            var resp = await hc.PostAsync("https://identity.ayaneru.moe:4433/connect/token", form);
            if (resp.IsSuccessStatusCode)
            {
                var json = await resp.Content.ReadAsStringAsync();
                var token = JsonConvert.DeserializeObject<JWToken>(json);
                token.ExpireDate = DateTimeOffset.Now.AddSeconds(token.ExpiresIn);
                AppCache.AppSettings.UserToken = token;
                AppCache.AppSettings.Save();
                return null;
            }
            else
            {
                var json = await resp.Content.ReadAsStringAsync();
                var err = JsonConvert.DeserializeObject<AccountError>(json);
                return $"{resp.StatusCode}: {err.ErrorDescription ?? err.Error}";
            }
        }

        public async Task<string> RefreshTokenAsync(string rt)
        {
            var dict = new Dictionary<string, string>
            {
                ["client_id"] = "bangumi",
                ["client_secret"] = "bangumi",
                ["grant_type"] = "refresh_token",
                ["refresh_token"] = rt,
            };
            var form = new FormUrlEncodedContent(dict);
            return await GetToken(form);
        }

        public async Task<string> LoginAsync(string user, string pwd)
        {
            var dict = new Dictionary<string, string>
            {
                ["client_id"] = "bangumi",
                ["client_secret"] = "bangumi",
                ["grant_type"] = "password",
                ["username"] = user,
                ["password"] = pwd,
            };
            var form = new FormUrlEncodedContent(dict);
            return await GetToken(form);
        }
    }

    public class AccountError
    {
        public string Error { get; set; }
        [JsonProperty("error_description")]
        public string ErrorDescription { get; set; }
    }
}
