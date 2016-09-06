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

                dynamic json = JsonConvert.DeserializeObject(str);
                string cid = json.cid;

                uri = $"http://comment.bilibili.cn/{cid}.xml";
                str = await HttpClient.GetStringAsync(uri);
                return ParseXml(str).ToList();
            }
            return null;
        }

        public override async Task<List<SearchResult>> Search(string key)
        {
            var str = await HttpClient.GetStringWithRedirect($"https://biliproxy.chinacloudsites.cn/search?keyword={key}");
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
                        Provider = Name,
                        Count = (int)item.video_review.Value
                    };
                    list.Add(r);
                }
            }
            return list;
        }
    }
}
