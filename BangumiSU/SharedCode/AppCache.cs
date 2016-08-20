//#define local

using BangumiSU.Models;
using BangumiSU.ApiClients;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.AccessCache;
using System.Collections.ObjectModel;
using Newtonsoft.Json;
using Windows.UI.Xaml;

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

        public static List<Bangumi> BangumiCache { get; set; } = new List<Bangumi>();

        public static List<Bangumi> MusicCache { get; set; } = new List<Bangumi>();

        public static ObservableCollection<Tracking> CurrentTrackings { get; set; } = new ObservableCollection<Tracking>();

        public static StorageFolder VideoFolder { get; set; }

        public static ElementTheme Theme { get; set; }

        public static async Task Init(Settings settings)
        {
            AppSettings = settings;

            var token = nameof(VideoFolder);
            var access = StorageApplicationPermissions.FutureAccessList.ContainsItem(token);
            if (access)
                VideoFolder = await StorageApplicationPermissions.FutureAccessList.GetFolderAsync(token);

            InitJsonConvert();

            BClient = new BangumiClient();
            TClient = new TrackingClient();
            IClient = new ImageClient();
        }

        public static async Task Reload()
        {
            await Init(AppSettings);
        }

        private static void InitJsonConvert()
        {
            var setting = new JsonSerializerSettings()
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                NullValueHandling = NullValueHandling.Ignore
            };
            JsonConvert.DefaultSettings = () => setting;
        }
    }
}
