using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Core.LykkeNews
{
    public interface ILykkeNewsRecord
    {
        string Author { get; set; }
        string Title { get; set; }
        DateTime DateTime { get; set; }
        string ImgUrl { get; set; }
        string Url { get; set; }
        string Text { get; set; }
    }

    public class LykkeNewsRecordDto : ILykkeNewsRecord
    {
        public string Author { get; set; }
        public string Title { get; set; }
        public DateTime DateTime { get; set; }
        public string ImgUrl { get; set; }
        public string Url { get; set; }
        public string Text { get; set; }
    }

    public interface ILykkeNewsService
    {
        Task<IEnumerable<ILykkeNewsRecord>> GetRecordsCached(int? skip = null, int? take = null);
    }
}
