using BangumiSU.Models;
using BangumiSU.Providers;
using BangumiSU.SharedCode;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;

namespace BangumiSU.ApiClients
{
    class DanDanClient : ApiClient
    {
        private const string ApiUrl = @"https://api.acplay.net/api/v2/";
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
        public async Task<List<Comment>> GetComments(int episodeId, bool withRelated = false)
        {
            var r = await Get<DanDanResult>($"comment/{episodeId}?withRelated={withRelated}");
            var list = r.Comments;
            list.ForEach(c => c.Parse());
            list.Distinct(new CommentEqualityComparer());
            return list.OrderBy(c => c.Time).ToList();
        }
            //=> (await Get<DanDanResult>()).Comments;

        //public async Task<List<Comment>> GetAllComments(int episodeId)
        //{
        //    var list = new List<Comment>();
        //    var dd = await GetComments(episodeId, true);
        //    list.AddRange(dd);

        //    var related = await GetRelateds(episodeId);
        //    CommentProvider.IdList.Clear();
        //    foreach (var r in related)
        //    {
        //        var pr = Providers.FirstOrDefault(p => p.Name == r.Provider);
        //        if (pr == null)
        //            continue;
        //        IEnumerable<Comment> temp = await pr.GetComments(r.Url);
        //        if (!temp.IsEmpty())
        //            list.AddRange(temp);
        //    }
        //    return list.Distinct(new CommentEqualityComparer()).OrderBy(c => c.Time).ToList();
        //}

        public async Task<List<SearchResult>> SearchAnime(string key)
        {
            var list = new List<SearchResult>();
            foreach (var p in Providers)
            {
                try
                {
                    var temp = await p.Search(key);
                    if (temp != null)
                        list.AddRange(temp);
                }
                catch { }
            }
            return list;
        }

        public async Task<List<Comment>> GetCommentsByAnime(SearchResult anime)
        {
            return await Providers.FirstOrDefault(p => p.Name == anime.Provider)?.GetComments(anime.Uri);
        }
        #endregion

        #region Match
        public async Task<List<Match>> GetMatches(string fileName, string hash, long length, int duration = 0, int force = 0)
        {
            var mr = new MatchRequest
            {
                FileHash = hash,
                FileName = fileName,
                FileSize = length,
                VideoDuration = duration,
                MatchMode = "hashAndFileName"
            };
            var r = await Post<DanDanResult>(mr, "match");
            return r.Matches;
        }
        #endregion

        #region Related
        //related/{episodeId}
        public async Task<List<Related>> GetRelateds(int episodeId)
        {
            var r = await Get<DanDanResult>($"related/{episodeId}");
            return r.Relateds;
        }
        #endregion

        #region SearchAll
        //searchall/{anime}/{episode}
        public async Task<List<Anime>> Search(string name, string episode)
        {
            var r = await Get<DanDanResult>($"search/episodes?anime={name}&episode={episode}");
            return r.Animes;
        }
        #endregion



        private class CommentEqualityComparer : IEqualityComparer<Comment>
        {
            public bool Equals(Comment x, Comment y)
            {
                return string.Equals(x.Message, y.Message) && Math.Abs(x.Time - y.Time) < 0.01;
            }

            public int GetHashCode(Comment obj)
            {
                return $"{obj.Time:0.00}{obj.Message}".GetHashCode();
            }
        }
    }
}
