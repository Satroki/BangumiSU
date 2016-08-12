using BangumiSU.Models;
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
using Windows.UI.Xaml;
using static BangumiSU.SharedCode.AppCache;

namespace BangumiSU.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        public MainViewModel()
        {
        }

        #region 属性
        public ObservableCollection<Tracking> Trackings { get; set; }

        private Tracking _SelectedTracking;
        public Tracking SelectedTracking
        {
            get { return _SelectedTracking; }
            set { SetProperty(ref _SelectedTracking, value); KeyWord = value?.KeyWords; }
        }

        public string KeyWord { get; set; }

        public string Info { get; set; }
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

        public async Task ScanFiles()
        {
            try
            {
                Info = "扫描中……";
                BangumiCache = await BClient.GetUnfinished();
                Trackings = await CreateList();
            }
            finally
            {
                Info = "";
            }
        }

        public void OpenFile()
        {
            if (SelectedTracking != null)
            {
                //Process.Start(SelectedTracking.Uri ?? SelectedTracking.Folder);
            }
        }

        public void LocateFile()
        {
            if (SelectedTracking != null && !SelectedTracking.Online)
            {
                //ProcessStartInfo psi = new ProcessStartInfo("Explorer.exe");
                //psi.Arguments = " /select," + SelectedTracking.Uri;
                //Process.Start(psi);
            }
        }

        public void Edit()
        {
            if (SelectedTracking != null)
            {
                //var vm = new TrackingViewModel();
                //vm.Tracking = SelectedTracking;
                //vm.EditMode = true;
                //var window = new Views.TrackingView(vm);
                //window.Owner = win;
                //window.ShowDialog();
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

        public void Search()
        {
            string url = AppSettings.DmhySearch + KeyWord?.Replace(' ', '+');
           // Process.Start(url);
        }

        public void ShowDetails()
        {
            if (SelectedTracking != null)
            {
                var vm = new ManageViewModel(SelectedTracking.Bangumi);
                //var win = new Views.ManageView(vm);
                //win.Show();
            }
        }

        public void VisitBgm()
        {
            //if (SelectedTracking?.Bangumi?.BangumiCode?.IsEmpty() == false)
            //    Process.Start("http://bangumi.tv/subject/" + SelectedTracking.Bangumi.BangumiCode);
        }

        public void VisitHP()
        {
            //if (SelectedTracking?.Bangumi?.HomePage?.IsEmpty() == false)
            //    Process.Start(SelectedTracking.Bangumi.HomePage);
        }

        public async Task Update()
        {
            var vm = new UpdateViewModel();
            //var win = new Views.UpdateView(vm);
            //win.Show();
            //await vm.Update();
        }

        public async Task Finish()
        {
            if (SelectedTracking != null)
            {
                await BClient.Finish(SelectedTracking.BangumiId);
                if (!SelectedTracking.Online)
                    moveDirectory(SelectedTracking);
                Trackings.Remove(SelectedTracking);
            }
        }

        public async void UpdateInfo()
        {
            if (SelectedTracking != null)
            {
                Info = "正在更新信息……";
                var bgm = await BClient.Update(SelectedTracking.Bangumi.Id);
                Trackings.Replace(SelectedTracking, bgm.Trackings.Single(t => t.Id == SelectedTracking.Id));
                Info = "";
            }
        }

        public void MusicInfo()
        {
            var vm = new MusicViewModel(Trackings.Select(a => a.Bangumi));
            //var win = new Views.MusicView(vm);
            //win.Show();
        }

        public void AdjustTime()
        {
            adjustTime(SelectedTracking);
        }
        #endregion

        #region 方法
        private void moveDirectory(Tracking t)
        {
            try
            {
                //var path = Path.Combine(Settings.Default.FinishFolder, t.Bangumi.Schedule);
                //if (!Directory.Exists(path))
                //    Directory.CreateDirectory(path);
                //var di = new DirectoryInfo(t.Folder);
                //di.MoveTo(Path.Combine(path, di.Name));
            }
            catch
            { throw; }
        }

        public async Task<ObservableCollection<Tracking>> CreateList()
        {
            try
            {
                string[] strs = AppSettings.FolderFormat.Split('|');
                var dirs = (new DirectoryInfo(strs[0])).GetDirectories().Where(
                    d => Regex.IsMatch(d.Name, strs[1])).ToArray();
                sortFolder(dirs);
                string exts =AppSettings.Extensions;
                var list = new List<Tracking>();
                foreach (var dir in dirs)
                {
                    var bgmDirs = dir.GetDirectories();
                    foreach (var di in bgmDirs)
                    {
                        var name = stringSplit(di.Name);
                        var idName = $"{dir}-{name[1]}";
                        var t = BangumiCache.SelectMany(b => b.Trackings).FirstOrDefault(tck => tck.FileIdName == idName)
                            ?? (await TClient.GetByIdName(idName)).FirstOrDefault();
                        if (t == null)
                        {
                            t = new Tracking()
                            {
                                SubGroup = name[0],
                                FileIdName = idName,
                                Uri = di.FullName
                            };
                            //var vm = new TrackingViewModel()
                            //{
                            //    Tracking = t,
                            //    EditMode = false
                            //};
                            var result = false;
                            //Application.Current.Dispatcher.Invoke(() =>
                            //{
                            //    var view = new Views.TrackingView(vm);
                            //    result = view.ShowDialog() == true;
                            //});
                            if (!result)
                                continue;
                        }
                        if (t.Finish)
                            continue;
                        if (t.Folder != di.FullName)
                            t.Folder = di.FullName;

                        t.Count = -1;
                        var files = di.GetFiles().OrderBy(f => f.CreationTime);
                        double temp = 0;
                        foreach (var file in files)
                        {
                            if (file.Extension == ".torrent")
                            {
                                file.Delete();
                                continue;
                            }
                            if (!exts.Contains(file.Extension.ToLower()))
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
                                    t.LastUpdate = file.CreationTime;
                                    t.Uri = file.FullName;
                                }
                            }
                        }
                        list.Add(t);
                    }
                }

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
               // Console.WriteLine(ex);
                return null;
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

        private void sortFolder(DirectoryInfo[] dirs)
        {
            try
            {
                foreach (var dir in dirs)
                {
                    var files = dir.GetFiles();
                    foreach (var f in files)
                    {
                        try
                        {
                            var ext = f.Extension;
                            if (ext == ".torrent")
                            {
                                File.Delete(f.FullName);
                                continue;
                            }
                            if (ext == ".td")
                                continue;
                            string[] temp = stringSplit(f.Name);
                            StringBuilder sb = new StringBuilder();
                            sb.Append(dir.FullName).Append("\\[").Append(temp[0]).Append("][").Append(temp[1]).Append(']');
                            string dirName = sb.ToString();
                            if (!Directory.Exists(dirName))
                                Directory.CreateDirectory(dirName);
                            f.MoveTo(Path.Combine(dirName, f.Name));
                        }
                        catch (IOException)
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
        #endregion
    }
}
