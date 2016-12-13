using BangumiSU.Models;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace BangumiSU.Providers
{
    public abstract class CommentProvider
    {
        public static List<string> IdList { get; } = new List<string>();

        public CommentProvider()
        {
            if (hc == null)
            {
                hc = new HttpClient(new HttpClientHandler()
                {
                    AllowAutoRedirect = false,
                    AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
                });
                hc.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                hc.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("gzip"));
                hc.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("deflate"));
                hc.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/52.0.2743.116 Safari/537.36 Edge/15.14965");
            }
        }

        protected static HttpClient hc { get; private set; }

        public string Name { get; protected set; }

        public abstract Task<List<Comment>> GetComments(string url);

        public abstract Task<List<SearchResult>> Search(string key);

        public IEnumerable<Comment> ParseXml(string xmlString)
        {
            xmlString = Regex.Replace(xmlString, @"[\u0000-\u001F]", string.Empty);
            var xml = XDocument.Parse(xmlString);
            foreach (var xe in xml.Descendants("d"))
            {
                var p = xe.Attribute("p").Value.Split(',');
                var m = xe.Value;
                yield return new Comment
                {
                    Time = double.Parse(p[0]),
                    Mode = (Mode)int.Parse(p[1]),
                    Color = long.Parse(p[3]),
                    Message = m
                };
            }
        }

        protected static bool IdExists(string id)
        {
            if (IdList.Contains(id))
                return true;
            IdList.Add(id);
            return false;
        }
    }
}
