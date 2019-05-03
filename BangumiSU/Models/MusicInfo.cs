using Newtonsoft.Json;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BangumiSU.Models
{
    public class MusicInfo : ModelBase
    {
        public long Id { get; set; }
        public int BangumiId { get; set; }
        public string Name { get; set; }

        public MusicType Type { get; set; }

        public MusicStorageLevel StorageLevel { get; set; }
        public string Note { get; set; }

        private static MusicStorageLevel[] storageLevels = new[] {
            MusicStorageLevel.None, MusicStorageLevel.MP3,
            MusicStorageLevel.CD, MusicStorageLevel.HiRes };

        [JsonIgnore]
        public MusicStorageLevel[] MusicStorageLevels => storageLevels;

        public MusicInfo()
        {
            PropertyChanged += MusicInfo_PropertyChanged;
        }

        private void MusicInfo_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(StorageLevel):
                case nameof(Note):
                    HasChanged = true;
                    break;
            }
        }
    }

    public enum MusicType
    {
        [Display(Name = "片头曲")]
        OP,
        [Display(Name = "片尾曲")]
        ED,
        [Display(Name = "角色歌")]
        CS,
        [Display(Name = "主题曲")]
        TM
    }

    public enum MusicStorageLevel
    {
        [Display(Name = "无")]
        None,
        [Display(Name = "320K/MP3")]
        MP3,
        [Display(Name = "44.1K/CD")]
        CD,
        [Display(Name = "Hi-Res")]
        HiRes,
    }
}
