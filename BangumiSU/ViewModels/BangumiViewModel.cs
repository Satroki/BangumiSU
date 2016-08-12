using BangumiSU.Models;
using System.Threading.Tasks;
using static BangumiSU.SharedCode.AppCache;

namespace BangumiSU.ViewModels
{
    public class BangumiViewModel : ViewModelBase
    {
        public BangumiViewModel(Bangumi bgm)
        {
            Bangumi = bgm;
        }

        #region 属性
        public Bangumi Bangumi { get; set; }
        #endregion

        #region 方法
        public async Task Save()
        {
            Bangumi = await LoadingTask(BClient.Update(Bangumi)) ?? Bangumi;
        }

        public async Task Update()
        {
            Bangumi = await LoadingTask(BClient.Update(Bangumi.Id)) ?? Bangumi;
        }
        #endregion
    }
}
