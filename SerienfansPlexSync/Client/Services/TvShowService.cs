using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SerienfansPlexSync.Shared.Models;
using SerienfansPlexSync.Shared.Services;

namespace SerienfansPlexSync.Client.Services
{
    public class TvShowService : ITvShowService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<TvShowService> _logger;

        public TvShowService(HttpClient httpClient, ILogger<TvShowService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<TvShow?> GetTvShowAsync(string name, int year, string language = "DE")
        {
            _logger.LogInformation($"Searching tv shows for {name} ({year})");

            try
            {
                return await _httpClient.GetFromJsonAsync<TvShow>($"TvShows?name={name}&year={year}&language={language}");
            }
            catch (HttpRequestException)
            {
                return null;
            }
        }
    }
}
