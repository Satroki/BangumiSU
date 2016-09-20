using BangumiSU.Models;
using BangumiSU.ViewModels;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace BangumiSU.Pages
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class BangumiInfoPage : Page
    {
        public BangumiInfoPage()
        {
            this.InitializeComponent();
            Model = new BangumiInfoViewModel();
        }

        public BangumiInfoViewModel Model { get; set; }

        private void Add_Click(object sender, RoutedEventArgs e)
        {
            var bi = ((Button)sender).DataContext as BangumiInfo;
            Model.Add(bi);
        }

        private void AutoSuggestBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            Model.Filter();
        }
    }
}
