using BangumiSU.ApiClients;
using BangumiSU.SharedCode;
using BangumiSU.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage.AccessCache;
using Windows.Storage.Pickers;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using static BangumiSU.SharedCode.AppCache;

namespace BangumiSU.Pages
{
    public sealed partial class SettingPage : Page, IContentPage
    {
        public SettingPage()
        {
            this.InitializeComponent();
        }

        private async void VideoFolder_Click(object sender, RoutedEventArgs e)
        {
            var token = nameof(VideoFolder);
            VideoFolder = await FolderHelper.PickFolder(token);
            txtFolder.Text = VideoFolder?.Path ?? string.Empty;
        }

        private async void FinishFolder_Click(object sender, RoutedEventArgs e)
        {
            var token = nameof(FinishFolder);
            FinishFolder = await FolderHelper.PickFolder(token);
            txtFinishFolder.Text = FinishFolder?.Path ?? string.Empty;
        }

        private async void Apply_Click(object sender, RoutedEventArgs e)
        {
            //AppSettings.UserPassword = pwbPassword.Password;
            AppSettings.FolderFormat = txtFolderFormat.Text;
            AppSettings.Extensions = txtExtensions.Text;
            AppSettings.UseInternalPlayer = tsUsePlayer.IsOn;
            AppSettings.UseInternalBrowser = tsUseInternalBrowser.IsOn;
            AppSettings.VideoSettings.ContinuousPlayback = tsContinuousPlayback.IsOn;
            AppSettings.UseLocalRss = tsRss.IsOn;
            AppSettings.LocalFilePattern = txtPattern.Text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            await Reload();
        }

        public void Arrived()
        {
            //pwbPassword.Password = AppSettings.UserPassword ?? string.Empty;
            txtFolder.Text = VideoFolder?.Path ?? string.Empty;
            txtFinishFolder.Text = FinishFolder?.Path ?? string.Empty;
            txtFolderFormat.Text = AppSettings.FolderFormat;
            txtExtensions.Text = AppSettings.Extensions;
            tsTheme.IsOn = Theme == ElementTheme.Dark;
            tsUsePlayer.IsOn = AppSettings.UseInternalPlayer;
            tsUseInternalBrowser.IsOn = AppSettings.UseInternalBrowser;
            tsContinuousPlayback.IsOn = AppSettings.VideoSettings.ContinuousPlayback;
            tsRss.IsOn = AppSettings.UseLocalRss;
            txtPattern.Text = string.Join(Environment.NewLine, AppSettings.LocalFilePattern);
        }

        public void Leaved()
        {
        }

        private void TsTheme_Toggled(object sender, RoutedEventArgs e)
        {
            Theme = tsTheme.IsOn ? ElementTheme.Dark : ElementTheme.Light;
            (Window.Current.Content as Frame).RequestedTheme = Theme;
        }

        private async void Clear_Click(object sender, RoutedEventArgs e)
        {
            await ImageHelper.DeleteImages();
            await this.Message("已清空");
        }

        private async void Check_Click(object sender, RoutedEventArgs e)
        {
            await new AccountClient().Check();
        }
    }
}
