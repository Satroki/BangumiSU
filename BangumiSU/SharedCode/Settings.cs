using Newtonsoft.Json;
using System;
using Windows.Storage;

namespace BangumiSU.SharedCode
{
    public class Settings
    {
        public string UserGUID { get; set; }

        public string FolderFormat { get; set; } = @"\d{4}.\d{2}月番";

        public string RssPattern { get; set; }

        public DateTimeOffset LastUpdate { get; set; } = new DateTime(2000, 1, 1);

        public string LastSearch { get; set; }

        public VideoSettings VideoSettings { get; set; } = new VideoSettings();

        public bool UseInternalPlayer { get; set; } = true;

        [JsonIgnore]
        public string DmhySearch { get; set; } = "https://share.dmhy.org/topics/list?keyword=";

        [JsonIgnore]
        public string Extensions { get; set; } = ".MP4|.MKV";

        public static Settings GetRoamingSetting()
        {
            var roaming = ApplicationData.Current.RoamingSettings;
            if (roaming.Values.ContainsKey("AppSettings"))
                return JsonConvert.DeserializeObject<Settings>(roaming.Values["AppSettings"].ToString());
            else
                return new Settings();
        }

        public void Save()
        {
            var json = JsonConvert.SerializeObject(this);
            var roaming = ApplicationData.Current.RoamingSettings;
            if (roaming.Values.ContainsKey("AppSettings"))
                roaming.Values["AppSettings"] = json;
            else
                roaming.Values.Add("AppSettings", json);
        }
    }

    public class VideoSettings
    {
        public double FontSize { get; set; } = 24;
        public double Duration { get; set; } = 8;
        public string Filter { get; set; } = "";
        public bool ContinuousPlayback { get; set; } = true;
    }
}
