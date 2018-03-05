using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Core.LykkeServiceApi;
using Core.Settings;
using Newtonsoft.Json;

namespace LkeServices.LykkeServiceApi
{
    public class LykkeServiceApiConnector : ILykkeServiceApiConnector
    {
        private readonly LykkeServiceApiSettings _lykkeServiceApiSettings;

        public LykkeServiceApiConnector(LykkeServiceApiSettings baseSettings)
        {
            _lykkeServiceApiSettings = baseSettings;
        }

        public async Task<IEnumerable<TResult>> GetAsync<TResult>(string requestPath)
        {
            var requestUrl = new UriBuilder($"{_lykkeServiceApiSettings.ServiceUri}/api/{requestPath}").ToString();

            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("text/plain"));

                var response = await client.GetAsync(requestUrl);

                if ((int)response.StatusCode == 201)
                    return null;

                var receiveStream = await response.Content.ReadAsStreamAsync();

                if (receiveStream == null)
                    throw new Exception("ReceiveStream == null");

                var serializer = new JsonSerializer();

                using (var sr = new StreamReader(receiveStream))
                {
                    using (var jsonTextReader = new JsonTextReader(sr))
                    {
                        return (IEnumerable<TResult>)serializer.Deserialize(jsonTextReader, typeof(IEnumerable<TResult>));
                    }
                }
            }
        }
    }
}