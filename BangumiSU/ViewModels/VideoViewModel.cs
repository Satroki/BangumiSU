using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
    }
}
