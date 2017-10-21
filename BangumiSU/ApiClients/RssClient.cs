using BangumiSU.Models;
using BangumiSU.SharedCode;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace BangumiSU.ApiClients
{
    public class RssClient : ApiClient
    {
        public RssClient() : base(AppCache.ApiUrl + "Rss")
        { }

        public async Task<List<RssItem>> GetRss(DateTimeOffset? date = null)
        {
            if (AppCache.AppSettings.UseLocalRss)
            {
                return await GetRssItems(date);
            }
            else
            {
                return await Get<List<RssItem>>(date?.ToString("u"));
            }
        }

        private async Task<List<RssItem>> GetRssItems(DateTimeOffset? date)
        {
            var xml = await hc.GetStringAsync(AppCache.AppSettings.DmhyRss);
            var doc = XDocument.Parse(xml);
            var items = doc.Descendants("item")
                .Select(
                   nd =>
                   {
                       var ti = nd.Element("title").Value.Trim();
                       var pd = nd.Element("pubDate").Value;
                       var link = nd.Element("link").Value;
                       var url = nd.Element("enclosure").Attribute("url").Value.Substring(0, 52);
                       return new RssItem(ti, DateTimeOffset.Parse(pd), url, link);
                   });
            if (date.HasValue)
                return items.Where(r => r.PubDate > date).ToList();
            return items.ToList();

        }
    }
}
