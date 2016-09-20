using BangumiSU.Models;
using BangumiSU.SharedCode;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BangumiSU.ApiClients
{
    public class BangumiInfoClient : ApiClient
    {
        public BangumiInfoClient() : base(AppCache.ApiUrl + "BangumiInfo") { }

        public async Task<List<BangumiInfo>> GetBangumis(int year, int month)
            => await Get<List<BangumiInfo>>($"{year}/{month}");
    }
}
