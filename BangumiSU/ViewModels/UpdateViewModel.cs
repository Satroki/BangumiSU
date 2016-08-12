using BangumiSU.Models;
using BangumiSU.ApiClients;
using BangumiSU.SharedCode;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
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
            RssItems = await LoadingTask(GetRss());
        }

        private async Task<List<RssItem>> GetRss()
        {
            Message = "正在获取RSS……";

            //var date = AppSettings.LastUpdate;
            var items = await rssClient.GetRss();
            for (int i = 0; i < items.Count; i++)
            {
                if (i % 3 == 0)
                    items[i].IsSelected = true;
            }
           
            Message = "获取完成，开始扫描……";
            scanItems(items, "");
            return items;
        }

        public async Task Download()
        {
            //IsButtonEnabled = false;
            //await Task.Run(() =>
            //{
            //    try
            //    {
            //        while (SelectedItems.Count > 0)
            //        {
            //            string file = SelectedItems[0].Magnet;
            //            //Process.Start(file);
            //            //Application.Current.Dispatcher.Invoke(() => SelectedItems.RemoveAt(0));
            //            //if (SelectedItems.Count > 0)
            //            //    Thread.Sleep(2000);
            //        }
            //    }
            //    catch (Exception ex)
            //    {
            //        Message = ex.Message;
            //    }
            //    finally
            //    {
            //        IsButtonEnabled = true;
            //    }
            //});
        }

        public async void OpenLink(RssItem item)
        {
            await item.Link.LaunchAsUri();
        }
        #endregion

        #region 方法
        //private string getPattern()
        //{
        //    if (list.IsEmpty())
        //        return null;
        //    var sb = new StringBuilder();
        //    foreach (var a in list)
        //    {
        //        if (!string.IsNullOrEmpty(a.KeyWords))
        //        {
        //            var keys = a.KeyWords.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
        //            sb.Append("((.*)");
        //            foreach (var s in keys)
        //                sb.Append(s).Append("(.*)");
        //            sb.Append(")|");
        //        }
        //    }
        //    var result = sb.ToString().TrimEnd('|');
        //    return result;
        //}

        private void scanItems(IEnumerable<RssItem> list, string pattern)
        {
            var last = AppSettings.LastUpdate;
            AppSettings.LastUpdate = list.FirstOrDefault()?.PubDate.LocalDateTime ?? DateTime.Now;
            foreach (var item in list)
            {
                RssItems.Add(item);
                if (pattern.IsEmpty())
                    continue;
                if (Regex.IsMatch(item.Title, pattern, RegexOptions.IgnoreCase))
                    SelectedItems.Add(item);
            }
            Message = "扫描完成。起始时间：" + last.ToString();
        }
        #endregion
    }
}
