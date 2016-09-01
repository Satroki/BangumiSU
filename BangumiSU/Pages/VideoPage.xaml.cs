using BangumiSU.ApiClients;
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
using BangumiSU.SharedCode;
using Windows.Storage.Pickers;
using Windows.Media.Core;
using Windows.UI.Xaml.Media.Animation;
using System.Threading.Tasks;
using BangumiSU.Controls;
using BangumiSU.Models;
using System.Diagnostics;

// “空白页”项模板在 http://go.microsoft.com/fwlink/?LinkId=234238 上有介绍

namespace BangumiSU.Pages
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class VideoPage : Page
    {
        public VideoPage()
        {
            this.InitializeComponent();
            timer.Tick += Timer_Tick;
            mediaElement.CurrentStateChanged += MediaElement_CurrentStateChanged;
        }

        private List<CommentBlock> Commemts = new List<CommentBlock>();
        private List<CommentBlock> TopCommemts = new List<CommentBlock>();
        private List<CommentBlock> BottomCommemts = new List<CommentBlock>();

        private int commentIndex = 0;

        private void MediaElement_CurrentStateChanged(object sender, RoutedEventArgs e)
        {
            switch (mediaElement.CurrentState)
            {
                case MediaElementState.Closed:
                    break;
                case MediaElementState.Opening:
                    break;
                case MediaElementState.Buffering:
                    break;
                case MediaElementState.Playing:
                    timer.Start();
                    break;
                case MediaElementState.Paused:
                    timer.Stop();
                    break;
                case MediaElementState.Stopped:
                    break;
                default:
                    break;
            }
        }

        private void Timer_Tick(object sender, object e)
        {
            var p = mediaElement.Position;
        }

        private DanDanClient dc = new DanDanClient();
        private DispatcherTimer timer = new DispatcherTimer() { Interval = TimeSpan.FromSeconds(0.1) };

        private async void button_Click(object sender, RoutedEventArgs e)
        {
            var list = await dc.GetComments(90420005);
            Commemts = list.OrderBy(c => c.Time).Select(c => new CommentBlock(c)).ToList();
        }

        private async void AppBarButton_Click(object sender, RoutedEventArgs e)
        {
            var fp = new FileOpenPicker();
            fp.FileTypeFilter.Add("*");
            fp.SuggestedStartLocation = PickerLocationId.ComputerFolder;
            var file = await fp.PickSingleFileAsync();
            if (file != null)
            {
                mediaElement.SetPlaybackSource(MediaSource.CreateFromStorageFile(file));
            }

        }

        private async void AppBarButton_Click_1(object sender, RoutedEventArgs e)
        {
            var c = new Comment
            {
                Color = 0xFFFFFF,
                Message = "1234测试",
            };
            for (int i = 0; i < 500; i++)
            {
                var txt = new CommentBlock(c)
                {
                    FontSize = 28,
                };
                Canvas.SetTop(txt, (i % 10) * 40);
                Canvas.SetLeft(txt, canvas.ActualWidth);
                canvas.Children.Add(txt);
                var sb = new Storyboard();
                var ani = new DoubleAnimation();
                ani.To = 0;
                ani.Duration = new Duration(TimeSpan.FromSeconds(5));
                Storyboard.SetTargetProperty(ani, "(Canvas.Left)");
                Storyboard.SetTarget(ani, txt);
                sb.Children.Add(ani);
                sb.Begin();
                await Task.Delay(100);
            }
        }
    }
}
