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
using BangumiSU.SharedCode;
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

        private List<string> IdList = new List<string>();

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
            IdList.Clear();
            using (var hc = CreateHC())
            {
                foreach (var r in related)
                {
                    IEnumerable<Comment> temp;
                    switch (r.Provider)
                    {
                        case "Tucao.cc":
                            temp = await GetTucao(hc, r.Url);
                            break;
                        case "BiliBili.com":
                            temp = await GetBiliBili(hc, r.Url);
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

        public async Task<IEnumerable<Comment>> GetTucao(HttpClient hc, string url)
        {
            var match = Regex.Match(url, @"h(\d{1,10})");
            if (match.Success)
            {
                if (IdExists(match.Value))
                    return null;
                hc = hc ?? CreateHC();

                var uri = $"http://www.tucao.tv/index.php?m=mukio&c=index&a=init&playerID=40-{match.Groups[1].Value}-1-0";
                var str = await hc.GetStringAsync(uri);
                return ParseXml(str);
            }
            return null;
        }

        public async Task<IEnumerable<Comment>> GetBiliBili(HttpClient hc, string url)
        {
            var match = Regex.Match(url, @"av(\d{1,10})");
            if (match.Success)
            {
                if (IdExists(match.Value))
                    return null;
                hc = hc ?? CreateHC();

                var uri = $"https://biliproxy.chinacloudsites.cn/av/{match.Groups[1].Value}/1?list=0";
                var str = await hc.GetStringWithRedirect(uri);

                dynamic json = JsonConvert.DeserializeObject(str);
                string cid = json.cid;

                uri = $"http://comment.bilibili.cn/{cid}.xml";
                str = await hc.GetStringAsync(uri);
                return ParseXml(str);
            }
            return null;
        }

        public async Task<List<SearchResult>> SearchComments(string key)
        {
            var hc = CreateHC();
            var list = new List<SearchResult>();

            var temp = await SearchBiliBili(key, hc);
            if (temp != null)
                list.AddRange(temp);

            temp = await SearchTucao(key, hc);
            if (temp != null)
                list.AddRange(temp);

            return list;
        }

        private async Task<List<SearchResult>> SearchBiliBili(string key, HttpClient hc)
        {
            var str = await hc.GetStringWithRedirect($"https://biliproxy.chinacloudsites.cn/search?keyword={key}");
            var list = new List<SearchResult>();
            dynamic json = JsonConvert.DeserializeObject(str);
            foreach (dynamic item in json.result)
            {
                if (item.type.Value == "video")
                {
                    var r = new SearchResult()
                    {
                        Title = item.title.Value,
                        Uri = item.arcurl.Value,
                        Provider = "BiliBili.com",
                        Count = (int)item.video_review.Value
                    };
                    list.Add(r);
                }
            }
            return list;
        }

        private async Task<List<SearchResult>> SearchTucao(string key, HttpClient hc)
        {
            //http://api.bilibili.cn/search?type=json&appkey=c1b107428d337928&keyword=%E9%AD%94%E8%A3%85%E5%AD%A6%E5%9B%ADH%C3%97H%2001&sign=efbb08029f243b4f1a2a30025ef191aa
            return null;
        }

        private bool IdExists(string id)
        {
            if (IdList.Contains(id))
                return true;
            IdList.Add(id);
            return false;
        }

        private IEnumerable<Comment> ParseXml(string xmlString)
        {
            var xml = XDocument.Parse(xmlString);
            foreach (var xe in xml.Descendants("d"))
            {
                var p = xe.Attribute("p").Value.Split(',');
                var m = xe.Value;
                yield return new Comment
                {
                    Time = double.Parse(p[0]),
                    Mode = (Mode)int.Parse(p[1]),
                    Color = int.Parse(p[3]),
                    Message = m
                };
            }
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
