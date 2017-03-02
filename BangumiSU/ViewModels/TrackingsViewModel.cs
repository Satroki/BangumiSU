using BangumiSU.Models;
using BangumiSU.Pages;
using BangumiSU.Pages.Controls;
using BangumiSU.SharedCode;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;
using Windows.Storage.Search;
using Windows.UI.Xaml.Controls;
using static BangumiSU.SharedCode.AppCache;

namespace BangumiSU.ViewModels
{
    public class TrackingsViewModel : ViewModelBase
    {
        public TrackingsViewModel()
        {
#if DEBUG1
#else
            if (Trackings.IsEmpty())
                Refresh();
#endif
        }

        #region 属性
        public ObservableCollection<Tracking> Trackings
        {
            get { return CurrentTrackings; }
            set { CurrentTrackings = value; }
        }

        private Tracking _SelectedTracking;
        public Tracking SelectedTracking
        {
            get { return _SelectedTracking; }
            set { SetProperty(ref _SelectedTracking, value); }
        }
        #endregion

        #region 命令
        public async Task AddProgress()
        {
            if (SelectedTracking != null)
            {
                var temp = SelectedTracking;
                var t = await TClient.UpdateProgress(temp.Id, temp.Progress + 1);
                Trackings.Replace(temp, t);
                SelectedTracking = t;
            }
        }

        public async void Refresh()
        {
            try
            {
                Message = "扫描中……";
                BangumiCache = await BClient.GetUnfinished();
                Trackings = await CreateList();
            }
            finally
            {
                Message = "";
            }
        }

        public async Task Open()
        {
            if (SelectedTracking != null)
            {
                if (SelectedTracking.Online)
                    await SelectedTracking.Folder.LaunchToWeb();
                else
                {
                    if (AppSettings.UseInternalPlayer)
                        NavigationHelper.Navigate<VideoPage>(SelectedTracking);
                    else
                        await SelectedTracking.Uri.LaunchAsFile();
                }
            }
        }

        public async Task OpenFolder()
        {
            if (SelectedTracking != null)
            {
                if (SelectedTracking.Online)
                    await SelectedTracking.Folder.LaunchToWeb();
                else
                    await SelectedTracking.Folder.LaunchAsFolder();
            }
        }

        public async Task Edit()
        {
            if (SelectedTracking != null)
            {
                var d = new TrackingDialog(SelectedTracking, true);
                var r = await d.ShowAsync();
                if (r == ContentDialogResult.Primary)
                {
                    var t = d.Model.Tracking;
                    Trackings.Replace(SelectedTracking, t);
                    SelectedTracking = t;
                }
            }
        }

        public async Task Score()
        {
            var t = SelectedTracking;
            if (t != null)
            {
                var d = new ScoreDialog(t.Bangumi);
                var r = await d.ShowAsync();
                if (r == ContentDialogResult.Primary)
                {
                    var s = d.Model.Bangumi.Scores;
                    t.Bangumi.Scores = s;
                }
            }
        }

        public void CopyName()
        {
            if (SelectedTracking != null)
            {
                var data = new DataPackage();
                data.SetText(SelectedTracking.Bangumi.LocalName);
                Clipboard.SetContent(data);
            }
        }

        public async void Search(string key)
        {
            string url = AppSettings.DmhySearch + key?.Replace(' ', '+');
            await url.LaunchToWeb();
        }

        public async void VisitBgm()
        {
            if (SelectedTracking?.Bangumi?.BangumiCode?.IsEmpty() == false)
            {
                var uri = "http://bangumi.tv/subject/" + SelectedTracking.Bangumi.BangumiCode;
                await uri.LaunchToWeb();
            }
        }

        public async void VisitHP()
        {
            if (SelectedTracking?.Bangumi?.HomePage?.IsEmpty() == false)
                await SelectedTracking.Bangumi.HomePage.LaunchToWeb();
        }

        public async Task Finish()
        {
            if (SelectedTracking != null)
            {
                var temp = SelectedTracking;
                await BClient.Finish(temp.BangumiId);
                if (!temp.Online)
                    await moveDirectory(temp);
                Trackings.Remove(temp);
            }
        }

        public async Task UpdateInfo()
        {
            if (SelectedTracking != null)
            {
                var temp = SelectedTracking;
                Message = "正在更新信息……";
                var bgm = await BClient.Update(temp.Bangumi.Id);
                Trackings.Replace(temp, bgm.Trackings.Single(t => t.Id == temp.Id));
                Message = "";
            }
        }

        public void MusicInfo()
        {
            NavigationHelper.Navigate(typeof(MusicPage), Trackings.Select(a => a.Bangumi));
        }

        public void AdjustTime()
        {
            adjustTime(SelectedTracking);
        }
        #endregion

        #region 方法
        private async Task moveDirectory(Tracking t)
        {
            try
            {
                if (FinishFolder != null)
                {
                    var dest = await FinishFolder.CreateFolderAsync(t.Bangumi.Schedule, CreationCollisionOption.OpenIfExists);

                    var name = t.Folder.Substring(t.Folder.LastIndexOf('\\') + 1);
                    dest = await dest.CreateFolderAsync(name, CreationCollisionOption.OpenIfExists);

                    var video = (await VideoFolder.TryGetItemAsync(t.Folder.Substring(VideoFolder.Path.Length))) as StorageFolder;
                    foreach (var item in await video.GetFilesAsync())
                    {
                        await item.MoveAsync(dest);
                    }
                    await video.DeleteAsync();
                }
            }
            catch
            { throw; }
        }

