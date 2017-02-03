using BangumiSU.Models;
using BangumiSU.SharedCode;
using Microsoft.AspNetCore.JsonPatch;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace BangumiSU.ApiClients
{
    public abstract class ApiClient
    {
        protected static HttpClient hc;
        protected virtual Uri BaseAddress { get; set; }

        public ApiClient(string baseAddress)
        {
            if (hc == null)
            {
                var user = AppCache.AppSettings.UserGUID;
                hc = new HttpClient(new HttpClientHandler()
                {
                    AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
                });
                hc.Timeout = TimeSpan.FromSeconds(20);
                hc.DefaultRequestHeaders.Add("user", user);
                hc.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                hc.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("gzip"));
                hc.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("deflate"));
            }
            if (baseAddress.Last() != '/')
                baseAddress = baseAddress + "/";
            BaseAddress = new Uri(baseAddress);
        }

        public async Task<T> Get<T>(string route = "")
        {
            var uri = new Uri(BaseAddress, route);
            var resp = await hc.GetAsync(uri);
            return await ReadResponse<T>(resp);
        }

        public async Task<T> Post<T>(T value, string route = "")
        {
            var uri = new Uri(BaseAddress, route);
            var resp = await hc.PostAsync(uri, GetJsonContent(value));
            return await ReadResponse<T>(resp);
        }

        public async Task<T> Put<T>(T value, string route = "")
        {
            var uri = new Uri(BaseAddress, route);
            var resp = await hc.PutAsync(uri, GetJsonContent(value));
            return await ReadResponse<T>(resp);
        }

        public async Task<int> Delete(long id)
        {
            var uri = new Uri(BaseAddress, id.ToString());
            var resp = await hc.DeleteAsync(uri);
            return await ReadResponse<int>(resp);
        }

        public async Task<T> Patch<T>(long id, JsonPatchDocument<T> model) where T : class
        {
            var uri = new Uri(BaseAddress, id.ToString());
            var method = new HttpMethod("PATCH");
            var request = new HttpRequestMessage(method, uri);
            request.Content = GetJsonContent(model);

            var resp = await hc.SendAsync(request);
            return await ReadResponse<T>(resp);
        }

        protected async Task<T> ReadResponse<T>(HttpResponseMessage resp)
        {
            if (resp.IsSuccessStatusCode)
            {
                var item = await resp.Content.ReadAsAsync<T>();
                return (T)AfterDeserialize(item);
            }
            else
            {
                var msg = "API异常";
                if (resp.StatusCode == HttpStatusCode.InternalServerError)
                    msg = (await resp.Content.ReadAsAsync<Error>()).Message;
                else
                    msg += "：" + (int)resp.StatusCode;

                throw new Exception(msg);
            }
        }

        protected async Task<string> ReadResponse(HttpResponseMessage resp)
        {
            if (resp.IsSuccessStatusCode)
            {
                var item = await resp.Content.ReadAsStringAsync();
                return item;
            }
            else
            {
                var msg = "API异常";
                if (resp.StatusCode == HttpStatusCode.InternalServerError)
                    msg = (await resp.Content.ReadAsAsync<Error>()).Message;
                else
                    msg += "：" + (int)resp.StatusCode;

                throw new Exception(msg);
            }
        }

        public StringContent GetJsonContent(object o)
        {
            var item = BeforeSerialize(o);
            return new StringContent(JsonConvert.SerializeObject(item), Encoding.UTF8, "application/json");
        }

        protected virtual object BeforeSerialize(object o)
        {
            return o;
        }

        protected virtual object AfterDeserialize(object o)
        {
            return o;
        }
    }
}
