using BangumiSU.Models;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace BangumiSU.Providers
{
    public class TucaoProvider : CommentProvider
    {
        public TucaoProvider() : base()
        {
            Name = "Tucao.cc";
        }

        public override async Task<List<Comment>> GetComments(string url)
        {
            var match = Regex.Match(url, @"h(\d{1,10})");
            if (match.Success)
            {
                if (IdExists(match.Value))
                    return null;

                var hid = match.Groups[1].Value;
                var uri = $"http://www.tucao.tv/index.php?m=mukio&c=index&a=init&playerID={hid.Substring(0, 2)}-{hid}-1-0";
                var str = await hc.GetStringAsync(uri);
                return ParseXml(str).OrderBy(c => c.Time).ToList();
            }
            return null;
        }

        public override async Task<List<SearchResult>> Search(string key)
        {
            var uri = $"http://www.tucao.tv/api_v2/search.php?type=xml&apikey=25tids8f1ew1821ed&page=1&pagesize=10&order=views&q={key}";
            var str = await hc.GetStringAsync(uri);
            var xml = XDocument.Parse(str);

            var list = new List<SearchResult>();
            foreach (var xe in xml.Root.Element("result").Elements("data"))
            {
                var r = new SearchResult()
                {
                    Title = xe.Element("title").Value,
                    Uri = $"http://www.tucao.tv/play/h{xe.Element("hid").Value}/",
                    Provider = Name,
                    Count = int.Parse(xe.Element("mukio").Value)
                };
                list.Add(r);
            }
            return list;
        }
    }
}
