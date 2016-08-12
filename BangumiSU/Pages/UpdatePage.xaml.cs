using BangumiSU.Models;
using BangumiSU.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
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
    public sealed partial class UpdatePage : Page
    {
        public UpdatePage()
        {
            InitializeComponent();
            Model = new UpdateViewModel();
        }

        public UpdateViewModel Model { get; set; }

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
