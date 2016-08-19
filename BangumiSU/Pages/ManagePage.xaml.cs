using BangumiSU.Models;
using BangumiSU.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace BangumiSU.Pages
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class ManagePage : Page
    {
        public ManagePage()
        {
            this.InitializeComponent();
            Model = new ManageViewModel();
        }

        public ManageViewModel Model { get; set; }

        private void ListView_DragOver(object sender, DragEventArgs e)
        {
            e.AcceptedOperation = DataPackageOperation.Link;
        }

        private async void ListView_Drop(object sender, DragEventArgs e)
        {
            if (e.DataView.Contains(StandardDataFormats.Text))
            {
                var uri = await e.DataView.GetTextAsync();
                var code = uri.Substring(uri.LastIndexOf('/') + 1);
                int t;
                if (int.TryParse(code, out t))
                    Model.DropBgm(code);
            }
        }

        private void Tracking_Click(object sender, RoutedEventArgs e)
        {
            var t = ((Button)sender).DataContext as Tracking;
            Model.SelectedTracking = t;
        }

        private void EditTracking_Click(object sender, RoutedEventArgs e)
        {
            Model.EditTracking();
        }

        private void DeleteTracking_Click(object sender, RoutedEventArgs e)
        {
            Model.DeleteTracking();
        }

        private void OpenTracking_Click(object sender, RoutedEventArgs e)
        {
            Model.OpenTracking();
        }
    }
}
