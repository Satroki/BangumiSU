using PropertyChanged;

namespace BangumiSU.Models
{
    public class BangumiInfo : ModelBase
    {
        public string Url { get; set; }
        public string Code { get; set; }
        public string LocalName { get; set; }
        public string Name { get; set; }
        public string Info { get; set; }
        public BangumiInfoState State { get; set; } = BangumiInfoState.None;
        [DependsOn(nameof(State))]
        public string StateString
        {
            get
            {
                switch (State)
                {
                    case BangumiInfoState.None:
                        return "";
                    case BangumiInfoState.Loading:
                        return "……";
                    case BangumiInfoState.Added:
                        return "添加成功";
                    case BangumiInfoState.Extist:
                        return "已存在";
                    case BangumiInfoState.Error:
                        return "添加失败";
                    default: return "";
                }
            }
        }
    }

    public enum BangumiInfoState { None, Loading, Added, Extist, Error }
}