        public async Task<ObservableCollection<Tracking>> CreateList()
        {
            try
            {
                var list = new List<Tracking>();

                await GetLocalFiles(list);

                SetRssPattern(list);

                var online = BangumiCache.SelectMany(b => b.Trackings).Where(t => !t.Finish && t.Online).ToList();

                list.AddRange(online);

                return list.OrderByDescending(a => a.LastUpdate).ToObservableCollection();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                Message = ex.Message;
                return null;
            }
        }

        private async Task GetLocalFiles(List<Tracking> list)
        {
            if (VideoFolder == null)
                return;

            var dirs = await VideoFolder.GetFoldersAsync(CommonFolderQuery.DefaultQuery);
            dirs = dirs.Where(d => Regex.IsMatch(d.Name, AppSettings.FolderFormat)).ToList();
            await sortFolder(dirs);
            string exts = AppSettings.Extensions;

            foreach (var dir in dirs)
            {
                var bgmDirs = await dir.GetFoldersAsync(CommonFolderQuery.DefaultQuery);
                foreach (var bgmDir in bgmDirs)
                {
                    var name = stringSplit(bgmDir.Name);
                    var idName = $"{dir.Name}-{name[1]}";
                    var t = BangumiCache.SelectMany(b => b.Trackings).FirstOrDefault(tck => tck.FileIdName == idName)
                        ?? (await TClient.GetByIdName(idName)).FirstOrDefault();

                    var createFlag = false;
                    if (t == null)
                    {
                        t = new Tracking()
                        {
                            SubGroup = name[0],
                            FileIdName = idName,
                            Folder = bgmDir.Path,
                        };
                        var dlg = new TrackingDialog(t);
                        var result = await dlg.ShowAsync();
                        if (result != ContentDialogResult.Primary)
                            continue;
                        t = dlg.Model.Tracking;
                        createFlag = true;
                    }
                    if (t.Finish)
                        continue;
                    t.Folder = bgmDir.Path;

                    t.Count = -1;
                    var files = (await bgmDir.GetFilesAsync()).OrderByDescending(f => f.DateCreated);
                    double temp = 0;
                    var updateFlag = false;
                    foreach (var file in files)
                    {
                        var ext = file.GetExt().ToUpper();
                        if (ext == ".TORRENT")
                        {
                            await file.DeleteAsync();
                            continue;
                        }
                        if (!exts.Contains(ext))
                            continue;
                        var info = stringSplit(file.Name);
                        if (info.Length < 3)
                            continue;
                        var macth = Regex.Match(info[2], @"\d{1,3}(\.5)?");
                        if (!macth.Success && info.Length > 3)
                            macth = Regex.Match(info[3], @"\d{1,3}(\.5)?");
                        if (macth.Success)
                        {
                            temp = double.Parse(macth.Value);
                            if (temp > t.Count)
                            {
                                t.Count = temp;
                                if (t.LastUpdate != file.DateCreated)
                                {
                                    t.LastUpdate = file.DateCreated;
                                    updateFlag = true;
                                }
                                t.Uri = file.Path;
                            }
                        }
                    }
                    if (createFlag)
                        t = await TClient.Create(t);
                    else if (updateFlag)
                        t = await TClient.Update(t);
                    list.Add(t);
                }
            }
        }

        private async void adjustTime(Tracking a)
        {
            var count = (int)((DateTimeOffset.Now - a.Bangumi.OnAir).TotalDays / 7);
            if (a.Count > count + 1)
                a.Count = count + 1;
            a.LastUpdate = a.Bangumi.OnAir.AddDays(7 * count);
            var na = await TClient.Update(a);
            Trackings.Replace(a, na);
        }

        private async Task sortFolder(IEnumerable<StorageFolder> dirs)
        {
            try
            {
                foreach (var dir in dirs)
                {
                    var files = await dir.GetFilesAsync();
                    foreach (var f in files)
                    {
                        try
                        {
                            var ext = f.GetExt();
                            if (ext == ".torrent")
                            {
                                await f.DeleteAsync();
                                continue;
                            }
                            if (ext == ".td")
                                continue;
                            string[] temp = stringSplit(f.Name);
                            StringBuilder sb = new StringBuilder();
                            string dirName = $"[{temp[0]}][{temp[1]}]";
                            var folder = (await dir.TryGetItemAsync(dirName)) as StorageFolder;
                            if (folder == null)
                                folder = await dir.CreateFolderAsync(dirName);
                            await f.MoveAsync(folder);
                        }
                        catch (Exception)
                        { }
                    }
                }
            }
            catch (Exception)
            { throw; }
        }

        private string[] stringSplit(string str)
        {
            var chars = new[] { '[', ']' };
            if (Regex.IsMatch(str, @"\[Mabors Sub\].* - \d*\["))
                chars = new[] { '[', ']', '-' };
            return str.Split(chars).Where(s => !string.IsNullOrWhiteSpace(s)).ToArray();
        }

        private void SetRssPattern(IEnumerable<Tracking> list)
        {
            if (list.IsEmpty())
                return;

            var sb = new StringBuilder();
            foreach (var a in list)
            {
                if (!string.IsNullOrEmpty(a.KeyWords))
                {
                    var keys = a.KeyWords.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    sb.Append("(.*");
                    foreach (var s in keys)
                        sb.Append(s).Append(".*");
                    sb.Append(")|");
                }
            }
            var result = sb.ToString().TrimEnd('|');
            AppSettings.RssPattern = result;
        }
        #endregion
    }
}
