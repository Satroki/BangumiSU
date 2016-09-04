using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;

namespace BangumiSU.Models
{
    public enum Mode { Normal = 1, Bottom = 4, Top = 5 }
    public class Comment : ModelBase
    {
        public double Time { get; set; }
        public Mode Mode { get; set; }
        public int Color { get; set; }
        public int Timestamp { get; set; }
        public int Pool { get; set; }
        public int UId { get; set; }
        public int CId { get; set; }
        public string Message { get; set; }
    }

    public class Match : ModelBase
    {
        public int EpisodeId { get; set; }
        public string AnimeTitle { get; set; }
        public string EpisodeTitle { get; set; }
        public int Type { get; set; }
        public double Shift { get; set; }
    }

    public class Related : ModelBase
    {
        public string Provider { get; set; }
        public string Url { get; set; }
        public double Shift { get; set; }
    }

    public class Anime : ModelBase
    {
        public string Title { get; set; }
        public int Type { get; set; }
        public List<Episode> Episodes { get; set; }
    }

    public class Episode : ModelBase
    {
        public int Id { get; set; }
        public string Title { get; set; }
    }

    public class SearchResult : ModelBase
    {
        public string Title { get; set; }
        public string Uri { get; set; }
        public int Count { get; set; }
        public string Provider { get; set; }
        public string InfoString => $"{Provider} -- {Count}";
    }
}
