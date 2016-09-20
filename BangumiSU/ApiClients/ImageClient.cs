using BangumiSU.Models;
using BangumiSU.SharedCode;
using System.Threading.Tasks;

namespace BangumiSU.ApiClients
{
    public class ImageClient : ApiClient
    {
        public ImageClient() : base(AppCache.ApiUrl + "Image") { }

        public async Task<Image> GetImage(string uri)
            => await Get<Image>("?uri=" + uri);
    }
}
