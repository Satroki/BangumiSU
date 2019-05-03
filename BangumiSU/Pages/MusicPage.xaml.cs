using BangumiSU.Models;
using BangumiSU.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// “空白页”项模板在 http://go.microsoft.com/fwlink/?LinkId=234238 上有介绍

namespace BangumiSU.Pages
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class MusicPage : Page
    {
        public MusicPage()
        {
            this.InitializeComponent();
        }

        public MusicViewModel Model { get; set; } = new MusicViewModel();

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            var bangumis = e.Parameter as IEnumerable<Bangumi>;
            Model = new MusicViewModel(bangumis);
            root.DataContext = Model;
            base.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            Model.Clear();
            base.OnNavigatedFrom(e);
        }

        private void AutoSuggestBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            Model.OpenFolder(args.QueryText);
        }

        private async void Save_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button b && b.DataContext is Bangumi bgm)
                await Model.UpdateBangumiMusic(bgm);
        }

        private async void Sync_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button b && b.DataContext is Bangumi bgm)
                await Model.SyncBangumiMusic(bgm);
        }

        private void Copy_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button b && b.DataContext is MusicInfo m)
            {
                var dp = new DataPackage();
                dp.SetText(m.Name);
                Clipboard.SetContent(dp);
            }
        }
    }
}
