#define local

using BangumiSU.Models;
using BangumiSU.ApiClients;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.AccessCache;

namespace BangumiSU.SharedCode
{
    public static class AppCache
    {

#if local
        public const string ApiUrl = "http://localhost:5123/Api/";
#else
        public const string ApiUrl = "http://ayaneru.moe:5123/Api/";
#endif

        public static Settings AppSettings { get; set; }

        public static BangumiClient BClient { get; set; }

        public static TrackingClient TClient { get; set; }

        public static ImageClient IClient { get; private set; }

        public static List<Bangumi> BangumiCache { get; set; }

        public static StorageFolder VideoFolder { get; set; }

        public static async Task Init(Settings settings)
        {
            AppSettings = settings;

            var token = nameof(VideoFolder);
            var access = StorageApplicationPermissions.FutureAccessList.ContainsItem(token);
            if (access)
                VideoFolder = await StorageApplicationPermissions.FutureAccessList.GetFolderAsync(token);

            BClient = new BangumiClient();
            TClient = new TrackingClient();
            IClient = new ImageClient();
        }
    }
}
