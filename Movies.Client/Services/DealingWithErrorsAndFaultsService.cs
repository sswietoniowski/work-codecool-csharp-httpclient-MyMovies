using Movies.Client.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Marvin.StreamExtensions;

namespace Movies.Client.Services
{
    public class DealingWithErrorsAndFaultsService : IIntegrationService
    {
        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        private readonly IHttpClientFactory _httpClientFactory;

        public DealingWithErrorsAndFaultsService(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
        }

        public async Task Run()
        {
            //GetMovieAndDealWithInvalidResponse(_cancellationTokenSource.Token);
            PostMovieAndHandleValidationErrors(_cancellationTokenSource.Token);
        }

        private async Task GetMovieAndDealWithInvalidResponse(CancellationToken cancellationToken)
        {
            var httpClient = _httpClientFactory.CreateClient("MoviesClient");

            var request = new HttpRequestMessage(HttpMethod.Get, "api/movies/5b1c2b4d-48c7-402a-80c3-cc796ad49c66");
            request.Headers.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
            request.Headers.AcceptEncoding.Add(new System.Net.Http.Headers.StringWithQualityHeaderValue("gzip"));

            using (var response = await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken))
            {
                if (!response.IsSuccessStatusCode)
                {
                    // inspect the status code
                    if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                    {
                        Console.WriteLine("The requested movie cannot be found.");
                        return;
                    }
                    else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    {
                        // trigger a login flow
                        return;
                    }

                    // handle what we can, as for the rest throw an exception as it was in the past
                    response.EnsureSuccessStatusCode();
                }

                var stream = await response.Content.ReadAsStreamAsync();
                var movie = stream.ReadAndDeserializeFromJson<Movie>();
            }
        }

        private async Task PostMovieAndHandleValidationErrors(CancellationToken cancellationToken)
        {
            var httpClient = _httpClientFactory.CreateClient("MoviesClient");

            var movieToCreate = new MovieForCreation()
            {
                Title = "Reservoir Dog"
            };

            var serializedMovieToCreate = JsonConvert.SerializeObject(movieToCreate);

            var request = new HttpRequestMessage(HttpMethod.Post, "api/movies");
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            request.Headers.AcceptEncoding.Add(new StringWithQualityHeaderValue("gzip"));
            request.Content = new StringContent(serializedMovieToCreate);
            request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            using (var response = await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken))
            {
                var stream = await response.Content.ReadAsStreamAsync();

                if (!response.IsSuccessStatusCode)
                {
                    // inspect the status code
                    if (response.StatusCode == System.Net.HttpStatusCode.UnprocessableEntity)
                    {
                        // read out the response body and log it to the console window
                        var validationErrors = stream.ReadAndDeserializeFromJson();
                        Console.WriteLine(validationErrors);
                        return;
                    }
                    else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    {
                        // trigger a login flow
                        return;
                    }

                    // handle what we can, as for the rest throw an exception as it was in the past
                    response.EnsureSuccessStatusCode();
                }

                var movie = stream.ReadAndDeserializeFromJson<Movie>();
            }
        }
    }
}
