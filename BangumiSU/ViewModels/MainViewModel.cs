using BangumiSU.Models;
using BangumiSU.Pages;
using BangumiSU.Pages.Controls;
using BangumiSU.SharedCode;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;
using Windows.Storage.Search;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using static BangumiSU.SharedCode.AppCache;

namespace BangumiSU.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        public MainViewModel()
        {
            if (Trackings.IsEmpty())
                Refresh();
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
                var t = await TClient.UpdateProgress(SelectedTracking.Id, SelectedTracking.Progress + 1);
                Trackings.Replace(SelectedTracking, t);
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
                    await SelectedTracking.Folder.LaunchAsUri();
                else
                    await SelectedTracking.Uri.LaunchAsFile();
            }
        }

        public async Task OpenFolder()
        {
            if (SelectedTracking != null)
            {
                if (SelectedTracking.Online)
                    await SelectedTracking.Folder.LaunchAsUri();
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
            await url.LaunchAsUri();
        }

        public async void VisitBgm()
        {
            if (SelectedTracking?.Bangumi?.BangumiCode?.IsEmpty() == false)
            {
                var uri = "http://bangumi.tv/subject/" + SelectedTracking.Bangumi.BangumiCode;
                await uri.LaunchAsUri();
            }
        }

        public async void VisitHP()
        {
            if (SelectedTracking?.Bangumi?.HomePage?.IsEmpty() == false)
                await SelectedTracking.Bangumi.HomePage.LaunchAsUri();
        }

        public async Task Finish()
        {
            if (SelectedTracking != null)
            {
                await BClient.Finish(SelectedTracking.BangumiId);
                if (!SelectedTracking.Online)
                    await moveDirectory(SelectedTracking);
                Trackings.Remove(SelectedTracking);
            }
        }

        public async Task UpdateInfo()
        {
            if (SelectedTracking != null)
            {
                Message = "正在更新信息……";
                var bgm = await BClient.Update(SelectedTracking.Bangumi.Id);
                Trackings.Replace(SelectedTracking, bgm.Trackings.Single(t => t.Id == SelectedTracking.Id));
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

                var online = BangumiCache.SelectMany(b => b.Trackings).Where(a => !a.Finish && a.Online).ToList();
                if (online.Count > 0)
                {
                    foreach (var item in online)
                    {
                        var t = item;
                        if (updateOnline(item))
                            t = await TClient.Update(item);
                        list.Add(t);
                    }
                }

                list.AddRange(await onlineBgms());
                return new ObservableCollection<Tracking>(list.OrderByDescending(a => a.LastUpdate));
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                return null;
            }
        }

        private async Task GetLocalFiles(List<Tracking> list)
        {
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
                    }
                    if (t.Finish)
                        continue;
                    if (t.Folder != bgmDir.Path)
                        t.Folder = bgmDir.Path;

                    t.Count = -1;
                    var files = (await bgmDir.GetFilesAsync()).OrderBy(f => f.DateCreated);
                    double temp = 0;
                    var updateFlag = false;
                    foreach (var file in files)
                    {
                        var ext = getExt(file).ToUpper();
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
                    if (updateFlag)
                        t = await TClient.Update(t);
                    list.Add(t);
                }
            }
        }

        private Task<Tracking[]> onlineBgms()
        {
            var bgms = BangumiCache.Where(b => !b.Finish && b.Trackings.Count == 0 && !string.IsNullOrEmpty(b.OnlineLink));
            return Task.WhenAll(bgms.Select(b =>
            {
                var t = new Tracking()
                {
                    BangumiId = b.Id,
                    Online = true,
                    LastUpdate = b.OnAir,
                    Folder = b.OnlineLink,
                    Count = 1,
                    Progress = 0,
                };
                t.GetSubGroup(b.OnlineLink);
                updateOnline(t, b);
                return TClient.Create(t);
            }));
        }

        private bool updateOnline(Tracking t, Bangumi b = null)
        {
            b = b ?? t.Bangumi;
            var week = (int)((DateTime.Now - t.LastUpdate).TotalDays / 7);
            if (week < 1)
                return false;
            t.Count += week;
            if (b.Episodes > 0 && t.Count > b.Episodes)
                t.Count = b.Episodes;
            t.LastUpdate = t.LastUpdate.AddDays(7 * week);
            return true;
        }

        private void adjustTime(Tracking a)
        {
            var count = (int)((DateTimeOffset.Now - a.Bangumi.OnAir).TotalDays / 7);
            if (a.Count > count + 1)
                a.Count = count + 1;
            a.LastUpdate = a.Bangumi.OnAir.AddDays(7 * count);
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
                            var ext = getExt(f);
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

        private string getExt(StorageFile file)
        {
            var index = file.Name.LastIndexOf('.');
            return file.Name.Substring(index);
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
                    sb.Append("((.*)");
                    foreach (var s in keys)
                        sb.Append(s).Append("(.*)");
                    sb.Append(")|");
                }
            }
            var result = sb.ToString().TrimEnd('|');
            AppSettings.RssPattern = result;
        }
        #endregion
    }
}
