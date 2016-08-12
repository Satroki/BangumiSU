using BangumiSU.Models;
using BangumiSU.SharedCode;
using Microsoft.AspNetCore.JsonPatch;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BangumiSU.ApiClients
{
    public class BangumiClient : ApiClient
    {
        public BangumiClient() : base("Bangumi/") { }

        public async Task<Bangumi> CreateByCode(string bangumiCode)
            => await Post<Bangumi>(null, bangumiCode);

        public async Task<Bangumi> Update(long id)
            => await Put<Bangumi>(null, id.ToString());

        public async Task<Bangumi> Update(Bangumi bgm)
            => await Put(bgm);

        public async Task<Bangumi> Finish(long id)
        {
            var patch = new JsonPatchDocument<Bangumi>();
            patch.Replace(b => b.Finish, true);
            return await Patch(id, patch);
        }

        public async Task<Bangumi> GetBangumi(int id)
            => await Get<Bangumi>(id.ToString());

        public async Task<List<Bangumi>> GetBangumis()
            => await Get<List<Bangumi>>();

        public async Task<List<Bangumi>> GetUnfinished()
            => await Get<List<Bangumi>>($"{nameof(Bangumi.Finish)}/{false}");

        public async Task<List<Bangumi>> Search(string prop, string key)
            => await Get<List<Bangumi>>($"{prop}/{key}");

        protected override object AfterDeserialize(object o)
        {
            return (object)(o as Bangumi)?.InitTrackings()
                ?? (o as List<Bangumi>)?.InitTrackings();
        }

        protected override object BeforeSerialize(object o)
        {
            var b = o as Bangumi;
            if (b != null)
            {
                b = b.Copy();
                b.Trackings.Clear();
                return b;
            }
            return o;
        }
    }
}
