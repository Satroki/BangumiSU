using BangumiSU.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using static BangumiSU.SharedCode.AppCache;

namespace BangumiSU.ViewModels
{
    public class VideoViewModel : ViewModelBase
    {
        public double FontSize
        {
            get { return AppSettings.VideoSettings.FontSize; }
            set { AppSettings.VideoSettings.FontSize = value; }
        }


        public double Duration
        {
            get { return AppSettings.VideoSettings.Duration; }
            set { AppSettings.VideoSettings.Duration = value; }
        }

        public string Filter
        {
            get { return AppSettings.VideoSettings.Filter; }
            set { AppSettings.VideoSettings.Filter = value; }
        }

        public int Offset { get; set; }

        public Tracking Tracking { get; set; }

        public List<Match> Matches { get; set; } = new List<Match>();

        public List<Anime> Animes { get; set; } = new List<Anime>();

        public Episode SelectedEpisode { get; set; }

        public bool IsApplyEnabled => SelectedEpisode != null;

        public List<SearchResult> SearchResult { get; set; }

        public List<StorageFile> Files { get; set; } = new List<StorageFile>();

        public string CurrentFileName { get; set; }

        public bool? ShowNormal { get; set; } = true;

        public bool? ShowTop { get; set; } = true;

        public long FileSize { get; set; }

        public string FileHash { get; set; }

        public string FileName { get; set; }
    }
}
