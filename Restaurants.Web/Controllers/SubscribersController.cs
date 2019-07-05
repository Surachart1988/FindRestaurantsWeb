using Microsoft.AspNetCore.Mvc;
using Restaurants.Web.Models;
using Restaurants.Web.Service;
using System.Linq;
using System.Threading.Tasks;

namespace Restaurants.Web.Controllers
{
    [Route("/api/[controller]/[action]/")]
    public class SubscribersController : Controller
    {
        private readonly ICacheStore _cacheStore;

        public SubscribersController(ICacheStore cacheStore)
        {
            _cacheStore = cacheStore;
        }

        [HttpGet]
        public async Task<IActionResult> GetPlace(string keyword)
        {
            PlacesApiQueryResponse data;

            data = await _cacheStore.GetAsync(new GeoAdrCacheKey(), keyword);

            var property = data.results.First().geometry;

            data = await _cacheStore.GetAsync(new NearPlaceCacheKey(), keyword, property.location.lat, property.location.lng);

            return Ok(data);
        }
    }
}