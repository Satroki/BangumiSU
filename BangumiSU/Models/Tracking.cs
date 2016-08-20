using Newtonsoft.Json;
using PropertyChanged;
using System;
using Windows.UI;
using Windows.UI.Xaml.Media;

namespace BangumiSU.Models
{
    public partial class Tracking : ModelBase
    {
        public int Id { get; set; }

        private string _SubGroup;
        public string SubGroup
        {
            get { return _SubGroup; }
            set { SetProperty(ref _SubGroup, value); }
        }

        private string _FileIdName;
        public string FileIdName
        {
            get { return _FileIdName; }
            set { SetProperty(ref _FileIdName, value); }
        }

        private string _KeyWords;
        public string KeyWords
        {
            get { return _KeyWords; }
            set { SetProperty(ref _KeyWords, value); }
        }

        private string _Uri;
        public string Uri
        {
            get { return _Uri; }
            set { SetProperty(ref _Uri, value); }
        }

        private string _Folder;
        public string Folder
        {
            get { return _Folder; }
            set { SetProperty(ref _Folder, value); }
        }

        private int _Progress;
        public int Progress
        {
            get { return _Progress; }
            set { SetProperty(ref _Progress, value); }
        }

        private DateTimeOffset _LastUpdate = new DateTimeOffset(new DateTime(2000, 1, 1));
        public DateTimeOffset LastUpdate
        {
            get { return _LastUpdate; }
            set { SetProperty(ref _LastUpdate, value); }
        }

        private double _Count;
        public double Count
        {
            get { return _Count; }
            set { SetProperty(ref _Count, value); }
        }

        private bool _Finish;
        public bool Finish
        {
            get { return _Finish; }
            set { SetProperty(ref _Finish, value); }
        }

        private bool _Online;
        public bool Online
        {
            get { return _Online; }
            set { SetProperty(ref _Online, value); }
        }

        private int _BangumiId;
        public int BangumiId
        {
            get { return _BangumiId; }
            set { SetProperty(ref _BangumiId, value); }
        }

        private Bangumi _Bangumi;
        public virtual Bangumi Bangumi
        {
            get { return _Bangumi; }
            set { SetProperty(ref _Bangumi, value); }
        }

        [JsonIgnore]
        [DependsOn(nameof(Progress), nameof(Count))]
        public string ProgressString => $"{Progress:D2} / {Count:00.#}";

        [JsonIgnore]
        [DependsOn(nameof(Progress), nameof(Count),nameof(LastUpdate))]
        public Brush StateBrush
        {
            get
            {
                Color clr;
                if (Count > Progress)
                    clr = Colors.SeaGreen;
                else if ((DateTimeOffset.Now - LastUpdate).TotalDays >= 7)
                    clr = Colors.IndianRed;
                else
                    clr = Colors.Transparent;
                return new SolidColorBrush(clr);
            }
        }

        [JsonIgnore]
        [DependsOn(nameof(LastUpdate))]
        public string LastUpdateString => LastUpdate.LocalDateTime.ToString("MM/dd HH:mm");

        public void GetSubGroup(string url)
        {
            if (Online && !string.IsNullOrEmpty(url))
            {
                if (url.Contains("bilibili"))
                    SubGroup = "哔哩哔哩";
                else if (url.Contains("tudou") || url.Contains("youku"))
                    SubGroup = "优土豆";
                else if (url.Contains("iqiyi"))
                    SubGroup = "爱奇艺";
                else if (url.Contains("letv"))
                    SubGroup = "乐视";
            }
        }
    }
}
