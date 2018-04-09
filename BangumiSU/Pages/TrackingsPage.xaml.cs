using BangumiSU.Models;
using BangumiSU.SharedCode;
using BangumiSU.ViewModels;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

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
            Model = new TrackingsViewModel();
        }

        public TrackingsViewModel Model { get; set; }

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

        private async void Border_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            await Model.Open();
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            if (Model.Trackings.IsEmpty())
                await Model.Refresh();
        }
    }
}
