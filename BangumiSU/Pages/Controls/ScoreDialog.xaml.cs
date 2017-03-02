using BangumiSU.Models;
using BangumiSU.SharedCode;
using BangumiSU.ViewModels;
using Windows.UI.Xaml.Controls;

namespace BangumiSU.Pages.Controls
{
    public sealed partial class ScoreDialog : ContentDialog
    {
        public ScoreDialog()
        {
            this.InitializeComponent();
        }

        public ScoreDialog(Bangumi bgm)
        {
            InitializeComponent();
            Model = new ScoreViewModel(bgm);
            RequestedTheme = AppCache.Theme;
        }

        public ScoreViewModel Model { get; set; }

        private async void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            await Model.Update();
        }

        private void ContentDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
        }
    }
}
