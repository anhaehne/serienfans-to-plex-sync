using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using HtmlAgilityPack;
using SerienfansPlexSync.Client.Services;
using SerienfansPlexSync.Shared.Models;
using SerienfansPlexSync.Shared.Services;

namespace SerienfansPlexSync.Server.Services
{
    public class SerienfansService : ISerienfansService
    {
        private const string EMPTY_RESPONSE = "Leider liegen zu dieser Staffel";

        private static readonly ConcurrentDictionary<string, Lazy<Task<string?>>> _tvIdRequest =
            new ConcurrentDictionary<string, Lazy<Task<string?>>>();

        private readonly IHttpClientFactory _httpClientFactory;

        public SerienfansService(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<SerienfansSeason?> CheckSeasonAsync(
            TvShow show,
            Season season,
            string language,
            string quality,
            IReadOnlyCollection<Hoster> hoster)
        {
            var tvShowId = await GetSerienfansId(show, language);

            if (tvShowId is null)
                return SerienfansSeason.Empty;

            var result = await GetSeason(tvShowId, season.SeasonNumber, quality, language);

            if (result.Contains(EMPTY_RESPONSE))
                return SerienfansSeason.Empty;

            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(result);

            var entry = htmlDoc.DocumentNode.SelectNodes("//div[@class='entry']").First();

            var episodeNodes = entry.SelectNodes("//div[@class='entry']/div[@class='list simple']/div[@class='row']");

            if (episodeNodes is null)
                return SerienfansSeason.Empty;

            var episodes = episodeNodes.Select(GetEpisode).Where(e => e.Mirrors.Any()).ToList();

            var seasonMirror = GetMirrors(entry).FirstOrDefault();

            return new SerienfansSeason { Link = seasonMirror?.Link ?? string.Empty, Episodes = episodes };

            SerienfansEpisode GetEpisode(HtmlNode episode)
            {
                var index = int.Parse(episode.ChildNodes.FindFirst("div").InnerText.Replace(".", "").Trim());

                return new SerienfansEpisode { Index = index, Mirrors = GetMirrors(episode) };
            }

            IReadOnlyCollection<SerienfansMirror> GetMirrors(HtmlNode node)
            {
                var mirrorNodes = node.SelectNodes("div[@class='row']/a[@class='dlb row']");

                return mirrorNodes.Select(GetMirror)
                    .Where(
                        mirror => hoster.Any(
                            h => string.Equals(h.Abbreviation, mirror.MirrorId, StringComparison.OrdinalIgnoreCase) ||
                                 string.Equals(h.Name, mirror.MirrorId, StringComparison.OrdinalIgnoreCase)))
                    .ToList();
            }

            SerienfansMirror GetMirror(HtmlNode node)
            {
                var mirror = node.SelectSingleNode("div/span").InnerText ?? "<invalid>";
                var link = node.GetAttributeValue("href", "");

                return new SerienfansMirror { MirrorId = mirror, Link = "https://serienfans.org" + link };
            }
        }

        private async Task<string?> GetSerienfansId(TvShow tvShow, string language)
        {
            var request = _tvIdRequest.GetOrAdd(
                $"{tvShow.Name}-{language}",
                s => new Lazy<Task<string?>>(() => GetSerienfansIdInternal(tvShow, language)));

            return await request.Value;

            async Task<string?> GetSerienfansIdInternal(TvShow tvShow, string language)
            {
                var client = _httpClientFactory.CreateClient("Serienfans");

                var response =
                    await client.GetFromJsonAsync<SerienfansSearchResponse>($"v2/search?q={tvShow.Name}&ql={language}");

                if (response?.Result is null)
                    return null;

                var result =
                    response.Result.SingleOrDefault(r => r.Title == tvShow.Name && r.Year == tvShow.Year) ??
                    response.Result.FirstOrDefault(r => r.Title == tvShow.Name);

                return result?.Id;
            }
        }

        private async Task<string> GetSeason(string tvShowId, int season, string quality, string language)
        {
            var client = _httpClientFactory.CreateClient("Serienfans");

            var response = await client.GetFromJsonAsync<SerienfansSeasonResponse>(
                $"v1/{tvShowId}/season/{season}?q={quality}&lang={language}");

            return response?.Html ?? EMPTY_RESPONSE;
        }

        private class SerienfansSeasonResponse
        {
            public string? Html { get; set; }
        }

        private class SerienfansSearchResponse
        {
            [JsonPropertyName("result")]
            public SerienfansSearchResult[]? Result { get; set; }
        }

        private class SerienfansSearchResult
        {
            [JsonPropertyName("url_id")]
            public string? Id { get; set; }

            [JsonPropertyName("year")]
            public long? Year { get; set; }

            [JsonPropertyName("title")]
            public string? Title { get; set; }
        }
    }
}