using BangumiSU.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BangumiSU.ApiClients
{
    class DanDanClient : ApiClient
    {
        private const string ApiUrl = @"http://acplay.net/api/v1/";
        public DanDanClient() : base("")
        {
            SetApiUrl(ApiUrl);
        }

        #region Comment
        //comment/{episodeId}?from={from}
        public async Task<List<Comment>> GetComments(int episodeId)
            => (await Get<TempObject>($"comment/{episodeId}")).Comments;
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
        public async Task<List<Anime>> Search(string name, double episode)
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
