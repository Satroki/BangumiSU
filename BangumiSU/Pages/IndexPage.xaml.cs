using BangumiSU.SharedCode;
using System.Collections.Generic;
using Windows.UI.Xaml.Controls;
using static BangumiSU.SharedCode.AppCache;

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
            InitSettings();
        }
        private async void InitSettings()
        {
            await Init(Settings.GetRoamingSetting());
            Model = new IndexViewModel();
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
                new PageLink("设置","\uE115",new SettingPage()),
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
