using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.LykkeNews;
using Core.Settings;
using Common.Cache;
using Common.Log;
using Core.Settings.LocalClients;
using Flurl.Http;

namespace LkeServices.LykkeNews
{
    public class LykkeNewsService : ILykkeNewsService
    {
        private readonly LykkeNewsServiceClientSettings _settings;
        private readonly ILog _log;
        private readonly ICacheManager _cacheManager;
        private const string NewsCacheKey = "lykke_news_cache";

        public LykkeNewsService(LykkeNewsServiceClientSettings settings, ILog log, ICacheManager cacheManager)
        {
            if (!settings.ServiceUrl.EndsWith("/"))
                settings.ServiceUrl += "/";

            _settings = settings;
            _log = log;
            _cacheManager = cacheManager;
        }

        public async Task<IEnumerable<ILykkeNewsRecord>> GetRecordsCached(int? skip = null, int? take = null)
        {
            var allRecords = await _cacheManager.Get(NewsCacheKey, async () => await GetRecords());

            if (skip != null)
                allRecords = allRecords.Skip(skip.Value);

            if (take != null)
                allRecords = allRecords.Take(take.Value);

            return allRecords;
        }

        private async Task<IQueryable<ILykkeNewsRecord>> GetRecords()
        {
            var result = new List<ILykkeNewsRecord>();
            try
            {
                var getNewsEndpoint = $"{_settings.ServiceUrl}api/news";
                var news = await getNewsEndpoint.GetJsonAsync<ApiNewsRecord[]>();
                foreach (var item in news)
                {
                    result.Add(new LykkeNewsRecordDto
                    {
                        Author = item.Author ?? "Lykke",
                        DateTime = item.DateTime,
                        Text = item.ShortText,
                        Title = item.Title,
                        Url = $"{item.Url}/export",
                        ImgUrl = item.ImgUrl
                    });
                }
            }
            catch (Exception ex)
            {
                await _log.WriteErrorAsync("LykkeNewsService", "GetRecordsCached", "", ex);
            }

            return result.AsQueryable();
        }

        private class ApiNewsRecord
        {
            public string Author { get; set; }
            public string Title { get; set; }
            public DateTime DateTime { get; set; }
            public string ImgUrl { get; set; }
            public string Url { get; set; }
            public string ShortText { get; set; }
            public string Text { get; set; }
        }
    }
}
