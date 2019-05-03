using BangumiSU.Models;
using BangumiSU.SharedCode;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System;
using Windows.Storage;
using Windows.System;
using System.Linq;

namespace BangumiSU.ViewModels
{
    public class MusicViewModel : ViewModelBase
    {
        public MusicViewModel()
        {
            Bangumis = new ObservableCollection<Bangumi>();
        }

        public MusicViewModel(IEnumerable<Bangumi> list)
        {
            Bangumis = new ObservableCollection<Bangumi>(list);
            foreach (var item in Bangumis)
                item.PropertyChanged += Item_PropertyChanged;
            LevelDict = Extensions.GetEnumDict<MusicStorageLevel>();
        }

        public ObservableCollection<Bangumi> Bangumis { get; set; }

        private List<Bangumi> changedList = new List<Bangumi>();

        public Dictionary<string, MusicStorageLevel> LevelDict { get; set; }

        public async Task LoadInfos()
        {
            var ids = Bangumis.Select(b => b.Id).ToArray();
            var mis = await AppCache.MIClient.GetMusicInfos(ids);
            var dict = mis.ToLookup(m => m.BangumiId, m => m);
            foreach (var b in Bangumis)
            {
                if (dict.Contains(b.Id))
                    b.MusicInfos = dict[b.Id].ToObservableCollection();
                else
                    b.MusicInfos = new ObservableCollection<MusicInfo>();
            }
        }

        public async Task UpdateBangumiMusic(Bangumi bgm)
        {
            await LoadingTask(async () =>
            {
                if (bgm != null)
                {
                    foreach (var m in bgm.MusicInfos)
                    {
                        if (m.HasChanged)
                        {
                            var r = await AppCache.MIClient.Update(m);
                            m.HasChanged = r == null;
                        }
                    }
                }
            });
        }

        public async Task SyncBangumiMusic(Bangumi b)
        {
            await LoadingTask(async () =>
            {
                if (b != null)
                {
                    var mis = await AppCache.MIClient.SyncMusicInfo(b.Id);
                    b.MusicInfos = mis.ToObservableCollection();
                }
            });
        }

        public async Task Save()
        {
            await LoadingTask(Update());
        }

        private async Task Update()
        {
            foreach (var b in changedList)
                await AppCache.BClient.Update(b);
            changedList.Clear();
        }

        public void Clear()
        {
            foreach (var item in Bangumis)
                item.PropertyChanged -= Item_PropertyChanged;
        }

        private void Item_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            var b = (Bangumi)sender;
            if (changedList.Contains(b))
                return;
            else
                changedList.Add(b);
        }

        public async void OpenFolder(string name)
        {
            var folder = AppCache.MusicFolder;
            if (folder == null)
                return;
            var f = await folder.TryGetItemAsync(name) as StorageFolder;
            if (f == null)
                f = await folder.CreateFolderAsync(name);
            await Launcher.LaunchFolderAsync(f);
        }
    }
}
