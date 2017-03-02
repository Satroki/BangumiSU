using Newtonsoft.Json;
using System.Collections.Generic;
using System.Reflection;

namespace BangumiSU.Models
{
    using Dict = Dictionary<string, int>;
    public class BangumiScore : ModelBase
    {
        private static PropertyInfo[] props = typeof(BangumiScore).GetProperties();
        public BangumiScore()
        {

        }

        public BangumiScore(Dict dict)
        {
            foreach (var p in props)
                if (dict.TryGetValue(p.Name, out int value))
                    p.SetValue(this, value);
        }

        public Dict ToDict()
        {
            var dict = new Dict();
            foreach (var p in props)
                dict.Add(p.Name, (int)p.GetValue(this));
            return dict;
        }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }

        public int 人设 { get; set; }
        public int 人物塑造 { get; set; }
        public int 配音 { get; set; }
        public int 音乐 { get; set; }
        public int 作画 { get; set; }
        public int 演出 { get; set; }
        public int 剧情 { get; set; }
        public int 信仰分 { get; set; }
    }
}
