using BangumiSU.ApiClients;
using BangumiSU.Models;
using BangumiSU.SharedCode;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace BangumiSU.ViewModels
{
    public class BangumiInfoViewModel : ViewModelBase
    {
        private BangumiInfoClient biClient = new BangumiInfoClient();

        public ObservableCollection<BangumiInfo> BangumiInfoListBak { get; set; }

        public ObservableCollection<BangumiInfo> BangumiInfoList { get; set; }

        public DateTimeOffset Date { get; set; } = DateTimeOffset.Now;

        public string FilterKey { get; set; }

        public async void GetItems()
        {
            await LoadingTask(getItems());
        }

        private async Task getItems()
        {
            BangumiInfoListBak = BangumiInfoList = (await biClient.GetBangumis(Date.Year, Date.Month)).ToObservableCollection();
        }

        public async void Add(BangumiInfo bi)
        {
            var code = bi.Url.Substring(bi.Url.LastIndexOf('/') + 1);
            try
            {
                bi.State = BangumiInfoState.Loading;
                var bgm = await SharedCode.AppCache.BClient.CreateByCode(code);
                bi.State = BangumiInfoState.Added;
            }
            catch (Exception ex)
            {
                bi.State = ex.Message.Contains("存在") ? BangumiInfoState.Extist : BangumiInfoState.Error;
            }
        }

        public void Filter()
        {
            if (FilterKey.IsEmpty())
                BangumiInfoList = BangumiInfoListBak;
            else
                BangumiInfoList = BangumiInfoListBak
                    .Where(bi => bi.LocalName.ContainsIgnoreCase(FilterKey) || bi.Name.ContainsIgnoreCase(FilterKey))
                    .ToObservableCollection();
        }
    }
}
