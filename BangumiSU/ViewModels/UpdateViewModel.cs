﻿using BangumiSU.ApiClients;
using BangumiSU.Models;
using BangumiSU.SharedCode;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        public List<RssItem> RssItemsBak { get; set; } = new List<RssItem>();

        public ObservableCollection<RssItem> SelectedItems { get; set; } = new ObservableCollection<RssItem>();

        public string KeyWords { get; set; }
        #endregion

        #region 命令
        public async Task Refresh()
        {
            await LoadingTask(GetRss());
        }

        public void MatchItem()
        {
            if (!AppSettings.RssPattern.IsEmpty())
            {
                foreach (var item in RssItems)
                    item.IsSelected = Regex.IsMatch(item.Title, AppSettings.RssPattern, RegexOptions.IgnoreCase);
            }
        }

        private async Task GetRss()
        {
            Message = "正在获取RSS……";
            //var date = AppSettings.LastUpdate;
            //var items = await rssClient.GetRss();
            var date = DateTime.Today.AddDays(-1);
            var items = await rssClient.GetRss(date);
            ScanItems(items);
            RssItemsBak = RssItems = items;
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
            var link = item.Link.Replace("http://", "https://");
            await link.LaunchToWeb();
        }

        public void Clear()
        {
            foreach (var item in RssItems)
                item.IsSelected = false;
        }

        public void FilterItems()
        {
            RssItems = RssItemsBak.Where(r => r.Title.IndexOf(KeyWords, StringComparison.OrdinalIgnoreCase) >= 0).ToList();
        }

        public async void Search()
        {
            string url = AppSettings.DmhySearch + KeyWords?.Replace(' ', '+');
            await url.LaunchToWeb();
        }

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
