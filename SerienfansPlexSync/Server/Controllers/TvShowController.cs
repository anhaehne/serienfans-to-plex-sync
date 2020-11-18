using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using SerienfansPlexSync.Shared.Models;
using SerienfansPlexSync.Shared.Services;

namespace SerienfansPlexSync.Server.Controllers
{
    [Route("TvShows")]
    public class TvShowController : ControllerBase
    {
        private readonly IMemoryCache _memoryCache;
        private readonly ITvShowService _movieDbService;

        public TvShowController(ITvShowService movieDbService, IMemoryCache memoryCache)
        {
            _movieDbService = movieDbService;
            _memoryCache = memoryCache;
        }

        [HttpGet]
        public async Task<ActionResult<TvShow?>> GetTvShow(string name, int year, string language = "DE")
        {
            if (_memoryCache.TryGetValue<TvShow>($"{name}-{year}-{language}", out var result))
                return result is not null ? Ok(result) : NotFound();

            result = await _movieDbService.GetTvShowAsync(name, year, language);

            _memoryCache.Set($"{name}-{year}-{language}", result, TimeSpan.FromMinutes(10));

            if (result is null)
                return NotFound();

            return Ok(result);
        }
    }
}