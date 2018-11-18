using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;

namespace BangumiSU.Models
{
    public enum Mode
    {
        Normal = 1,
        Bottom = 4,
        Top = 5
    }

    public enum AnimeType
    {
        TV动画 = 1,
        TV动画特别放送 = 2,
        OVA = 3,
        剧场版 = 4,
        音乐视频 = 5,
        网络放送 = 6,
        其他分类 = 7,
        三次元电影 = 10,
        三次元电视剧或国产动画 = 20,
        未知 = 99
    }

    public class Comment : ModelBase
    {
        public int Cid { get; set; }
        public string P { get; set; }
        public string M { get; set; }

        public double Time { get; set; }
        public Mode Mode { get; set; }
        public long Color { get; set; }
        public string Message { get; set; }

        public void Parse()
        {
            var vs = P.Split(',');
            Time = double.Parse(vs[0]);
            Mode = (Mode)int.Parse(vs[1]);
            Color = int.Parse(vs[2]);
            Message = M;
        }
    }

    public class Match : ModelBase
    {
        public int EpisodeId { get; set; }
        public int AnimeId { get; set; }
        public string AnimeTitle { get; set; }
        public string EpisodeTitle { get; set; }
        public string Type { get; set; }
        public string TypeDescription { get; set; }
        public double Shift { get; set; }
    }

    public class DanDanResult
    {
        public int Count { get; set; }
        public bool IsMatched { get; set; }
        public List<Comment> Comments { get; set; }

        public List<Match> Matches { get; set; }

        public List<Related> Relateds { get; set; }

        public List<Anime> Animes { get; set; }

        public bool HasMore { get; set; }
        public int ErrorCode { get; set; }
        public bool Success { get; set; }
        public string ErrorMessage { get; set; }
    }

    public class Related : ModelBase
    {
        public string Provider { get; set; }
        public string Url { get; set; }
        public double Shift { get; set; }
    }

    public class Anime : ModelBase
    {
        public int AnimeId { get; set; }
        public string AnimeTitle { get; set; }
        public string Type { get; set; }
        public string TypeDescription { get; set; }
        public List<Episode> Episodes { get; set; }
    }

    public class Episode : ModelBase
    {
        public int EpisodeId { get; set; }
        public string EpisodeTitle { get; set; }
    }

    public class SearchResult : ModelBase
    {
        public string Title { get; set; }
        public string Uri { get; set; }
        public int Count { get; set; }
        public string Provider { get; set; }
        public string InfoString => $"{Provider} -- {Count}";
    }

    public class MatchRequest
    {
        public string FileName { get; set; }
        public string FileHash { get; set; }
        public long FileSize { get; set; }
        public int VideoDuration { get; set; }
        public string MatchMode { get; set; }
    }
}
