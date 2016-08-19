using BangumiSU.SharedCode;
using Newtonsoft.Json;
using PropertyChanged;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Windows.UI.Xaml.Media;

namespace BangumiSU.Models
{
    public partial class Bangumi : ModelBase
    {
        public int Id { get; set; }

        private DateTimeOffset _OnAir;
        public DateTimeOffset OnAir
        {
            get { return _OnAir; }
            set { SetProperty(ref _OnAir, value); Schedule = value.ToString("yyyyMM"); }
        }

        private string _Schedule;
        public string Schedule
        {
            get { return _Schedule; }
            set { SetProperty(ref _Schedule, value); }
        }

        private string _Name;
        public string Name
        {
            get { return _Name; }
            set { SetProperty(ref _Name, value); }
        }

        private string _LocalName;
        public string LocalName
        {
            get { return _LocalName; }
            set { SetProperty(ref _LocalName, value); }
        }

        private string _Author;
        public string Author
        {
            get { return _Author; }
            set { SetProperty(ref _Author, value); }
        }

        private string _AnimeCompany;
        public string AnimeCompany
        {
            get { return _AnimeCompany; }
            set { SetProperty(ref _AnimeCompany, value); }
        }

        private string _HomePage;
        public string HomePage
        {
            get { return _HomePage; }
            set { SetProperty(ref _HomePage, value); }
        }

        private string _BangumiCode;
        public string BangumiCode
        {
            get { return _BangumiCode; }
            set { SetProperty(ref _BangumiCode, value); }
        }

        private string _ImageUri;
        public string ImageUri
        {
            get { return _ImageUri; }
            set { SetProperty(ref _ImageUri, value); }
        }

        private int _Episodes;
        public int Episodes
        {
            get { return _Episodes; }
            set { SetProperty(ref _Episodes, value); }
        }

        private string _Opening;
        public string Opening
        {
            get { return _Opening; }
            set { SetProperty(ref _Opening, value); }
        }

        private string _Ending;
        public string Ending
        {
            get { return _Ending; }
            set { SetProperty(ref _Ending, value); }
        }

        private string _MusicInfo;
        public string MusicInfo
        {
            get { return _MusicInfo; }
            set { SetProperty(ref _MusicInfo, value); }
        }

        private string _OnlineLink;
        public string OnlineLink
        {
            get { return _OnlineLink; }
            set { SetProperty(ref _OnlineLink, value); }
        }

        private bool _Finish = false;
        public bool Finish
        {
            get { return _Finish; }
            set { SetProperty(ref _Finish, value); }
        }

        private ObservableCollection<Tracking> _Trackings;
        public virtual ObservableCollection<Tracking> Trackings
        {
            get { return _Trackings; }
            set { SetProperty(ref _Trackings, value); }
        }

        private ImageSource _Cover;
        [JsonIgnore]
        public ImageSource Cover
        {
            get
            {
                if (_Cover == null)
                    ImageHelper.GetImage(this);
                return _Cover;
            }
            set { SetProperty(ref _Cover, value); }
        }

        [JsonIgnore]
        [DependsOn(nameof(OnAir))]
        public string OnAirString => OnAir.LocalDateTime.ToString("yyyy/MM/dd HH:mm");

        [JsonIgnore]
        [DependsOn(nameof(OnAir))]
        public string DayString => OnAir.LocalDateTime.ToString("dddd");
    }
}
