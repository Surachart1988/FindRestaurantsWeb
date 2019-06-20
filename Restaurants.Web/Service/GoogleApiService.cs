using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Restaurants.Web.Models;
using System.Net.Http;
using System.Threading.Tasks;

namespace Restaurants.Web.Service
{

    public interface GetAsync<T> where T : class
    {
        Task<T> GetGeoAsync(string keyword);
        Task<T> GetNearbyAsync(double lat, double lng);
    }

    public class GoogleApiService : GetAsync<PlacesApiQueryResponse>
    {
        private string _url = "https://maps.googleapis.com/maps/api/";
        public string _apikey { get; }

        private readonly HttpClient _httpClient;
        public GoogleApiService(IConfiguration configuration)
        {
            _httpClient = new HttpClient();
            _apikey = configuration["google:ApiKey"];
        }

        public async Task<PlacesApiQueryResponse> GetGeoAsync(string keyword)
        {
            var request = await _httpClient.GetStringAsync(string.Format(_url + "geocode/json?address={0}&key={1}", keyword, _apikey));

            return JsonConvert.DeserializeObject<PlacesApiQueryResponse>(request);
        }

        public async Task<PlacesApiQueryResponse> GetNearbyAsync(double lat, double lng)
        {
            var request2 = await _httpClient.GetStringAsync(string.Format(_url + "place/nearbysearch/json?location={0},{1}&radius=1500&type=restaurant&key={2}", lat, lng, _apikey));
            return JsonConvert.DeserializeObject<PlacesApiQueryResponse>(request2);
        }
    }
}