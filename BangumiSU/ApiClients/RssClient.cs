using BangumiSU.Models;
using BangumiSU.SharedCode;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BangumiSU.ApiClients
{
    public class RssClient : ApiClient
    {
        public RssClient() : base(AppCache.ApiUrl + "Rss")
        { }

        public async Task<List<RssItem>> GetRss(DateTimeOffset? date = null)
        {
            return await Get<List<RssItem>>(date?.ToString("u"));
        }
    }
}
