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
using System.Collections.Concurrent;
using BangumiSU.ViewModels;
using Windows.Security.Cryptography.Core;
using Windows.Security.Cryptography;

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
            mediaElement.DoubleTapped += MediaElement_DoubleTapped; ;
        }

        public VideoViewModel Model { get; set; } = new VideoViewModel();

        private List<Comment> Comments = new List<Comment>();
        private List<Storyboard> Storyboards = new List<Storyboard>();
        private Dictionary<Storyboard, CommentBlock> InScreenComments = new Dictionary<Storyboard, CommentBlock>();

        private int commentIndex = 0;
        private uint bufferMD5 = 16 * 1024 * 1024;
        private double fontsize = 24;
        private double seconds = 5;
        private double lastTick = 0;
        private DanDanClient dc = new DanDanClient();
        private DispatcherTimer timer = new DispatcherTimer() { Interval = TimeSpan.FromSeconds(0.1) };
        private double[] rowTimeLines = new double[0];
        private double rowHeight = 0;
        private double rowWidth = 0;

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
                    Storyboards.ForEach(sb => sb.Resume());
                    break;
                case MediaElementState.Paused:
                    timer.Stop();
                    Storyboards.ForEach(sb => sb.Pause());
                    break;
                case MediaElementState.Stopped:
                    ResetAll();
                    break;
                default:
                    break;
            }
        }

        private void MediaElement_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            switch (mediaElement.CurrentState)
            {
                case MediaElementState.Playing:
                    mediaElement.Pause();
                    break;
                case MediaElementState.Paused:
                    mediaElement.Play();
                    break;
                case MediaElementState.Stopped:
                    break;
            }
        }

        private void Timer_Tick(object sender, object e)
        {
            var p = mediaElement.Position;
            if (p.TotalSeconds < lastTick)
            {
                ResetAll();
            }
            lastTick = p.TotalSeconds;
            var index = Comments.FindIndex(c => c.Time >= lastTick - 0.5);
            commentIndex = Math.Max(commentIndex, index);
            for (; commentIndex < Comments.Count; commentIndex++)
            {
                var c = Comments[commentIndex];
                if (c.Time < lastTick + 0.3)
                {
                    var cb = CreateCommentBlock(c);
                    CreateStoryboard(cb);
                }
                else
                {
                    break;
                }
            }
        }

        private async void button_Click(object sender, RoutedEventArgs e)
        {

        }

        private async void AppBarButton_Click(object sender, RoutedEventArgs e)
        {
            var fp = new FileOpenPicker();
            fp.FileTypeFilter.Add("*");
            fp.SuggestedStartLocation = PickerLocationId.ComputerFolder;
            var file = await fp.PickSingleFileAsync();
            if (file != null)
            {
                Model.Message = "计算MD5";
                var md5 = HashAlgorithmProvider.OpenAlgorithm(HashAlgorithmNames.Md5);
                var buffer = new byte[bufferMD5].AsBuffer();
                long fileLength = 0;
                using (var s = await file.OpenReadAsync())
                {
                    fileLength = (long)s.Size;
                    var length = (uint)Math.Min(bufferMD5, s.Size);
                    await s.ReadAsync(buffer, length, Windows.Storage.Streams.InputStreamOptions.None);
                }
                var hash = md5.HashData(buffer);
                var result = CryptographicBuffer.EncodeToHexString(hash);
                Model.Message = "搜索匹配";
                var matches = await dc.GetMatches(Path.GetFileNameWithoutExtension(file.Path), result, fileLength);
                await GetComments(matches.FirstOrDefault());
                mediaElement.SetPlaybackSource(MediaSource.CreateFromStorageFile(file));
            }
        }

        private async void AppBarButton_Click_1(object sender, RoutedEventArgs e)
        {
          
        }

        private async Task GetComments(Match m)
        {
            if (m == null)
                return;
            Model.Message = "获取弹幕……";
            Comments = (await dc.GetAllComments(m.EpisodeId)).OrderBy(c => c.Time).ToList();
            Model.Message = $"共 {Comments.Count} 条";
            CalcCommentsPosition();
        }

        private void CreateStoryboard(CommentBlock cb)
        {
            var sb = new Storyboard();
            var ani = new DoubleAnimation();
            ani.To = -cb.Size.Width;
            ani.Duration = new Duration(TimeSpan.FromSeconds(seconds));
            Storyboard.SetTargetProperty(ani, "(Canvas.Left)");
            Storyboard.SetTarget(ani, cb);
            sb.Children.Add(ani);
            sb.Completed += Storyboard_Completed;

            Storyboards.Add(sb);
            InScreenComments.Add(sb, cb);
            canvas.Children.Add(cb);

            sb.Begin();
        }

        private CommentBlock CreateCommentBlock(Comment c)
        {
            var cb = new CommentBlock(c);
            cb.FontSize = fontsize;
            cb.MeasureSize();
            cb.ShownTime = cb.Size.Width / (canvas.ActualWidth + cb.Size.Width) * seconds + cb.StartTime;

            var line = GetMinIndex(rowTimeLines);
            rowTimeLines[line] = cb.ShownTime;
            Canvas.SetLeft(cb, canvas.ActualWidth);
            Canvas.SetTop(cb, line * rowHeight);
            return cb;
        }

        private void Storyboard_Completed(object sender, object e)
        {
            var sb = sender as Storyboard;
            var c = InScreenComments[sb];

            InScreenComments.Remove(sb);
            Storyboards.Remove(sb);
            canvas.Children.Remove(c);
        }

        private void CalcCommentsPosition()
        {
            var height = canvas.ActualHeight * 0.8;
            var temp = new CommentBlock()
            {
                Text = "测量",
                FontSize = fontsize
            };
            var size = temp.MeasureSize();
            var rows = (int)(height / size.Height);
            rowHeight = size.Height;
            rowWidth = canvas.ActualWidth;
            rowTimeLines = new double[rows];
        }

        private int GetMinIndex(double[] array)
        {
            var minIndex = 0;
            var currentMin = double.PositiveInfinity;
            for (int i = 0; i < array.Length; i++)
            {
                if (array[i] < currentMin)
                {
                    currentMin = array[i];
                    minIndex = i;
                }
            }
            return minIndex;
        }

        private void ResetAll()
        {
            canvas.Children.Clear();
            InScreenComments.Clear();
            Storyboards.ForEach(sb => sb.Stop());
            Storyboards.Clear();
            commentIndex = 0;
            rowTimeLines = new double[rowTimeLines.Length];
        }
    }
}
