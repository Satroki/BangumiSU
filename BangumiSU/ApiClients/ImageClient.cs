using BangumiSU.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BangumiSU.ApiClients
{
    public class ImageClient : ApiClient
    {
        public ImageClient() : base("Image/") { }

        public async Task<Image> GetImage(string uri)
            => await Get<Image>("?uri=" + uri);
    }
}
