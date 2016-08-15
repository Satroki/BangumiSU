using System;

namespace BangumiSU.Models
{
    public partial class RssItem : ModelBase
    {
        public string Title { get; set; }
        public DateTimeOffset PubDate { get; set; }
        public string Magnet { get; set; }
        public string Link { get; set; }

        public RssItem()
        {

        }

        public RssItem(string t, DateTimeOffset d, string m, string l)
        {
            Title = t;
            PubDate = d;
            Magnet = m;
            Link = l;
        }
    }
}
