using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Restaurants.Web.Models;
using Restaurants.Web.Service;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Restaurants.Web.Controllers
{
    [Route("/api/[controller]/[action]/")]
    public class SubscribersController : Controller
    {
        private readonly IMemoryCache _MemoryCache;

        private readonly GetAsync<PlacesApiQueryResponse> _service;

        public SubscribersController(IMemoryCache memCache,
            GetAsync<PlacesApiQueryResponse> serv)
        {
            _MemoryCache = memCache;
            _service = serv;
        }

        [HttpGet]
        public async Task<IActionResult> GetPlace(string keyword)
        {
            PlacesApiQueryResponse data;

            data = await GetGeoAdrAsync("Req1", keyword);

            var property = data.results.First().geometry;

            data = await GetNearPlaceAsync("Req2", property.location.lat, property.location.lng);

            return Ok(data);
        }

        private async Task<PlacesApiQueryResponse> GetGeoAdrAsync(string keys, string keyword)
        {
            PlacesApiQueryResponse data;
            // We will try to get the Cache data If the data is present in cache the Condition will be true else it is false 
            if (!_MemoryCache.TryGetValue(keys, out data))
            {
                data = await _service.GetGeoAsync(keyword);
                //Save the received data in cache
                _MemoryCache.Set(keys, data,
                    new MemoryCacheEntryOptions()
                    .SetAbsoluteExpiration(TimeSpan.FromSeconds(3)));
            }
            return data;
        }

        private async Task<PlacesApiQueryResponse> GetNearPlaceAsync(string keys, double lat, double lng)
        {
            PlacesApiQueryResponse data;
            // We will try to get the Cache data If the data is present in cache the Condition will be true else it is false 
            if (!_MemoryCache.TryGetValue(keys, out data))
            {
                data = await _service.GetNearbyAsync(lat, lng);
                //Save the received data in cache
                _MemoryCache.Set(keys, data,
                    new MemoryCacheEntryOptions()
                    .SetAbsoluteExpiration(TimeSpan.FromSeconds(3)));
                //ViewBag.Status = "Data is added in Cache";

            }
            return data;
        }
    }
}