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

        [JsonProperty("人设")]
        public int RS { get; set; }
        [JsonProperty("人物塑造")]
        public int RWSZ { get; set; }
        [JsonProperty("配音")]
        public int PY { get; set; }
        [JsonProperty("音乐")]
        public int YY { get; set; }
        [JsonProperty("作画")]
        public int ZH { get; set; }
        [JsonProperty("演出")]
        public int YC { get; set; }
        [JsonProperty("剧情")]
        public int JQ { get; set; }
        [JsonProperty("信仰分")]
        public int XYF { get; set; }
    }
}
