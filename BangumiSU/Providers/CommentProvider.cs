using BangumiSU.Models;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
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
            }
        }

        protected static HttpClient hc { get; private set; }

        public string Name { get; protected set; }

        public abstract Task<List<Comment>> GetComments(string url);

        public abstract Task<List<SearchResult>> Search(string key);

        public IEnumerable<Comment> ParseXml(string xmlString)
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

        protected static bool IdExists(string id)
        {
            if (IdList.Contains(id))
                return true;
            IdList.Add(id);
            return false;
        }
    }
}
