using BangumiSU.Models;
using BangumiSU.Pages.Controls;
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
    public sealed partial class TrackingsPage : Page
    {
        public TrackingsPage()
        {
            this.InitializeComponent();
        }

        private async void button_Click(object sender, RoutedEventArgs e)
        {
            var bgm = await SharedCode.AppCache.BClient.GetBangumi(1234);
            await new BangumiDialog(bgm).ShowAsync();
        }

        private async void button1_Click(object sender, RoutedEventArgs e)
        {
            var t = new Tracking
            {
                Count = 1,
                Progress = 1,
                KeyWords = "123131",
            };
            await new TrackingDialog(t).ShowAsync();
        }
    }
}
