using BangumiSU.Models;
using BangumiSU.SharedCode;
using System.Collections.Generic;
using System.Threading.Tasks;
using static BangumiSU.SharedCode.AppCache;

namespace BangumiSU.ViewModels
{
    public class TrackingViewModel : ViewModelBase
    {
        public TrackingViewModel(Tracking t, bool edit)
        {
            Tracking = t;
            EditMode = edit;
            GetBangumis();
        }

        #region 属性
        public List<Bangumi> Bangumis { get; set; }
        public Tracking Tracking { get; set; }
        public bool EditMode { get; set; }
        public string Title => EditMode ? "编辑" : "添加";
        #endregion

        #region 方法
        public async void GetBangumis()
        {
            if (BangumiCache.IsEmpty())
                BangumiCache = await BClient.GetUnfinished();
            Bangumis = BangumiCache;
        }

        public async Task Save()
        {
            if (!EditMode)
            {
                if (Tracking.BangumiId == 0)
                    return;
                Tracking = Tracking;
            }
            else
            {
                Tracking = await LoadingTask(TClient.Update(Tracking)) ?? Tracking;
            }
        }
        #endregion
    }
}
