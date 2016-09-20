using BangumiSU.ApiClients;
using BangumiSU.Controls;
using BangumiSU.Models;
using BangumiSU.SharedCode;
using BangumiSU.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Media.Core;
using Windows.Security.Cryptography;
using Windows.Security.Cryptography.Core;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.System.Display;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

namespace BangumiSU.Pages
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class VideoPage : Page, IContentPage
    {
        public VideoViewModel Model { get; set; } = new VideoViewModel();

        private DanDanClient DClient = new DanDanClient();
        private List<Comment> Comments = new List<Comment>();
        private List<Storyboard> Storyboards = new List<Storyboard>();
        private Dictionary<Storyboard, CommentBlock> InScreenComments = new Dictionary<Storyboard, CommentBlock>();
        private DispatcherTimer timer = new DispatcherTimer() { Interval = TimeSpan.FromSeconds(0.1) };

        private int commentIndex = 0;
        private uint bufferMD5 = 16 * 1024 * 1024;
        private double fontSize = AppCache.AppSettings.VideoSettings.FontSize;
        private double duration = AppCache.AppSettings.VideoSettings.Duration;
        private string filter = AppCache.AppSettings.VideoSettings.Filter;
        private double lastTick = 0;
        private double[] rowTimeLines = new double[0];
        private double[] rowTopTimeLines = new double[0];
        private double rowHeight = 0;
        private double rowWidth = 0;

        private DisplayRequest dr = new DisplayRequest();

        public VideoPage()
        {
            this.InitializeComponent();
            timer.Tick += Timer_Tick;
        }

        #region 基础
        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            Model.Tracking = e.Parameter as Tracking;
            var file = await Model.Tracking.Uri.AsFile();
            OpenFile(file);
        }

        private void Setting_Click(object sender, RoutedEventArgs e)
        {
            if (popupSetting.IsOpen)
            {
                popupSetting.IsOpen = false;
            }
            else
            {
                gridSetting.Height = canvas.ActualHeight;
                popupSetting.IsOpen = true;
            }
        }

        private void ApplySettings_Click(object sender, RoutedEventArgs e)
        {
            ApplySettings();
            popupSetting.IsOpen = false;
        }

        private void ApplySettings()
        {
            fontSize = AppCache.AppSettings.VideoSettings.FontSize;
            duration = AppCache.AppSettings.VideoSettings.Duration;
            filter = AppCache.AppSettings.VideoSettings.Filter;
            CalcCommentsPosition();
        }

        public void Arrived()
        {
        }

        public void Leaved()
        {
            ResetAll();
            mediaElement.Stop();
        }
        #endregion

        #region 播放控制
        private async void OpenFile_Click(object sender, RoutedEventArgs e)
        {
            var fp = new FileOpenPicker();
            fp.FileTypeFilter.Add("*");
            fp.SuggestedStartLocation = PickerLocationId.ComputerFolder;
            var file = await fp.PickSingleFileAsync();
            if (file != null)
            {
                OpenFile(file);
            }
        }

        private void OpenFile(StorageFile file)
        {
            mediaElement.SetPlaybackSource(MediaSource.CreateFromStorageFile(file));
            SearchComments(file);
        }

        private void RequestRelease()
        {
            try
            {
                dr.RequestRelease();
            }
            catch { }
        }

        private void MediaElement_CurrentStateChanged(object sender, RoutedEventArgs e)
        {
            switch (mediaElement.CurrentState)
            {
                case MediaElementState.Closed:
                    RequestRelease();
                    break;
                case MediaElementState.Opening:
                    break;
                case MediaElementState.Buffering:
                    break;
                case MediaElementState.Playing:
                    timer.Start();
                    Storyboards.ForEach(sb => sb.Resume());
                    dr.RequestActive();
                    break;
                case MediaElementState.Paused:
                    timer.Stop();
                    Storyboards.ForEach(sb => sb.Pause());
                    RequestRelease();
                    break;
                case MediaElementState.Stopped:
                    ResetAll();
                    RequestRelease();
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

        private void Canvas_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            CalcCommentsPosition();
        }
        #endregion

        #region 弹幕载入
        private async void SearchComments(StorageFile file)
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

            Model.FileHash = result;
            Model.FileSize = fileLength;
            Model.FileName = Path.GetFileNameWithoutExtension(file.Path);

            await GetMatches();
        }

        private async Task GetMatches()
        {
            if (Model.FileHash.IsEmpty())
                return;
            Model.Message = "搜索匹配……";
            Model.Matches = await DClient.GetMatches(Model.FileName, Model.FileHash, Model.FileSize);
            await GetComments(Model.Matches.FirstOrDefault());
        }

        private async Task GetComments(Match m)
        {
            if (m == null)
                return;
            try
            {
                Model.Message = "获取弹幕……";
                Comments = (await DClient.GetAllComments(m.EpisodeId)).OrderBy(c => c.Time).ToList();
                CalcCommentsPosition();
                Model.Message = $"共 {Comments.Count} 条";
            }
            catch
            {
                Model.Message = $"获取失败";
            }
            finally
            {
                mediaElement.Play();
            }
        }

        private async void RefreshCommemts_Click(object sender, RoutedEventArgs e)
        {
            var m = (sender as Button).DataContext as Match;
            await GetComments(m);
        }

        private async void SearchAnime_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            if (!sender.Text.IsEmpty())
                Model.SearchResult = await DClient.SearchAnime(sender.Text);
        }

        private void AddComments_Click(object sender, RoutedEventArgs e)
        {
            if (popupSearch.IsOpen)
            {
                popupSearch.IsOpen = false;
            }
            else
            {
                gridSearch.Height = canvas.ActualHeight;
                if (Model.Tracking != null)
                    searchBox.Text = $"{Model.Tracking.Bangumi.LocalName} {Model.Tracking.Count}";
                popupSearch.IsOpen = true;
            }
        }

        private async void AddSource_Click(object sender, RoutedEventArgs e)
        {
            var r = (sender as Button).DataContext as SearchResult;
            Model.Message = "添加弹幕……";
            try
            {
                var temp = await DClient.GetCommentsByAnime(r);
                if (!temp.IsEmpty())
                    Comments.AddRange(temp);
            }
            finally
            {
                Model.Message = $"共 {Comments.Count} 条";
            }
        }

        private async void SearchAnime_Click(object sender, RoutedEventArgs e)
        {
            if (!txtTitle.Text.IsEmpty())
                Model.Animes = await DClient.Search(txtTitle.Text, txtEp.Text);
        }

        private async void ApplyAnime_Click(object sender, RoutedEventArgs e)
        {
            if (Model.SelectedEpisode != null)
            {
                try
                {
                    Model.Message = "获取弹幕……";
                    Comments = await DClient.GetAllComments(Model.SelectedEpisode.Id);
                }
                finally
                {
                    Model.Message = $"共 {Comments.Count} 条";
                }
            }
        }

        private void RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            var ep = (sender as RadioButton).DataContext as Episode;
            Model.SelectedEpisode = ep;
        }

        private void SelectAnime_Click(object sender, RoutedEventArgs e)
        {
            if (popupSearchAnime.IsOpen)
            {
                popupSearchAnime.IsOpen = false;
            }
            else
            {
                gridSearchAnime.Height = canvas.ActualHeight;
                if (Model.Tracking != null)
                {
                    txtTitle.Text = Model.Tracking.Bangumi.LocalName;
                    txtEp.Text = Model.Tracking.Count.ToString();
                }
                popupSearchAnime.IsOpen = true;
            }
        }
        #endregion

        #region 弹幕控制
        private void CalcCommentsPosition()
        {
            var height = canvas.ActualHeight * 0.8;
            var temp = new CommentBlock()
            {
                Text = "测量",
                FontSize = fontSize
            };
            var size = temp.MeasureSize();
            var rows = (int)(height / size.Height);
            rowHeight = size.Height;
            rowWidth = canvas.ActualWidth;
            rowTimeLines = new double[rows];
            rowTopTimeLines = new double[rows];
        }

        private void Timer_Tick(object sender, object e)
        {
            var ts = mediaElement.Position.TotalSeconds - Model.Offset;
            if (ts < lastTick)
            {
                ResetAll();
            }
            lastTick = ts;
            var index = Comments.FindIndex(c => c.Time >= lastTick - 0.5);
            commentIndex = Math.Max(commentIndex, index);
            for (; commentIndex < Comments.Count; commentIndex++)
            {
                var c = Comments[commentIndex];
                if (c.Time < lastTick + 0.3)
                {
                    if ((c.Mode == Mode.Top && Model.ShowTop != true) || (c.Mode != Mode.Top && Model.ShowNormal != true))
                        continue;
                    if (!filter.IsEmpty() && System.Text.RegularExpressions.Regex.IsMatch(c.Message, filter, System.Text.RegularExpressions.RegexOptions.IgnoreCase))
                        continue;
                    var cb = CreateCommentBlock(c);
                    CreateStoryboard(cb);
                }
                else
                {
                    break;
                }
            }
        }

        private CommentBlock CreateCommentBlock(Comment c)
        {
            var cb = new CommentBlock(c);
            cb.FontSize = fontSize;
            cb.MeasureSize();
            if (c.Mode == Mode.Top)
            {
                cb.ShownTime = duration * 0.5 + cb.StartTime;
                var line = GetMinIndex(rowTopTimeLines, cb.StartTime);
                rowTopTimeLines[line] = cb.ShownTime;
                Canvas.SetLeft(cb, canvas.ActualWidth / 2 - cb.Size.Width / 2);
                Canvas.SetTop(cb, line * rowHeight);
                Canvas.SetZIndex(cb, 1);
            }
            else
            {
                cb.ShownTime = (rowWidth * 0.25 + cb.Size.Width) / (canvas.ActualWidth + cb.Size.Width) * duration + cb.StartTime;
                var line = GetMinIndex(rowTimeLines, cb.StartTime);
                rowTimeLines[line] = cb.ShownTime;
                Canvas.SetLeft(cb, canvas.ActualWidth);
                Canvas.SetTop(cb, line * rowHeight);
            }
            return cb;
        }

        private void CreateStoryboard(CommentBlock cb)
        {
            var sb = new Storyboard();
            var ani = new DoubleAnimation();
            if (cb.Mode == Mode.Top)
            {
                ani.To = Canvas.GetLeft(cb);
                ani.Duration = new Duration(TimeSpan.FromSeconds(duration * 0.5));
            }
            else
            {
                ani.To = -cb.Size.Width;
                ani.Duration = new Duration(TimeSpan.FromSeconds(duration));
            }
            Storyboard.SetTargetProperty(ani, "(Canvas.Left)");
            Storyboard.SetTarget(ani, cb);
            sb.Children.Add(ani);
            sb.Completed += Storyboard_Completed;

            Storyboards.Add(sb);
            InScreenComments.Add(sb, cb);
            canvas.Children.Add(cb);

            sb.Begin();
        }

        private void Storyboard_Completed(object sender, object e)
        {
            var sb = sender as Storyboard;
            var c = InScreenComments[sb];

            InScreenComments.Remove(sb);
            Storyboards.Remove(sb);
            canvas.Children.Remove(c);
        }

        private int GetMinIndex(double[] array, double start)
        {
            var minIndex = 0;
            var currentMin = double.PositiveInfinity;
            for (int i = 0; i < array.Length; i++)
            {
                if (array[i] < start)
                    return i;
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
            rowTopTimeLines = new double[rowTopTimeLines.Length];
        }

        private void Add1_Click(object sender, RoutedEventArgs e)
        {
            Model.Offset = Model.Offset + 1;
        }

        private void Add5_Click(object sender, RoutedEventArgs e)
        {
            Model.Offset = Model.Offset + 5;
        }

        private void Subtract1_Click(object sender, RoutedEventArgs e)
        {
            Model.Offset = Model.Offset - 1;
        }

        private void Subtract5_Click(object sender, RoutedEventArgs e)
        {
            Model.Offset = Model.Offset - 5;
        }

        private void ResetOffset_Click(object sender, RoutedEventArgs e)
        {
            Model.Offset = 0;
        }
        #endregion

        private void txtTitle_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                SearchAnime_Click(null, null);
            }
        }
    }
}
