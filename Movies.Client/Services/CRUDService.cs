using Movies.Client.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Movies.Client.Services
{
    public class CRUDService : IIntegrationService
    {
        private static HttpClient _httpClient = new HttpClient();

        public CRUDService()
        {
            _httpClient.BaseAddress = new Uri("http://localhost:33333");
            _httpClient.Timeout = new TimeSpan(0, 0, 30);
            _httpClient.DefaultRequestHeaders.Clear();
            // accept header is crucial, here we can see how to use different Accept values + quality value (our preference)
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/xml", 0.9));
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            // here default headers
            // only common options... now vnd.... is typical
        }

        public async Task Run()
        {
            //1
            //await GetResource();
            //2
            //await GetResourceThroughHttpRequestMessage();
            //3
            //CreateResource();
            //4
            //UpdateResource();
            //5
            //DeleteResource();
            //6
            //PostResourceShortcut();
            //7
            //PutResourceShortcut();
            //8
            DeleteResourceShortcut();
            // why not use shortcuts :-), because they are less flexible, by not allowing you to define Accept header per request
        }

        private async void DeleteResourceShortcut()
        {
            var response = await _httpClient.DeleteAsync("api/movies/5b1c2b4d-48c7-402a-80c3-cc796ad49c6b");
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
        }

        private async void PutResourceShortcut()
        {
            var movieToUpdate = new MovieForUpdate
            {
                Title = "Pulp Fiction",
                Description = "Blah, blah, blah, ...",
                DirectorId = Guid.Parse("d28888e9-2ba9-473a-a40f-e38cb54f9b35"),
                ReleaseDate = new DateTimeOffset(new DateTime(1992, 9, 2)),
                Genre = "Crime, Drama"
            };

            var response = await _httpClient.PutAsync("api/movies/5b1c2b4d-48c7-402a-80c3-cc796ad49c6b", new StringContent(JsonSerializer.Serialize(movieToUpdate), Encoding.UTF8, "application/json"));

            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var updateMovie = JsonSerializer.Deserialize<Movie>(content, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });
        }

        private async void PostResourceShortcut()
        {
            //_httpClient.PostAsync(); // only shortcut to use HttpRequestMessage
            var movieToCreate = new MovieForCreation()
            {
                Title = "Reservoir Dog",
                Description = "Blah, blah, blah, ...",
                DirectorId = Guid.Parse("d28888e9-2ba9-473a-a40f-e38cb54f9b35"),
                ReleaseDate = new DateTimeOffset(new DateTime(1992, 9, 2)),
                Genre = "Crime, Drama"
            };

            var response = await _httpClient.PostAsync("api/movies", new StringContent(JsonSerializer.Serialize(movieToCreate), Encoding.UTF8, "application/json"));
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();

            var createdMovie = JsonSerializer.Deserialize<Movie>(content, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });
        }

        public async Task GetResource()
        {
            //not use using with HttpClient, long live, reused
            var response = await _httpClient.GetAsync("api/movies");
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            var movies = new List<Movie>();
            if (response.Content.Headers.ContentType.MediaType == "application/json")
            {
                movies = JsonSerializer.Deserialize<List<Movie>>(content, new JsonSerializerOptions()
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });
            } 
            else if (response.Content.Headers.ContentType.MediaType == "application/xml")
            {
                var serializer = new XmlSerializer(typeof(List<Movie>));
                movies = (List<Movie>)serializer.Deserialize(new StringReader(content));
            }
            else
            {

            }

            // do something with the movies list
        }

        public async Task GetResourceThroughHttpRequestMessage()
        {
            // proper way to share _httpClient while useing different headers inside different threads
            // here headers applicable whether or not a request has a body
            var request = new HttpRequestMessage(HttpMethod.Get, "api/movies");
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            var movies = new List<Movie>();
            if (response.Content.Headers.ContentType.MediaType == "application/json")
            {
                movies = JsonSerializer.Deserialize<List<Movie>>(content, new JsonSerializerOptions()
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });
            }
            else if (response.Content.Headers.ContentType.MediaType == "application/xml")
            {
                var serializer = new XmlSerializer(typeof(List<Movie>));
                movies = (List<Movie>)serializer.Deserialize(new StringReader(content));
            }
            else
            {

            }

            // do something with the movies list

        }

        public async Task CreateResource()
        {
            //_httpClient.PostAsync(); // only shortcut to use HttpRequestMessage
            var movieToCreate = new MovieForCreation()
            {
                Title = "Reservoir Dog",
                Description = "Blah, blah, blah, ...",
                DirectorId = Guid.Parse("d28888e9-2ba9-473a-a40f-e38cb54f9b35"),
                ReleaseDate = new DateTimeOffset(new DateTime(1992, 9, 2)),
                Genre = "Crime, Drama"
            };

            var serializedMovieToCreate = JsonSerializer.Serialize(movieToCreate);

            var request = new HttpRequestMessage(HttpMethod.Post, "api/movies");
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            request.Content = new StringContent(serializedMovieToCreate);
            // here headers related to the body of a request
            request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();

            var createdMovie = JsonSerializer.Deserialize<Movie>(content, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });
        }


        public async Task UpdateResource()
        {
            var movieToUpdate = new MovieForUpdate
            {
                Title = "Pulp Fiction",
                Description = "Blah, blah, blah, ...",
                DirectorId = Guid.Parse("d28888e9-2ba9-473a-a40f-e38cb54f9b35"),
                ReleaseDate = new DateTimeOffset(new DateTime(1992, 9, 2)),
                Genre = "Crime, Drama"
            };

            var serializedMovieToUpdate = JsonSerializer.Serialize(movieToUpdate);

            var request = new HttpRequestMessage(HttpMethod.Put, "api/movies/5b1c2b4d-48c7-402a-80c3-cc796ad49c6b");
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            request.Content = new StringContent(serializedMovieToUpdate);
            request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var updateMovie = JsonSerializer.Deserialize<Movie>(content, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

        }

        public async Task DeleteResource()
        {
            var request = new HttpRequestMessage(HttpMethod.Delete, "api/movies/5b1c2b4d-48c7-402a-80c3-cc796ad49c6b");
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
        }


    }
}
