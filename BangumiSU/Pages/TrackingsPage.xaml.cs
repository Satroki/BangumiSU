using BangumiSU.Models;
using BangumiSU.Pages.Controls;
using BangumiSU.SharedCode;
using BangumiSU.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text.RegularExpressions;
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
    public sealed partial class TrackingsPage : Page, IContentPage
    {
        public TrackingsPage()
        {
            this.InitializeComponent();
            Model = new MainViewModel();
        }

        public MainViewModel Model { get; set; }

        private void AutoSuggestBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            Model.Search(args.QueryText);
        }

        private void MusicInfo_Click(object sender, RoutedEventArgs e)
        {

        }

        private void VisitBgm_Click(object sender, RoutedEventArgs e)
        {
            Model.VisitBgm();
        }

        private void VisitHP_Click(object sender, RoutedEventArgs e)
        {
            Model.VisitHP();
        }

        private void AdjustTime_Click(object sender, RoutedEventArgs e)
        {
            Model.AdjustTime();
        }

        private async void AddProgress_Click(object sender, RoutedEventArgs e)
        {
            await Model.AddProgress();
        }

        private void Button_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var t = (sender as Button).DataContext as Tracking;
            Model.SelectedTracking = t;
        }

        public void Arrived()
        {

        }

        public void Leaved()
        {

        }
    }
}
