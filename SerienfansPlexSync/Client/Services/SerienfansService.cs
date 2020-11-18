using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SerienfansPlexSync.Shared.Models;
using SerienfansPlexSync.Shared.Services;

namespace SerienfansPlexSync.Client.Services
{
    public class SerienfansService : ISerienfansService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<SerienfansService> _logger;

        public SerienfansService(HttpClient httpClient, ILogger<SerienfansService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<SerienfansSeason?> CheckSeasonAsync(
            TvShow show,
            Season season,
            string language,
            string quality,
            IReadOnlyCollection<Hoster> hoster)
        {
            _logger.LogInformation(
                $"Requesting season {season.SeasonNumber} of tv show {show.Name} ({show.Year}) from serienfans.");

            try
            {
                return await _httpClient.GetFromJsonAsync<SerienfansSeason>(
                    $"serienfans?tvShowName={show.Name}&tvShowYear={show.Year}&seasonNumber={season.SeasonNumber}&language={language}&quality={quality}&hosterName={hoster.First().Name}&hosterAbbreviation={hoster.First().Abbreviation}");
            }
            catch (HttpRequestException)
            {
                return null;
            }
        }
    }
}