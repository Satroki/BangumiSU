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
    public sealed partial class WebPage : Page
    {
        public WebPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            var uri = e.Parameter.ToString();
            txtUri.Text = uri;
            WebNavigate(uri);
        }

        private void AutoSuggestBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            WebNavigate(args.QueryText);
        }

        private void web_NewWindowRequested(WebView sender, WebViewNewWindowRequestedEventArgs args)
        {
            web.Navigate(args.Uri);
            args.Handled = true;
        }

        private void WebNavigate(string uri)
        {
            if (!uri.StartsWith("http"))
                uri = "http://" + uri;
            txtUri.Text = uri;
            web.Navigate(new Uri(uri));
        }

        private void web_LoadCompleted(object sender, NavigationEventArgs e)
        {
            pr.IsActive = false;
        }

        private void web_ContentLoading(WebView sender, WebViewContentLoadingEventArgs args)
        {
            pr.IsActive = true;
        }
    }
}
