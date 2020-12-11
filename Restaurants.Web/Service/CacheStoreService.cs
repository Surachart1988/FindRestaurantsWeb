using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Restaurants.Web.Models;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace Restaurants.Web.Service
{
    public class MemoryCacheStore : ICacheStore
    {
        private readonly IMemoryCache _memoryCache;
        private readonly GetAsync<PlacesApiQueryResponse> _service;
        public MemoryCacheStore(
            IMemoryCache memoryCache,
            GetAsync<PlacesApiQueryResponse> serv)
        {
            _memoryCache = memoryCache;
            _service = serv;
        }

        public void Add(PlacesApiQueryResponse item, string CacheKey)
        {
            _memoryCache.Set(CacheKey, item,
                    new MemoryCacheEntryOptions()
                    .SetSlidingExpiration(TimeSpan.FromMinutes(5))
                    .SetAbsoluteExpiration(DateTime.Now.AddHours(1)));
        }

        public async Task<PlacesApiQueryResponse> GetAsync<TItem>(ICacheKey<TItem> key, string keyword, double lat = 0, double lng = 0) where TItem : class
        {
            PlacesApiQueryResponse data;
            string cacheKey = $"{key.CacheKey}{keyword}";
            if (_memoryCache.TryGetValue(cacheKey, out data))
            {
                return data;
            }
            else
            {
                if (lat > 0 && lng > 0)
                {
                    data = await _service.GetNearbyAsync(lat, lng); Add(data, cacheKey);
                }
                else
                {
                    data = await _service.GetGeoAsync(keyword); Add(data, cacheKey);
                }
            }
            return data;
        }

        public void Remove<TItem>(ICacheKey<TItem> key)
        {
            this._memoryCache.Remove(key.CacheKey);
        }
    }
    public interface ICacheStore
    {
        void Add(PlacesApiQueryResponse item, string key);

        Task<PlacesApiQueryResponse> GetAsync<TItem>(ICacheKey<TItem> key, string keyword, double lat = 0, double lng = 0) where TItem : class;

        void Remove<TItem>(ICacheKey<TItem> key);
    }
    public interface ICacheStoreItem
    {
        string CacheKey { get; }
    }
    public interface ICacheKey<TItem>
    {
        string CacheKey { get; }
    }
    public class GeoAdrCacheKey : ICacheKey<PlacesApiQueryResponse>
    {
        public string CacheKey => "GeoAdrCache";
    }
    public class NearPlaceCacheKey : ICacheKey<PlacesApiQueryResponse>
    {
        public string CacheKey => "NearPlaceCache";
    }
}