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
            _httpClient.BaseAddress = new Uri("http://localhost:57863");
            _httpClient.Timeout = new TimeSpan(0, 0, 30);
            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/xml", 0.9));
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public async Task Run()
        {
            await GetResource();
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

        }

    }
}
