﻿using BangumiSU.ApiClients;
using BangumiSU.Models;
using BangumiSU.Pages;
using BangumiSU.Pages.Controls;
using BangumiSU.SharedCode;
using Newtonsoft.Json;
using PropertyChanged;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using static BangumiSU.SharedCode.AppCache;

namespace BangumiSU.ViewModels
{
    public class ManageViewModel : ViewModelBase
    {
        public ManageViewModel(Bangumi bgm = null)
        {
            if (bgm != null)
            {
                Bangumis = new ObservableCollection<Bangumi>() { bgm };
                SelectedBangumi = bgm;
            }
        }

        #region 属性
        public string SearchKey { get; set; }

        public string[] SearchMode { get; } = new[]
        {
            nameof(Bangumi.Schedule),
            nameof(Bangumi.Name),
            nameof(Bangumi.LocalName),
            nameof(Bangumi.Author),
            nameof(Bangumi.AnimeCompany),
            nameof(Bangumi.HomePage),
            nameof(Bangumi.BangumiCode),
           "Any",
        };

        public string SearchProp
        {
            get { return AppSettings.LastSearch; }
            set { AppSettings.LastSearch = value; }
        }

        public Bangumi SelectedBangumi { get; set; }

        [DependsOn(nameof(SelectedBangumi))]
        public bool ShowDetail => SelectedBangumi != null;

        public Tracking SelectedTracking { get; set; }

        public ObservableCollection<Bangumi> Bangumis { get; set; } = new ObservableCollection<Bangumi>();
        #endregion

        #region 方法

        public async Task Search()
        {
            Message = "查询中……";
            if (string.IsNullOrEmpty(SearchKey))
                Bangumis = new ObservableCollection<Bangumi>(await BClient.GetBangumis());
            else
                Bangumis = new ObservableCollection<Bangumi>(await BClient.Search(SearchProp, SearchKey));
            Message = "";
        }

        public async void VisitHP()
        {
            if (SelectedBangumi?.HomePage?.IsEmpty() == false)
                await SelectedBangumi.HomePage.LaunchToWeb();
        }

        public async void VisitBgm()
        {
            if (SelectedBangumi != null)
                await ("https://bangumi.tv/subject/" + SelectedBangumi.BangumiCode).LaunchToWeb();
        }

        public async void EditBgm()
        {
            if (SelectedBangumi != null)
            {
                var temp = SelectedBangumi;
                var dlg = new BangumiDialog(temp);
                var result = await dlg.ShowAsync();
                if (result == ContentDialogResult.Primary)
                {
                    Bangumis.Replace(temp, dlg.Model.Bangumi);
                    SelectedBangumi = dlg.Model.Bangumi;
                }
            }
        }

        public async Task Score()
        {
            var b = SelectedBangumi;
            if (b != null)
            {
                var d = new ScoreDialog(b);
                await d.ShowAsync();
            }
        }

        public async void EditTracking()
        {
            if (SelectedTracking != null)
            {
                var temp = SelectedTracking;
                var dlg = new TrackingDialog(temp, true);
                var result = await dlg.ShowAsync();
                if (result == ContentDialogResult.Primary)
                {
                    var bgm = dlg.Model.Tracking.Bangumi;
                    Bangumis.Replace(SelectedBangumi, bgm);
                    SelectedBangumi = bgm;
                }
            }
        }

        public async void OpenTracking()
        {
            if (SelectedTracking?.Folder?.IsEmpty() == false)
            {
                if (SelectedTracking.Online)
                    await SelectedTracking.Folder.LaunchToWeb();
                else
                    await SelectedTracking.Folder.LaunchAsFolder();
            }
        }

        public async void DeleteBgm()
        {
            if (SelectedBangumi != null)
            {
                Message = "等待删除……";
                var temp = SelectedBangumi;
                var r = await BClient.Delete(temp.Id);
                if (r > 0)
                    Bangumis.Remove(temp);
                Message = "";
            }
        }

