//#define local

using BangumiSU.Models;
using BangumiSU.ApiClients;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Storage;
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
        public const string ApiUrl = "https://bgm.ayaneru.moe/Api/";
#endif

        public static Settings AppSettings { get; set; }

        public static BangumiClient BClient { get; set; }

        public static TrackingClient TClient { get; set; }

        public static ImageClient IClient { get; private set; }

        public static List<Bangumi> BangumiCache { get; set; } = new List<Bangumi>();

        public static List<Bangumi> MusicCache { get; set; } = new List<Bangumi>();

        public static ObservableCollection<Tracking> CurrentTrackings { get; set; } = new ObservableCollection<Tracking>();

        public static StorageFolder VideoFolder { get; set; }

        public static StorageFolder FinishFolder { get; set; }

        public static ElementTheme Theme { get; set; } = ElementTheme.Dark;

        public static async Task Init(Settings settings)
        {
            AppSettings = settings;

            VideoFolder = await FolderHelper.GetFolder(nameof(VideoFolder));
            FinishFolder = await FolderHelper.GetFolder(nameof(FinishFolder));

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
