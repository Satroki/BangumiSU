﻿using BangumiSU.Models;
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
        private readonly bool withToken;

        protected virtual Uri BaseAddress { get; set; }

        public ApiClient(string baseAddress, bool withToken = true)
        {
            if (hc == null)
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
                hc.DefaultRequestHeaders.UserAgent.ParseAdd("bangumi_su/windows 1.0");
            }
            if (baseAddress.Last() != '/')
                baseAddress = baseAddress + "/";
            BaseAddress = new Uri(baseAddress);
            this.withToken = withToken;
        }

        private void PrepareHeader()
        {
            if (withToken)
            {
                var token = AppCache.AppSettings.UserToken?.AccessToken;
                if (!token.IsEmpty())
                    if (hc.DefaultRequestHeaders.Authorization?.Parameter != token)
                        hc.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
        }

        public async Task<T> Get<T>(string route = "")
        {
            PrepareHeader();
            var uri = new Uri(BaseAddress, route);
            var resp = await hc.GetAsync(uri);
            return await ReadResponse<T>(resp);
        }

        public async Task<T> Post<T>(object value, string route = "")
        {
            PrepareHeader();
            var uri = new Uri(BaseAddress, route);
            var resp = await hc.PostAsync(uri, GetJsonContent(value));
            return await ReadResponse<T>(resp);
        }

        public async Task<T> PostFile<T>(byte[] value, string route = "")
        {
            PrepareHeader();
            var uri = new Uri(BaseAddress, route);
            var bc = new ByteArrayContent(value);
            bc.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
            var resp = await hc.PostAsync(uri, bc);
            return await ReadResponse<T>(resp);
        }

        public async Task<T> Put<T>(T value, string route = "")
        {
            PrepareHeader();
            var uri = new Uri(BaseAddress, route);
            var resp = await hc.PutAsync(uri, GetJsonContent(value));
            return await ReadResponse<T>(resp);
        }

        public async Task<int> Delete(long id)
        {
            PrepareHeader();
            var uri = new Uri(BaseAddress, id.ToString());
            var resp = await hc.DeleteAsync(uri);
            return await ReadResponse<int>(resp);
        }

        public async Task<T> Patch<T>(long id, JsonPatchDocument<T> model) where T : class
        {
            return await Patch(id.ToString(), model);
        }

        public async Task<T> Patch<T>(string route, JsonPatchDocument<T> model) where T : class
        {
            PrepareHeader();
            var uri = new Uri(BaseAddress, route);
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
