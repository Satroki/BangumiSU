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
                            temp = await GetTucao(hc, r);
                            break;
                        case "BiliBili.com":
                            temp = await GetBiliBili(hc, r);
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
                if (IdExists(match.Value))
                    return null;

                var uri = $"http://www.tucao.tv/index.php?m=mukio&c=index&a=init&playerID=40-{match.Groups[1].Value}-1-0";
                var str = await hc.GetStringAsync(uri);
                return ParseXml(str);
            }
            return null;
        }

        private async Task<IEnumerable<Comment>> GetBiliBili(HttpClient hc, Related r)
        {
            var match = Regex.Match(r.Url, @"av(\d{1,10})");
            if (match.Success)
            {
                if (IdExists(match.Value))
                    return null;

                var uri = $"https://biliproxy.chinacloudsites.cn/av/{match.Groups[1].Value}/1?list=0";
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
                return ParseXml(str);
            }
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
