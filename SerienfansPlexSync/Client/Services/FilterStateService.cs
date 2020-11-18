using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using SerienfansPlexSync.Shared.Models;

namespace SerienfansPlexSync.Client.Services
{
    public interface IFilterStateService
    {
        IReadOnlyList<Language> Languages { get; }

        BehaviorSubject<Language> SelectedLanguage { get; }

        IReadOnlyList<string> Qualities { get; }

        BehaviorSubject<string> SelectedQuality { get; }

        IReadOnlyList<PlexServer> Servers { get; }

        BehaviorSubject<PlexServer?> SelectedServer { get; }

        IReadOnlyList<Hoster> Hoster { get; }

        BehaviorSubject<Hoster?> SelectedHoster { get; }

        BehaviorSubject<bool> SearchOldSeasons { get; }

        Task InitializeAsync();
    }

    public class FilterStateService : IFilterStateService
    {
        private readonly IPlexService _plexService;

        public FilterStateService(IPlexService plexService)
        {
            _plexService = plexService;
            Languages = new[] { new Language("DE", "German"), new Language("EN", "English") };
            SelectedLanguage = new BehaviorSubject<Language>(Languages.First());
            Qualities = new[] { "1080p", "720p", "480p" };
            SelectedQuality = new BehaviorSubject<string>(Qualities.First());

            Hoster = new[]
            {
                new Hoster("ddownload", "DD"), new Hoster("1ficher", "1F"), new Hoster("rapidgator", "RG"),
                new Hoster("uploaded", "UL"), new Hoster("nitroflare", "NI")
            };
        }
        
        public IReadOnlyList<Language> Languages { get; }

        public BehaviorSubject<Language> SelectedLanguage { get; }

        public IReadOnlyList<string> Qualities { get; }

        public BehaviorSubject<string> SelectedQuality { get; }

        public IReadOnlyList<PlexServer> Servers { get; private set; } = Array.Empty<PlexServer>();

        public BehaviorSubject<PlexServer?> SelectedServer { get; } = new BehaviorSubject<PlexServer?>(null);

        public IReadOnlyList<Hoster> Hoster { get; }

        public BehaviorSubject<Hoster?> SelectedHoster { get; } = new BehaviorSubject<Hoster?>(null);

        public BehaviorSubject<bool> SearchOldSeasons { get; } = new BehaviorSubject<bool>(false);

        public async Task InitializeAsync()
        {
            Servers = (await _plexService.GetServersAsync()).ToList();
        }
    }
}
