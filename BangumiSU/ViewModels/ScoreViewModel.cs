using BangumiSU.Models;
using System.Threading.Tasks;
using static BangumiSU.SharedCode.AppCache;

namespace BangumiSU.ViewModels
{
    public class ScoreViewModel : ViewModelBase
    {
        public ScoreViewModel(Bangumi bgm)
        {
            Bangumi = bgm;
            Score = new BangumiScore(bgm.ScoreDict);
        }

        #region 属性
        public Bangumi Bangumi { get; set; }
        public BangumiScore Score { get; set; }
        #endregion

        #region 方法
        public async Task Update()
        {
            var str = Score.ToString();
            var b = await LoadingTask(BClient.UpdateScores(Bangumi.Id, str));
            Bangumi.Scores = str;
        }
        #endregion
    }
}
