using Movies.Client.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
            //await GetPosterWithStreamAndCompletionMode();
            // performance testing
            //TestGetPosterWithoutStream();
            //TestGetPosterWithStream();
            //TestGetPosterWithStreamAndCompletionMode();
            //PostPosterWithStream();
            //PostAndReadPosterWithStream();
            // option Marvin.StreamExtensions
            // performance testing
            TestPostPosterWithoutStream();
            TestPostPosterWithStream();
            TestPostAndReadPosterWithStream();
        }

        public async Task TestPostPosterWithoutStream()
        {
            // warmup
            PostPosterWithoutStream();

            var stopWatch = Stopwatch.StartNew();

            for (int i = 0; i < 200; i++)
            {
                PostPosterWithoutStream();
            }

            stopWatch.Stop();

            Console.WriteLine($"Elapsed ms without stream: {stopWatch.ElapsedMilliseconds}, averaging: {stopWatch.ElapsedMilliseconds / 200} ms/request");
        }

        public async Task TestPostPosterWithStream()
        {
            // warmup
            PostPosterWithStream();

            var stopWatch = Stopwatch.StartNew();

            for (int i = 0; i < 200; i++)
            {
                PostPosterWithStream();
            }

            stopWatch.Stop();

            Console.WriteLine($"Elapsed ms with stream: {stopWatch.ElapsedMilliseconds}, averaging: {stopWatch.ElapsedMilliseconds / 200} ms/request");
        }

        public async Task TestPostAndReadPosterWithStream()
        {
            // warmup
            PostAndReadPosterWithStream();

            var stopWatch = Stopwatch.StartNew();

            for (int i = 0; i < 200; i++)
            {
                PostAndReadPosterWithStream();
            }

            stopWatch.Stop();

            Console.WriteLine($"Elapsed ms with stream (post & read): {stopWatch.ElapsedMilliseconds}, averaging: {stopWatch.ElapsedMilliseconds / 200} ms/request");
        }

        private async Task PostPosterWithoutStream()
        {
            var random = new Random();
            var generatedBytes = new byte[524288];
            random.NextBytes(generatedBytes);

            var posterForCreation = new PosterForCreation()
            {
                Name = "A new poster for The Big Lebowski",
                Bytes = generatedBytes
            };

            var serializedPosterForCreation = JsonConvert.SerializeObject(posterForCreation);

            var request = new HttpRequestMessage(HttpMethod.Post, $"api/movies/5b1c2b4d-48c7-402a-80c3-cc796ad49c6b/posters");
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            request.Content = new StringContent(serializedPosterForCreation);
            request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            var createdMovie = JsonConvert.DeserializeObject<Poster>(content);

        }


        private async Task PostAndReadPosterWithStream()
        {
            var random = new Random();
            var generatedBytes = new byte[524288];
            random.NextBytes(generatedBytes);

            var posterForCreation = new PosterForCreation()
            {
                Name = "A new poster for The Big Lebowski",
                Bytes = generatedBytes
            };

            var memoryContentStream = new MemoryStream();
            memoryContentStream.SerializeToJsonAndWrite<PosterForCreation>(posterForCreation);
            memoryContentStream.Seek(0, SeekOrigin.Begin);

            using (var request = new HttpRequestMessage(HttpMethod.Post, $"api/movies/5b1c2b4d-48c7-402a-80c3-cc796ad49c6b/posters"))
            {
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                using (var streamContent = new StreamContent(memoryContentStream))
                {
                    streamContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                    request.Content = streamContent;
                    var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
                    response.EnsureSuccessStatusCode();

                    using (var stream = await response.Content.ReadAsStreamAsync())
                    {
                        response.EnsureSuccessStatusCode();

                        // simplification using extension method

                        var poster = stream.ReadAndDeserializeFromJson<Poster>();
                    }
                }
            }
        }

        private async Task PostPosterWithStream()
        {
            var random = new Random();
            var generatedBytes = new byte[524288];
            random.NextBytes(generatedBytes);

            var posterForCreation = new PosterForCreation()
            {
                Name = "A new poster for The Big Lebowski",
                Bytes = generatedBytes
            };

            var memoryContentStream = new MemoryStream();
            memoryContentStream.SerializeToJsonAndWrite<PosterForCreation>(posterForCreation);
            memoryContentStream.Seek(0, SeekOrigin.Begin);

            using (var request = new HttpRequestMessage(HttpMethod.Post, $"api/movies/5b1c2b4d-48c7-402a-80c3-cc796ad49c6b/posters"))
            {
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                using (var streamContent = new StreamContent(memoryContentStream))
                {
                    streamContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                    request.Content = streamContent;
                    var response = await _httpClient.SendAsync(request);
                    response.EnsureSuccessStatusCode();

                    // shouldn't that be based on stream too?

                    var createdContent = await response.Content.ReadAsStringAsync();
                    var createdPoster = JsonConvert.DeserializeObject<Poster>(createdContent);
                }
            }
        }

        public async Task GetPosterWithoutStream()
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"api/movies/5b1c2b4d-48c7-402a-80c3-cc796ad49c6b/posters/{Guid.NewGuid()}");
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var poster = JsonConvert.DeserializeObject<Poster>(content);
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

        public async Task TestGetPosterWithoutStream()
        {
            // warmup
            GetPosterWithoutStream();

            var stopWatch = Stopwatch.StartNew();

            for (int i = 0; i < 200; i++)
            {
                GetPosterWithoutStream();
            }

            stopWatch.Stop();

            Console.WriteLine($"Elapsed ms without stream: {stopWatch.ElapsedMilliseconds}, averaging: {stopWatch.ElapsedMilliseconds / 200} ms/request");
        }

        public async Task TestGetPosterWithStream()
        {
            // warmup
            GetPosterWithStream();

            var stopWatch = Stopwatch.StartNew();

            for (int i = 0; i < 200; i++)
            {
                GetPosterWithStream();
            }

            stopWatch.Stop();

            Console.WriteLine($"Elapsed ms with stream: {stopWatch.ElapsedMilliseconds}, averaging: {stopWatch.ElapsedMilliseconds / 200} ms/request");
        }
        public async Task TestGetPosterWithStreamAndCompletionMode()
        {
            // warmup
            GetPosterWithStreamAndCompletionMode();

            var stopWatch = Stopwatch.StartNew();

            for (int i = 0; i < 200; i++)
            {
                GetPosterWithStreamAndCompletionMode();
            }

            stopWatch.Stop();

            Console.WriteLine($"Elapsed ms with stream and completion mode: {stopWatch.ElapsedMilliseconds}, averaging: {stopWatch.ElapsedMilliseconds / 200} ms/request");
        }
    }
}
