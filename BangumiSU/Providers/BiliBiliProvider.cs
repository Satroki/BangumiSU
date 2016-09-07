using BangumiSU.Models;
using BangumiSU.SharedCode;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace BangumiSU.Providers
{
    public class BiliBiliProvider : CommentProvider
    {
        public BiliBiliProvider() : base()
        {
            Name = "BiliBili.com";
        }

        public override async Task<List<Comment>> GetComments(string url)
        {
            var match = Regex.Match(url, @"av(\d{1,10})");
            if (match.Success)
            {
                if (IdExists(match.Value))
                    return null;

                var uri = $"https://biliproxy.chinacloudsites.cn/av/{match.Groups[1].Value}/1?list=0";
                var str = await HttpClient.GetStringWithRedirect(uri);

                var m = JsonConvert.DeserializeObject<JsonModel>(str);
                uri = $"http://comment.bilibili.cn/{m.Cid}.xml";
                str = await HttpClient.GetStringAsync(uri);
                return ParseXml(str).ToList();
            }
            return null;
        }

        public override async Task<List<SearchResult>> Search(string key)
        {
            var str = await HttpClient.GetStringWithRedirect($"https://biliproxy.chinacloudsites.cn/search?keyword={key}");
            var list = new List<SearchResult>();
            var m = JsonConvert.DeserializeObject<JsonModel>(str);
            foreach (var item in m.Result)
            {
                if (item.Type == "video")
                {
                    var r = new SearchResult()
                    {
                        Title = item.Title,
                        Uri = item.Arcurl,
                        Provider = Name,
                        Count = item.Video_Review
                    };
                    list.Add(r);
                }
            }
            return list;
        }

        private class JsonModel
        {
            public string Cid { get; set; }
            public List<ResultModel> Result { get; set; }
        }

        private class ResultModel
        {
            public string Type { get; set; }
            public string Title { get; set; }
            public string Arcurl { get; set; }
            public int Video_Review { get; set; }
        }
    }
}
