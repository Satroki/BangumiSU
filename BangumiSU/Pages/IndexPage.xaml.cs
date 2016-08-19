using BangumiSU.Models;
using BangumiSU.SharedCode;
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
using System.Collections.ObjectModel;
using static BangumiSU.SharedCode.AppCache;
using System.Windows.Input;
using Windows.UI.Xaml.Media.Animation;

// “空白页”项模板在 http://go.microsoft.com/fwlink/?LinkId=234238 上有介绍

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
            Model = new IndexViewModel();
            DataContext = Model;
        }

        public IndexViewModel Model { get; set; }

        private void NavLinksList_ItemClick(object sender, ItemClickEventArgs e)
        {
            var link = (PageLink)e.ClickedItem;
            if (link.Page == null)
                splitView.IsPaneOpen = !splitView.IsPaneOpen;
            else
            {
                (Model.SelectedPage?.Page as IContentPage)?.Leaved();
                Model.SelectedPage = link;
                (Model.SelectedPage?.Page as IContentPage)?.Arrived();
            }
        }

        private void Theme_Tapped(object sender, TappedRoutedEventArgs e)
        {
            switch (RequestedTheme)
            {
                case ElementTheme.Default:
                case ElementTheme.Dark:
                    RequestedTheme = ElementTheme.Light;
                    break;
                case ElementTheme.Light:
                    RequestedTheme = ElementTheme.Dark;
                    break;
            }
            AppCache.Theme = RequestedTheme;
        }
    }

    public class IndexViewModel : ViewModels.ViewModelBase
    {
        public List<PageLink> PageLinks { get; set; }

        public PageLink SelectedPage { get; set; }

        public IndexViewModel()
        {
            PageLinks = new List<PageLink>
            {
                new PageLink("菜单","\uE700",null),
                new PageLink("首页","\uE10F",new TrackingsPage()),
                new PageLink("管理","\uE178",new ManagePage()),
                new PageLink("更新","\uE118",new UpdatePage()),
            };
            SelectedPage = PageLinks[1];
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
