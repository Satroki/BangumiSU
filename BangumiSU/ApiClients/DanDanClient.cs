using BangumiSU.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Newtonsoft.Json;
using Regex = System.Text.RegularExpressions.Regex;

namespace BangumiSU.ApiClients
{
    class DanDanClient : ApiClient
    {
        private const string ApiUrl = @"http://acplay.net/api/v1/";
        public DanDanClient() : base("")
        {
            SetApiUrl(ApiUrl);
        }

        #region Comment
        //comment/{episodeId}?from={from}
        public async Task<List<Comment>> GetComments(int episodeId)
            => (await Get<TempObject>($"comment/{episodeId}")).Comments;

        public async Task<List<Comment>> GetAllComments(int episodeId)
        {
            var list = new List<Comment>();
            var dd = (await Get<TempObject>($"comment/{episodeId}")).Comments;
            list.AddRange(dd);

            var related = await GetRelateds(episodeId);
            var bList = new List<string>();
            using (var hc = CreateHC())
            {
                foreach (var r in related)
                {
                    IEnumerable<Comment> temp;
                    switch (r.Provider)
                    {
                        case "Tucao.cc":
                            temp = await GetTucao(hc, r);
                            break;
                        case "BiliBili.com":
                            temp = await GetBiliBili(hc, r, bList);
                            break;
                        default:
                            continue;
                    }
                    if (temp != null)
                        list.AddRange(temp);
                }
            }
            return list;
        }

        private async Task<IEnumerable<Comment>> GetTucao(HttpClient hc, Related r)
        {
            var match = Regex.Match(r.Url, @"h(\d{1,10})");
            if (match.Success)
            {
                var uri = $"http://www.tucao.tv/index.php?m=mukio&c=index&a=init&playerID=40-{match.Groups[1].Value}-1-0";
                var str = await hc.GetStringAsync(uri);
                var xml = XDocument.Parse(str);
                return xml.Descendants("d").Select(xe =>
                {
                    var p = xe.Attribute("p").Value.Split(',');
                    var m = xe.Value;
                    return new Comment
                    {
                        Time = double.Parse(p[0]),
                        Color = int.Parse(p[3]),
                        Message = m
                    };
                });
            }
            return null;
        }

        private async Task<IEnumerable<Comment>> GetBiliBili(HttpClient hc, Related r, List<string> bList)
        {
            var match = Regex.Match(r.Url, @"av(\d{1,10})");
            if (match.Success)
            {
                var av = match.Groups[1].Value;
                if (bList.Contains(av))
                    return null;

                bList.Add(av);
                var uri = $"https://biliproxy.chinacloudsites.cn/av/{av}/1?list=0";
                var res = await hc.GetAsync(uri);
                var str = "";
                if (res.StatusCode == HttpStatusCode.OK)
                    str = await res.Content.ReadAsStringAsync();
                else if (res.StatusCode == HttpStatusCode.Redirect)
                {
                    var location = res.Headers.Location;
                    res = await hc.GetAsync(location);
                    str = await res.Content.ReadAsStringAsync();
                }
                dynamic json = JsonConvert.DeserializeObject(str);
                string cid = json.cid;

                uri = $"http://comment.bilibili.cn/{cid}.xml";
                str = await hc.GetStringAsync(uri);
                var xml = XDocument.Parse(str);
                return xml.Descendants("d").Select(xe =>
                {
                    var p = xe.Attribute("p").Value.Split(',');
                    var m = xe.Value;
                    return new Comment
                    {
                        Time = double.Parse(p[0]),
                        Color = int.Parse(p[3]),
                        Message = m
                    };
                });
            }
            return null;
        }

        private HttpClient CreateHC()
        {
            var hc = new HttpClient(new HttpClientHandler()
            {
                AllowAutoRedirect = false,
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
            });
            hc.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            hc.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("gzip"));
            hc.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("deflate"));
            return hc;
        }
        #endregion

        #region Match
        //match?fileName={fileName}&hash={hash}&length={length}&duration={duration}&force={force}
        public async Task<List<Match>> GetMatches(string fileName, string hash, long length, int duration = 0, int force = 0)
            => (await Get<TempObject>($"match?fileName={fileName}&hash={hash}&length={length}&duration={duration}&force={force}")).Matches;
        #endregion

        #region Related
        //related/{episodeId}
        public async Task<List<Related>> GetRelateds(int episodeId)
            => (await Get<TempObject>($"related/{episodeId}")).Relateds;
        #endregion

        #region SearchAll
        //searchall/{anime}/{episode}
        public async Task<List<Anime>> Search(string name, double episode)
            => (await Get<TempObject>($"searchall/{name}/{episode}")).Animes;
        #endregion

        private class TempObject
        {
            public List<Comment> Comments { get; set; }

            public List<Match> Matches { get; set; }

            public List<Related> Relateds { get; set; }

            public List<Anime> Animes { get; set; }

            public bool HasMore { get; set; }
        }
    }
}
