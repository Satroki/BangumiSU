using BangumiSU.Models;
using BangumiSU.ViewModels;
using Windows.UI.Xaml.Controls;

namespace BangumiSU.Pages.Controls
{
    public sealed partial class BangumiDialog : ContentDialog
    {
        public BangumiDialog(Bangumi bgm)
        {
            InitializeComponent();
            Model = new BangumiViewModel(bgm);
        }

        public BangumiViewModel Model { get; set; }

        private async void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            await Model.Save();
        }

        private void ContentDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {

        }
    }
}
