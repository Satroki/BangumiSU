using BangumiSU.Models;
using BangumiSU.SharedCode;
using Microsoft.AspNetCore.JsonPatch;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BangumiSU.ApiClients
{
    public class TrackingClient : ApiClient
    {
        public TrackingClient() : base(AppCache.ApiUrl + "Tracking") { }

        public async Task<Tracking> Create(Tracking t)
            => await Post<Tracking>(t);

        public async Task<Tracking> Update(Tracking t)
            => await Put(t);

        public async Task<Tracking> UpdateProgress(long id, int progress)
        {
            var patch = new JsonPatchDocument<Tracking>();
            patch.Replace(t => t.Progress, progress);
            return await Patch(id, patch);
        }

        public async Task<Tracking> Finish(long id)
        {
            var patch = new JsonPatchDocument<Tracking>();
            patch.Replace(b => b.Finish, true);
            return await Patch(id, patch);
        }

        public async Task<Tracking> GetById(long id)
            => await Get<Tracking>(id.ToString());

        public async Task<List<Tracking>> GetByBangumiId(long id)
            => await Get<List<Tracking>>($"{nameof(Tracking.BangumiId)}/{id}");

        public async Task<List<Tracking>> GetByIdName(string idName)
            => await Get<List<Tracking>>($"{nameof(Tracking.FileIdName)}/{idName}");

        protected override object AfterDeserialize(object o)
        {
            return (o as Tracking)?.InitBangumi()
                ?? (o as List<Tracking>)?.InitBangumi()
                ?? o;
        }

        protected override object BeforeSerialize(object o)
        {
            var t = o as Tracking;
            if (t != null)
            {
                t = t.Copy();
                t.Bangumi = null;
                return t;
            }
            return o;
        }
    }
}
