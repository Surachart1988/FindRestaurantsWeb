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

        public void Add<TItem>(PlacesApiQueryResponse item, ICacheKey<TItem> key)
        {
            _memoryCache.Set(key.CacheKey, item,
                    new MemoryCacheEntryOptions()
                    .SetAbsoluteExpiration(TimeSpan.FromSeconds(3)));
        }

        public async Task<PlacesApiQueryResponse> GetAsync<TItem>(ICacheKey<TItem> key, string keyword, double lat = 0, double lng = 0) where TItem : class
        {
            PlacesApiQueryResponse data;
            if (_memoryCache.TryGetValue(key.CacheKey, out data))
            {
                return data;
            }
            else
            {
                if (lat > 0 && lng > 0)
                {
                    data = await _service.GetNearbyAsync(lat, lng); Add(data, new NearPlaceCacheKey());
                }
                else
                {
                    data = await _service.GetGeoAsync(keyword); Add(data, new GeoAdrCacheKey());
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
        void Add<TItem>(PlacesApiQueryResponse item, ICacheKey<TItem> key);

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