namespace BangumiSU.Models
{
    public class Image : ModelBase
    {
        public int Id { get; set; }

        private string _Uri;
        public string Uri
        {
            get { return _Uri; }
            set { SetProperty(ref _Uri, value); }
        }

        private byte[] _Data;
        public byte[] Data
        {
            get { return _Data; }
            set { SetProperty(ref _Data, value); }
        }
    }
}
