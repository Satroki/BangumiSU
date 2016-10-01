using BangumiSU.Models;
using BangumiSU.SharedCode;
using BangumiSU.ViewModels;
using System;
using Windows.ApplicationModel.DataTransfer;
using Windows.UI.Xaml.Controls;

namespace BangumiSU.Pages.Controls
{
    public sealed partial class BangumiDialog : ContentDialog
    {
        public BangumiDialog(Bangumi bgm)
        {
            InitializeComponent();
            Model = new BangumiViewModel(bgm);
            RequestedTheme = AppCache.Theme;
        }

        public BangumiViewModel Model { get; set; }

        private async void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            await Model.Save();
        }

        private void ContentDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {

        }

        private void ContentDialog_DragOver(object sender, Windows.UI.Xaml.DragEventArgs e)
        {
            e.AcceptedOperation = DataPackageOperation.Link;
        }

        private async void ContentDialog_Drop(object sender, Windows.UI.Xaml.DragEventArgs e)
        {
            if (e.DataView.Contains(StandardDataFormats.Text))
            {
                var uri = await e.DataView.GetTextAsync();
                Model.Bangumi.OnlineLink = uri;
            }
        }
    }
}
