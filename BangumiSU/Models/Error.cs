using System;

namespace BangumiSU.Models
{
    public class Error
    {
        public string Message { get; set; }

        public Error(Exception e)
        {
            Message = e?.Message;
        }
    }
}
