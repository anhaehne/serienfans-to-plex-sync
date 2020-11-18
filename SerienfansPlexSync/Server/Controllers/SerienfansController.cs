using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using SerienfansPlexSync.Shared.Models;
using SerienfansPlexSync.Shared.Services;

namespace SerienfansPlexSync.Server.Controllers
{
    [Route("serienfans")]
    public class SerienfansController : ControllerBase
    {
        private readonly IMemoryCache _memoryCache;
        private readonly ISerienfansService _serienfansService;

        public SerienfansController(ISerienfansService serienfansService, IMemoryCache memoryCache)
        {
            _serienfansService = serienfansService;
            _memoryCache = memoryCache;
        }

        [HttpGet]
        public async Task<ActionResult<SerienfansSeason?>> GetSerienfansSeasonTask(
            string tvShowName,
            int tvShowYear,
            int seasonNumber,
            string language,
            string quality,
            string hosterName,
            string hosterAbbreviation)
        {
            var cacheKey =
                $"{tvShowName}-{tvShowYear}-{seasonNumber}-{language}-{quality}-{hosterName}-{hosterAbbreviation}";

            if (_memoryCache.TryGetValue<SerienfansSeason>(cacheKey, out var season))
                return season;

            season = await _serienfansService.CheckSeasonAsync(
                new TvShow { Year = tvShowYear, Name = tvShowName },
                new Season { SeasonNumber = seasonNumber },
                language,
                quality,
                new[] { new Hoster(hosterName, hosterAbbreviation) });

            _memoryCache.Set(cacheKey, season, TimeSpan.FromMinutes(15));

            return season;
        }
    }
}