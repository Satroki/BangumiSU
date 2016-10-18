using BangumiSU.Models;
using BangumiSU.Providers;
using BangumiSU.SharedCode;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BangumiSU.ApiClients
{
    class DanDanClient : ApiClient
    {
        private const string ApiUrl = @"http://acplay.net/api/v1/";
        public DanDanClient() : base(ApiUrl)
        {
            Providers = new List<CommentProvider>
            {
                new BiliBiliProvider(),
                new TucaoProvider()
            };
        }

        private List<CommentProvider> Providers;

        #region Comment
        //comment/{episodeId}?from={from}
        public async Task<List<Comment>> GetComments(int episodeId)
            => (await Get<TempObject>($"comment/{episodeId}")).Comments;

        public async Task<List<Comment>> GetAllComments(int episodeId)
        {
            var list = new List<Comment>();
            var dd = await GetComments(episodeId);
            list.AddRange(dd);

            var related = await GetRelateds(episodeId);
            CommentProvider.IdList.Clear();
            foreach (var r in related)
            {
                var pr = Providers.FirstOrDefault(p => p.Name == r.Provider);
                if (pr == null)
                    continue;
                IEnumerable<Comment> temp = await pr.GetComments(r.Url);
                if (!temp.IsEmpty())
                    list.AddRange(temp);
            }
            return list;
        }

        public async Task<List<SearchResult>> SearchAnime(string key)
        {
            var list = new List<SearchResult>();
            foreach (var p in Providers)
            {
                var temp = await p.Search(key);
                if (temp != null)
                    list.AddRange(temp);
            }
            return list;
        }

        public async Task<List<Comment>> GetCommentsByAnime(SearchResult anime)
        {
            return await Providers.FirstOrDefault(p => p.Name == anime.Provider)?.GetComments(anime.Uri);
        }
        #endregion

        #region Match
        //match?fileName={fileName}&hash={hash}&length={length}&duration={duration}&force={force}
        public async Task<List<Match>> GetMatches(string fileName, string hash, long length, int duration = 0, int force = 0)
            => (await Get<TempObject>($"match?fileName={fileName}&hash={hash}&length={length}&duration={duration}&force={force}")).Matches;
        #endregion

        #region Related
        //related/{episodeId}
        public async Task<List<Related>> GetRelateds(int episodeId)
            => (await Get<TempObject>($"related/{episodeId}")).Relateds;
        #endregion

        #region SearchAll
        //searchall/{anime}/{episode}
        public async Task<List<Anime>> Search(string name, string episode)
            => (await Get<TempObject>($"searchall/{name}/{episode}")).Animes;
        #endregion

        private class TempObject
        {
            public List<Comment> Comments { get; set; }

            public List<Match> Matches { get; set; }

            public List<Related> Relateds { get; set; }

            public List<Anime> Animes { get; set; }

            public bool HasMore { get; set; }
        }
    }
}
