using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Blazored.LocalStorage;
using Microsoft.Extensions.Logging;
using Microsoft.VisualBasic;
using Plex.Api;
using Plex.Api.Api;
using Plex.Api.Models;
using Plex.Api.Models.Server;
using SerienfansPlexSync.Client.Authentication;
using SerienfansPlexSync.Shared.Models;

namespace SerienfansPlexSync.Client.Services
{
    public interface IPlexService
    {
        Task<IReadOnlyCollection<PlexTvShow>> GetTvShowsAsync(PlexServer server);

        Task<IReadOnlyCollection<PlexServer>> GetServersAsync();
        Task<IReadOnlyCollection<Season>> GetSeasonsAsync(PlexTvShow tvShow, PlexServer server);
    }

    public class PlexService : IPlexService
    {
        private readonly ILocalStorageService _localStorageService;
        private readonly IApiService _apiService;
        private readonly ILogger<PlexService> _logger;
        private readonly IPlexClient _plexClient;

        public PlexService(
            IPlexClient plexClient,
            ILogger<PlexService> logger,
            ILocalStorageService localStorageService,
            IApiService apiService)
        {
            _plexClient = plexClient;
            _logger = logger;
            _localStorageService = localStorageService;
            _apiService = apiService;
        }

        public async Task<IReadOnlyCollection<PlexServer>> GetServersAsync()
        {
            _logger.LogInformation("Loading servers from plex.");

            var token = await _localStorageService.GetItemAsync<string>(PlexAuthenticationProvider.PLEX_TOKEN);

            if (string.IsNullOrEmpty(token))
                throw new UnauthorizedAccessException("You are not signed in.");
            
            var servers = await GetServersAsync(token);

            return (await Task.WhenAll(servers.Select(s => DiscoverServer(s.Name, s.AccessToken, s.Addresses)))).ToList();
        }

        public async Task<IReadOnlyCollection<PlexTvShow>> GetTvShowsAsync(PlexServer server)
        {
            _logger.LogInformation($"Retrieving plex tv shows from server {server.Name}.");

            var libraries = await _plexClient.GetLibraries(server.AccessToken, server.Host);

            var tvDirectories = libraries.MediaContainer.Directory.Where(x => x.Type == "show").ToList();

            var tvShowMetadata = (await Task.WhenAll(tvDirectories.Select(d => _plexClient.GetLibrary(server.AccessToken, server.Host, d.Key)))).SelectMany(x => x.MediaContainer.Metadata).ToList();

            var tvShows = tvShowMetadata.Select(metadata => new PlexTvShow(metadata.Title, metadata.Year, metadata.RatingKey));

            return tvShows.ToList();
        }

        public async Task<IReadOnlyCollection<Season>> GetSeasonsAsync(PlexTvShow tvShow, PlexServer server)
        {
            var seasonMetadata = await _plexClient.GetChildrenMetadata(
                    server.AccessToken,
                    server.Host,
                    int.Parse(tvShow.Key));

            return await Task.WhenAll(seasonMetadata.MediaContainer.Metadata.Select(GetSeasonsFromMetadataAsync));

            async Task<Season> GetSeasonsFromMetadataAsync(Metadata metadata)
            {
                var episodesMetadata = await _plexClient.GetChildrenMetadata(
                    server.AccessToken,
                    server.Host,
                    int.Parse(metadata.RatingKey));

                var episodes = episodesMetadata.MediaContainer.Metadata.Select(
                    m => new Episode
                    {
                        Title = m.Title,
                        SeasonNumber = m.ParentIndex,
                        EpisodeNumber = m.Index
                    });

                return new Season { SeasonNumber = metadata.Index, Episodes = episodes.ToList() };
            }
        }

        private async Task<PlexServer> DiscoverServer(string name, string accessToken, string[] addresses)
        {
            var tasks = addresses.Select(
                    async currentAddress =>
                    {
                        var success = await TryConnectAsync(currentAddress);
                        return new PlexServer(name, currentAddress, accessToken, success);
                    })
                .ToList();

            // Wait for any probe to finish successfully.
            while (tasks.Any())
            {
                var finished = await Task.WhenAny(tasks);

                if (finished.Result.Success)
                    return finished.Result;

                tasks.Remove(finished);
            }

            return new PlexServer(name, string.Empty, string.Empty, false);

            async Task<bool> TryConnectAsync(string host)
            {
                _logger.LogInformation($"Validating plex server address {host}");

                try
                {
                    await _plexClient.GetPlexInfo(accessToken, host);
                    _logger.LogInformation($"Plex server address {host} is valid.");
                    return true;
                }
                catch (HttpRequestException)
                {
                    _logger.LogInformation($"Plex server address {host} is not valid.");
                    return false;
                }
            }
        }

        private async Task<IReadOnlyCollection<(string Name, string AccessToken, string[] Addresses)>> GetServersAsync(string authToken)
        {
            var apiRequest = new ApiRequestBuilder("https://plex.tv/pms/resources.xml", "", HttpMethod.Get)
                .AddPlexToken(authToken)
                .AddQueryParam("includeHttps", "1")
                .Build();

            ResourceContainer resourceContainer = await _apiService.InvokeApiAsync<ResourceContainer>(apiRequest);

            return resourceContainer.Devices.Where(d => d.Provides.Contains("server"))
                .Select(r => (r.Name, r.AccessToken, r.Connections.Select(c => c.Uri).ToArray()))
                .ToList();
        }
    }


    public record PlexServer(string Name, string Host, string AccessToken, bool Success);

    public record PlexTvShow(string Name, int Year, string Key);

}