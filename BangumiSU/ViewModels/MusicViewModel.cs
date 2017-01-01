using BangumiSU.Models;
using BangumiSU.SharedCode;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System;

namespace BangumiSU.ViewModels
{
    public class MusicViewModel : ViewModelBase
    {
        public MusicViewModel(IEnumerable<Bangumi> list)
        {
            Bangumis = new ObservableCollection<Bangumi>(list);
            foreach (var item in Bangumis)
                item.PropertyChanged += Item_PropertyChanged;
        }

        public ObservableCollection<Bangumi> Bangumis { get; set; }
        private List<Bangumi> changedList = new List<Bangumi>();

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
    }
}
