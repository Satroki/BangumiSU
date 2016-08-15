using BangumiSU.ApiClients;
using BangumiSU.Models;
using BangumiSU.SharedCode;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static BangumiSU.SharedCode.AppCache;

namespace BangumiSU.ViewModels
{
    public class UpdateViewModel : ViewModelBase
    {
        public UpdateViewModel()
        {
            rssClient = new RssClient();
        }

        private RssClient rssClient;

        #region 属性
        public List<RssItem> RssItems { get; set; } = new List<RssItem>();

        public List<RssItem> SelectedItems { get; set; } = new List<RssItem>();
        #endregion

        #region 命令
        public async Task Refresh()
        {
            await LoadingTask(GetRss());
        }

        private async Task GetRss()
        {
            Message = "正在获取RSS……";
            var date = AppSettings.LastUpdate;
            var items = await rssClient.GetRss(date);
            ScanItems(items);
            RssItems = items;
        }

        public async Task Download()
        {
            await LoadingTask(Launch());
        }

        private async Task Launch()
        {
            var tempList = SelectedItems.ToList();
            foreach (var item in tempList)
            {
                await item.Magnet.LaunchAsUri();
                item.IsSelected = false;
                await Task.Delay(2000);
            }
        }

        public async void OpenLink(RssItem item)
        {
            await item.Link.LaunchAsUri();
        }

        public void Clear()
        {
            foreach (var item in RssItems)
                item.IsSelected = false;
        }
        #endregion

        #region 方法
        private void ScanItems(IEnumerable<RssItem> list)
        {
            var last = AppSettings.LastUpdate;
            AppSettings.LastUpdate = list.FirstOrDefault()?.PubDate.LocalDateTime ?? DateTime.Now;

            if (!AppSettings.RssPattern.IsEmpty())
            {
                foreach (var item in list)
                    item.IsSelected = Regex.IsMatch(item.Title, AppSettings.RssPattern, RegexOptions.IgnoreCase);
            }
            Message = "完成。起始时间：" + last.ToString();
        }
        #endregion
    }
}
