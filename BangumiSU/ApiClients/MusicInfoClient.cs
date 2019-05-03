using BangumiSU.Models;
using BangumiSU.SharedCode;
using Microsoft.AspNetCore.JsonPatch;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BangumiSU.ApiClients
{
    public class MusicInfoClient : ApiClient
    {
        public MusicInfoClient() : base(AppCache.ApiUrl + "MusicInfo") { }

        public async Task<List<MusicInfo>> GetMusicInfo(int bid)
            => await Get<List<MusicInfo>>($"GetByBangumiId/{bid}");

        public async Task<List<MusicInfo>> SyncMusicInfo(int bid)
            => await Get<List<MusicInfo>>($"SyncByBangumiIds/{bid}");

        public async Task<List<MusicInfo>> GetMusicInfos(IEnumerable<int> bids)
        {
            return await Post<List<MusicInfo>>(bids, "GetByBangumiIds");
        }

        public async Task<MusicInfo> Update(MusicInfo mi)
        {
            var patch = new JsonPatchDocument<MusicInfo>();
            patch.Replace(m => m.StorageLevel, mi.StorageLevel);
            patch.Replace(m => m.Note, mi.Note);
            return await Patch($"Patch/{mi.Id}", patch);
        }
    }
}
