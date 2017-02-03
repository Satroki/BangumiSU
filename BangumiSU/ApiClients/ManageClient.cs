using BangumiSU.SharedCode;
using System.Net.Http;
using System.Threading.Tasks;

namespace BangumiSU.ApiClients
{
    public class ManageClient : ApiClient
    {
        public ManageClient() : base(AppCache.ApiUrl + "Manage") { }

        public async Task<string> Upload(string json)
        {
            var c = new StringContent(json);
            var resp = await hc.PostAsync(BaseAddress, c);
            return await ReadResponse(resp);
        }

        public async Task<string> Download()
        {
            var resp = await hc.GetAsync(BaseAddress);
            return await ReadResponse(resp);
        }
    }
}
