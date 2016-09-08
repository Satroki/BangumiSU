using BangumiSU.Models;
using BangumiSU.SharedCode;
using BangumiSU.ViewModels;
using System.Diagnostics;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace BangumiSU.Pages
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class UpdatePage : Page, IContentPage
    {
        public UpdatePage()
        {
            InitializeComponent();
            Model = new UpdateViewModel();
        }

        public UpdateViewModel Model { get; set; }

        public async void Arrived()
        {
            if (Model.RssItems.IsEmpty())
                await Model.Refresh();
        }

        public void Leaved()
        {

        }

        private void GridView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            foreach (RssItem ai in e.AddedItems)
                Model.SelectedItems.Add(ai);
            foreach (RssItem ri in e.RemovedItems)
                Model.SelectedItems.Remove(ri);
        }

        private void OpenLink_Click(object sender, RoutedEventArgs e)
        {
            var item = ((Button)sender).DataContext as RssItem;
            if (item != null)
                Model.OpenLink(item);
        }
    }
}
