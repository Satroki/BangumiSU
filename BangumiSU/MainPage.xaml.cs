using BangumiSU.Pages;
using BangumiSU.SharedCode;
using System;
using Windows.Storage.AccessCache;
using Windows.Storage.Pickers;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using static BangumiSU.SharedCode.AppCache;

namespace BangumiSU
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
            getSettings();
        }

        private async void getSettings()
        {
            await Init(Settings.GetRoamingSetting());

            pwbPassword.Password = AppSettings.UserGUID ?? string.Empty;
            txtFolder.Text = VideoFolder?.Path ?? string.Empty;
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            var token = nameof(VideoFolder);
            var fp = new FolderPicker();
            fp.FileTypeFilter.Add("*");
            fp.SuggestedStartLocation = PickerLocationId.ComputerFolder;
            var folder = await fp.PickSingleFolderAsync();
            if (folder != null)
            {
                VideoFolder = folder;
                txtFolder.Text = folder.Path;
                StorageApplicationPermissions.FutureAccessList.AddOrReplace(token, folder);
            }
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Exit();
        }

        private async void Next_Click(object sender, RoutedEventArgs e)
        {
            AppSettings.UserGUID = pwbPassword.Password;
            if (AppSettings.UserGUID.IsEmpty())
                await this.Message("请输入GUID");
            else
                Frame.Navigate(typeof(IndexPage));
        }
    }
}
