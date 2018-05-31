using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BangumiSU.Models
{
    public class JWToken
    {
        [JsonProperty("access_token")]
        public string AccessToken { get; set; }

        /// <summary>
        /// 过期时间:秒
        /// </summary>
        [JsonProperty("expires_in")]
        public int ExpiresIn { get; set; }
        [JsonProperty("refresh_token")]
        public string RefreshToken { get; set; }
        [JsonProperty("token_type")]
        public string TokenType { get; set; }
        public DateTimeOffset ExpireDate { get; set; }
    }
}
