using BangumiSU.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BangumiSU.ApiClients
{
    public class RssClient : ApiClient
    {
        public RssClient() : base("Rss/")
        { }

        public async Task<List<RssItem>> GetRss(DateTimeOffset? date = null)
        {
            return await Get<List<RssItem>>(date?.ToString("u"));
        }
    }
}
