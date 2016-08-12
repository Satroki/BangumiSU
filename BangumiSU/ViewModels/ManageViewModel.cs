using BangumiSU.Models;
using BangumiSU.SharedCode;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using static BangumiSU.SharedCode.AppCache;

namespace BangumiSU.ViewModels
{
    public class ManageViewModel : ViewModelBase
    {
        public ManageViewModel(Bangumi bgm = null)
        {
            if (bgm != null)
            {
                BgmList = new ObservableCollection<Bangumi>() { bgm };
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
            nameof(Bangumi.BangumiCode)
        };

        public string SearchProp { get; set; } = AppSettings.LastSearch;

        public Bangumi SelectedBangumi { get; set; }

        public Tracking SelectedTracking { get; set; }

        public ObservableCollection<Bangumi> BgmList { get; set; }
        #endregion

        #region 方法
        public void SaveSettings()
        {
           AppSettings.LastSearch = SearchProp;
        }

        public async Task Search()
        {
            if (string.IsNullOrEmpty(SearchKey))
                BgmList = new ObservableCollection<Bangumi>(await BClient.GetBangumis());
            else
                BgmList = new ObservableCollection<Bangumi>(await BClient.Search(SearchProp, SearchKey));
        }

        public void VisitHP()
        {
            //if (SelectedBangumi?.HomePage?.IsEmpty() == false)
            //    Process.Start(SelectedBangumi.HomePage);
        }

        public void VisitBgm()
        {
            //if (SelectedBangumi != null)
            //    Process.Start("http://bangumi.tv/subject/" + SelectedBangumi.BangumiCode);
        }

        public void EditBgm()
        {
            if (SelectedBangumi != null)
            {
                var bvm = new BangumiViewModel(SelectedBangumi);
                //var view = new Views.BangumiView(bvm);
                //if (view.ShowDialog() == true)
                //{
                //    BgmList.Replace(SelectedBangumi, bvm.Bangumi);
                //    SelectedBangumi = bvm.Bangumi;
                //}
            }
        }

        public void EditTracking()
        {
            if (SelectedTracking != null)
            {
                //var vm = new TrackingViewModel();
                //vm.Tracking = SelectedTracking;
                //vm.EditMode = true;
                //var view = new Views.TrackingView(vm);
                //view.Owner = win;
                //view.ShowDialog();
            }
        }

        public void OpenTracking()
        {
            //if (SelectedTracking?.Folder?.IsEmpty() == false)
            //    Process.Start(SelectedTracking.Folder);
        }

        public async void DelBgm()
        {
            if (SelectedBangumi != null)
            {
                var r = await BClient.Delete(SelectedBangumi.Id);
                if (r > 0)
                    BgmList.Remove(SelectedBangumi);
            }
        }

        public async void DeleteTracking()
        {
            if (SelectedTracking != null)
            {
                var r = await TClient.Delete(SelectedTracking.Id);
                if (r > 0)
                    SelectedBangumi.Trackings.Remove(SelectedTracking);
            }
        }

        public async void DropBgm(string code)
        {
            var bgm = await BClient.CreateByCode(code);
            BgmList.Add(bgm);
        }

        public async void UpdateInfo()
        {
            if (SelectedBangumi != null)
                await BClient.Update(SelectedBangumi.Id);
        }

        public void MusicInfo()
        {
            var vm = new MusicViewModel(BgmList);
            //var view = new Views.MusicView(vm);
            //view.Show();
        }
        #endregion
    }
}
