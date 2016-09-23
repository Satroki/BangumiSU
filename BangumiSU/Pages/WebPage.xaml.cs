using System;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

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

        private static WebView web;
        private long gbt;
        private long gft;

        private void AddWebView()
        {
            if (web == null)
            {
                web = new WebView(WebViewExecutionMode.SameThread);
            }
            web.NewWindowRequested += Web_NewWindowRequested;
            web.LoadCompleted += Web_LoadCompleted;
            web.ContentLoading += Web_ContentLoading;
            gbt = web.RegisterPropertyChangedCallback(WebView.CanGoBackProperty,
                (s, e) => btnGoBack.IsEnabled = web.CanGoBack);
            gft = web.RegisterPropertyChangedCallback(WebView.CanGoForwardProperty,
                (s, e) => btnGoForward.IsEnabled = web.CanGoForward);
            webContent.Content = web;
        }

        private void RemoveWebView()
        {
            web.NewWindowRequested -= Web_NewWindowRequested;
            web.LoadCompleted -= Web_LoadCompleted;
            web.ContentLoading -= Web_ContentLoading;
            web.UnregisterPropertyChangedCallback(WebView.CanGoBackProperty, gbt);
            web.UnregisterPropertyChangedCallback(WebView.CanGoForwardProperty, gft);
            webContent.Content = null;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            AddWebView();
            var uri = e.Parameter.ToString();
            txtUri.Text = uri;
            WebNavigate(uri);
            base.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            RemoveWebView();
            base.OnNavigatedFrom(e);
        }

        private void AutoSuggestBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            WebNavigate(args.QueryText);
        }

        private void Web_NewWindowRequested(WebView sender, WebViewNewWindowRequestedEventArgs args)
        {
            txtUri.Text = args.Uri.AbsoluteUri;
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

        private void Web_LoadCompleted(object sender, NavigationEventArgs e)
        {
            pr.IsActive = false;
        }

        private void Web_ContentLoading(WebView sender, WebViewContentLoadingEventArgs args)
        {
            pr.IsActive = true;
        }

        public void Stop() => web.Stop();
        public void Refresh() => web.Refresh();
        public void GoBack() => web.GoBack();
        public void GoForward() => web.GoForward();
    }
}
