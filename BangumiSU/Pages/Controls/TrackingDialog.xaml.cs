using BangumiSU.Models;
using BangumiSU.SharedCode;
using BangumiSU.ViewModels;
using Windows.UI.Xaml.Controls;

namespace BangumiSU.Pages.Controls
{
    public sealed partial class TrackingDialog : ContentDialog
    {
        public TrackingDialog(Tracking t, bool edit = false)
        {
            InitializeComponent();
            Model = new TrackingViewModel(t, edit);
            RequestedTheme = AppCache.Theme;
        }

        public TrackingViewModel Model { get; set; }

        private async void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            await Model.Save();
            if (Model.Tracking.BangumiId == 0)
                args.Cancel = true;
        }

        private void ContentDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
        }
    }
}
