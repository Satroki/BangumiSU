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
        public const string ApiUrl = "http://127.0.0.1:5124/Api/";
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
        public static StorageFolder MusicFolder { get; set; }

        public static ElementTheme Theme { get; set; } = ElementTheme.Dark;

        public static void Init(Settings settings)
        {
            AppSettings = settings;
            InitJsonConvert();

            BClient = new BangumiClient();
            TClient = new TrackingClient();
            IClient = new ImageClient();
        }

        public static async Task InitFolderAsync()
        {
            VideoFolder = await FolderHelper.GetFolder(nameof(VideoFolder));
            FinishFolder = await FolderHelper.GetFolder(nameof(FinishFolder));
            MusicFolder = await FolderHelper.GetFolder(nameof(MusicFolder));
        }

        public static async Task Reload()
        {
            Init(AppSettings);
            await InitFolderAsync();
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