        public async void DeleteTracking()
        {
            if (SelectedTracking != null)
            {
                Message = "等待删除……";
                var temp = SelectedTracking;
                var bgm = SelectedBangumi;
                var r = await TClient.Delete(temp.Id);
                if (r > 0)
                    bgm.Trackings.Remove(temp);
                Message = "";
            }
        }

        public async void DropBgm(string code)
        {
            Message = "正在添加……";
            var bgm = await BClient.CreateByCode(code);
            Bangumis.Add(bgm);
            Message = "";
        }

        public async void UpdateInfo()
        {
            if (SelectedBangumi != null)
            {
                Message = "同步中……";
                var temp = SelectedBangumi;
                var bgm = await BClient.Update(temp.Id);
                Bangumis.Replace(temp, bgm);
                SelectedBangumi = bgm;
                Message = "";
            }
        }

        public void MusicInfo()
        {
            NavigationHelper.Navigate(typeof(MusicPage), Bangumis);
        }

        public void OrderBy(object sender, RoutedEventArgs e)
        {
            var t = ((FrameworkElement)sender).Tag.ToString();
            IEnumerable<Bangumi> list = Bangumis;
            switch (t)
            {
                case nameof(Bangumi.OnAir):
                    list = Bangumis.OrderBy(b => b.OnAir);
                    break;
                case nameof(Bangumi.DayString):
                    list = Bangumis.OrderBy(b => (int)b.OnAir.LocalDateTime.DayOfWeek);
                    break;
                case nameof(Bangumi.LocalName):
                    list = Bangumis.OrderBy(b => b.LocalName);
                    break;
                case nameof(Bangumi.AnimeCompany):
                    list = Bangumis.OrderBy(b => b.AnimeCompany);
                    break;
            }
            Bangumis = list.ToObservableCollection();
        }

        public async void Export()
        {
            if (Bangumis.IsEmpty())
                return;

            var fsp = new FileSavePicker()
            {
                CommitButtonText = "保存",
                DefaultFileExtension = ".json",
                SuggestedFileName = "bangumis",
                SuggestedStartLocation = PickerLocationId.DocumentsLibrary,
            };
            fsp.FileTypeChoices.Add("数据", new string[] { ".json" });
            var f = await fsp.PickSaveFileAsync();
            if (f != null)
            {
                var json = JsonConvert.SerializeObject(Bangumis);
                await FileIO.WriteTextAsync(f, json);
                await new MessageDialog("导出完成").ShowAsync();
            }
        }

        public async void Download()
        {
            var fsp = new FileSavePicker()
            {
                CommitButtonText = "保存",
                DefaultFileExtension = ".json",
                SuggestedFileName = "bangumis",
                SuggestedStartLocation = PickerLocationId.DocumentsLibrary,
            };
            fsp.FileTypeChoices.Add("数据", new string[] { ".json" });
            var f = await fsp.PickSaveFileAsync();
            if (f != null)
            {
                var mc = new ManageClient();
                var data = await mc.Download();
                await FileIO.WriteTextAsync(f, data);
                await new MessageDialog("备份完成").ShowAsync();
            }
        }

        public async void Upload()
        {
            var fop = new FileOpenPicker()
            {
                CommitButtonText = "确定",
                SuggestedStartLocation = PickerLocationId.DocumentsLibrary,
            };
            fop.FileTypeFilter.Add(".json");
            var f = await fop.PickSingleFileAsync();
            if (f != null)
            {
                var data = await FileIO.ReadTextAsync(f, Windows.Storage.Streams.UnicodeEncoding.Utf8);
                var mc = new ManageClient();
                var count = await mc.Upload(data);
                await new MessageDialog($"完成，共{count}条").ShowAsync();
            }
        }

        public void Add()
        {
            NavigationHelper.Navigate<BangumiInfoPage>();
        }
        #endregion
    }
}
