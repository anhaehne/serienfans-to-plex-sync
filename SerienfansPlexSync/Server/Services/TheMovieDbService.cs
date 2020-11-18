using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SerienfansPlexSync.Server.Config;
using SerienfansPlexSync.Shared.Models;
using SerienfansPlexSync.Shared.Services;
using TMDbLib.Client;

namespace SerienfansPlexSync.Server.Services
{
    public class TheMovieDbService : ITvShowService
    {
        private readonly ILogger<TheMovieDbService> _logger;
        private readonly IOptions<TheMovieDbConfig> _movieDbConfig;

        public TheMovieDbService(ILogger<TheMovieDbService> logger, IOptions<TheMovieDbConfig> movieDbConfig)
        {
            _logger = logger;
            _movieDbConfig = movieDbConfig;
        }

        public async Task<TvShow?> GetTvShowAsync(string name, int year, string language = "DE")
        {
            _logger.LogInformation($"Searching TMDb for {name} ({year})");

            var client = new TMDbClient(_movieDbConfig.Value.ApiKey)
            {
                DefaultLanguage = language.ToLower()
            };

            var results = await client.SearchTvShowAsync(name);

            var showResult =
                results.Results.FirstOrDefault(r => r.FirstAirDate.HasValue && r.FirstAirDate.Value.Year == year) ??
                results.Results.FirstOrDefault();

            if (showResult == null)
            {
                _logger.LogInformation($"No result for {name} ({year})");

                return null;
            }

            var show = await client.GetTvShowAsync(showResult.Id);

            var seasons =
                await Task.WhenAll(show.Seasons.Select(s => client.GetTvSeasonAsync(show.Id, s.SeasonNumber)));

            var externalIds = await client.GetTvShowExternalIdsAsync(showResult.Id);

            if (externalIds == null)
                return null;

            return new TvShow
            {
                ImdbId = externalIds.ImdbId, Name = name, Year = year,
                Seasons = seasons.Where(s => s.SeasonNumber > 0).Select(
                        s => new Season
                        {
                            SeasonNumber = s.SeasonNumber,
                            Episodes = s.Episodes.Select(
                                    e => new Episode
                                    {
                                        SeasonNumber = e.SeasonNumber,
                                        EpisodeNumber = e.EpisodeNumber,
                                        Title = e.Name
                                    })
                                .ToList()
                        })
                    .ToList()
            };
        }
    }
}