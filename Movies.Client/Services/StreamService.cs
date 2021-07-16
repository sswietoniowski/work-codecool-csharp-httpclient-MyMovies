using Movies.Client.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Movies.Client.Services
{
    public class StreamService : IIntegrationService
    {
        private static HttpClient _httpClient = new HttpClient();

        public StreamService()
        {
            _httpClient.BaseAddress = new Uri("http://localhost:33333");
            _httpClient.Timeout = new TimeSpan(0, 0, 30);
            _httpClient.DefaultRequestHeaders.Clear();
        }

        public async Task Run()
        {
            //await GetPosterWithStream();
            await GetPosterWithStreamAndCompletionMode();
        }          

        public async Task GetPosterWithStream()
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"api/movies/5b1c2b4d-48c7-402a-80c3-cc796ad49c6b/posters/{Guid.NewGuid()}");
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var response = await _httpClient.SendAsync(request);
            using (var stream = await response.Content.ReadAsStreamAsync())
            {
                response.EnsureSuccessStatusCode();

                using (var streamReader = new StreamReader(stream))
                {
                    using (var jsonTextReader = new JsonTextReader(streamReader))
                    {
                        var jsonSerializer = new JsonSerializer();
                        var poster = jsonSerializer.Deserialize<Poster>(jsonTextReader);

                        // do something with the poster
                    }
                }
            }
        }

        public async Task GetPosterWithStreamAndCompletionMode()
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"api/movies/5b1c2b4d-48c7-402a-80c3-cc796ad49c6b/posters/{Guid.NewGuid()}");
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
            using (var stream = await response.Content.ReadAsStreamAsync())
            {
                response.EnsureSuccessStatusCode();

                // simplification using extension method

                var poster = stream.ReadAndDeserializeFromJson<Poster>();
            }
        }
    }
}
