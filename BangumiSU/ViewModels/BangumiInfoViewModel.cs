using BangumiSU.ApiClients;
using BangumiSU.Models;
using BangumiSU.SharedCode;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage.Pickers;
using Windows.Storage;
using Windows.UI.Popups;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Collections.Generic;

namespace BangumiSU.ViewModels
{
    public class BangumiInfoViewModel : ViewModelBase
    {
        private BangumiInfoClient biClient = new BangumiInfoClient();

        public ObservableCollection<BangumiInfo> BangumiInfoListBak { get; set; } = new ObservableCollection<BangumiInfo>();

        public ObservableCollection<BangumiInfo> BangumiInfoList { get; set; } = new ObservableCollection<BangumiInfo>();

        public DateTimeOffset Date { get; set; } = DateTimeOffset.Now;

        private HashSet<string> AddedCode = new HashSet<string>();

        public string FilterKey { get; set; }
        public long Code { get; set; }

        public async void GetItems()
        {
            await LoadingTask(getItems());
        }

        private async Task getItems()
        {
            var items = await biClient.GetBangumis(Date.Year, Date.Month);
            foreach (var bi in items)
            {
                bi.Code = bi.Url.Substring(bi.Url.LastIndexOf('/') + 1);
            }
            BangumiInfoListBak = BangumiInfoList = items.Where(bi => !AddedCode.Contains(bi.Code)).ToObservableCollection();
        }

        private void UpdateSource(BangumiInfo bi)
        {
            //BangumiInfoListBak = BangumiInfoListBak.Where(bi => !AddedCode.Contains(bi.Code)).ToObservableCollection();
            BangumiInfoList.Remove(bi);
            BangumiInfoListBak.Remove(bi);
        }

        public async void Add(BangumiInfo bi)
        {
            var code = bi.Url.Substring(bi.Url.LastIndexOf('/') + 1);
            try
            {
                bi.State = BangumiInfoState.Loading;
                var bgm = await AppCache.BClient.CreateByCode(code);
                bi.State = BangumiInfoState.Added;
            }
            catch (Exception ex)
            {
                bi.State = ex.Message.Contains("存在") ? BangumiInfoState.Extist : BangumiInfoState.Error;
            }
            if (bi.State == BangumiInfoState.Added || bi.State == BangumiInfoState.Extist)
            {
                AddedCode.Add(code);
                UpdateSource(bi);
                //Filter();
            }
        }

        public async void AddByCode()
        {
            if (Code > 0)
            {
                var bgm = await AppCache.BClient.CreateByCode(Code.ToString());
                await new MessageDialog("成功").ShowAsync();
                AddedCode.Add(Code.ToString());
            }
        }

        public async void AddByFile()
        {
            var fp = new FileOpenPicker();
            fp.FileTypeFilter.Add("*");
            fp.SuggestedStartLocation = PickerLocationId.Desktop;
            var file = await fp.PickSingleFileAsync();
            if (file != null)
            {
                var buffer = await FileIO.ReadBufferAsync(file);
                var bgm = await AppCache.BClient.CreateByFile(buffer.ToArray());
                await new MessageDialog("成功").ShowAsync();
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
