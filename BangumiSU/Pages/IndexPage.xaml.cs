using BangumiSU.SharedCode;
using System.Collections.Generic;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using static BangumiSU.SharedCode.AppCache;
using System;
using Windows.UI.Xaml;

namespace BangumiSU.Pages
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class IndexPage : Page
    {
        public IndexPage()
        {
            InitializeComponent();
            NavigationCacheMode = NavigationCacheMode.Enabled;
            InitSettings();
        }
        private async void InitSettings()
        {
            await Init(Settings.GetRoamingSetting());
            Model = new IndexViewModel();
            nv.SelectedItem = nv.MenuItems[0];
        }

        public IndexViewModel Model { get; set; }

        private void NavigationView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            var key = (args.SelectedItem as FrameworkElement)?.Tag as string;
            if (args.IsSettingsSelected)
                key = "SettingPage";
            (Model.SelectedPage as IContentPage)?.Leaved();
            Model.SelectedPage = Model.PageDict[key];
            (Model.SelectedPage as IContentPage)?.Arrived();
        }
    }

    public class IndexViewModel : ViewModels.ViewModelBase
    {
        public Dictionary<string, Page> PageDict { get; set; }

        public Page SelectedPage { get; set; }

        public IndexViewModel()
        {
            PageDict = new Dictionary<string, Page>
            {
                ["TrackingsPage"] = new TrackingsPage(),
                ["ManagePage"] = new ManagePage(),
                ["UpdatePage"] = new UpdatePage(),
                ["VideoPage"] = new VideoPage(),
                ["SettingPage"] = new SettingPage()
            };
            SelectedPage = PageDict["TrackingsPage"];
        }
    }

    public class PageLink
    {
        public string Name { get; set; }
        public string Glyph { get; set; }
        public Page Page { get; set; }

        public PageLink(string name, string glyph, Page page)
        {
            Name = name;
            Glyph = glyph;
            Page = page;
        }
    }
}
