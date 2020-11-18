using System.Reactive.Subjects;
using SerienfansPlexSync.Client.Services;
using SerienfansPlexSync.Shared.Models;

namespace SerienfansPlexSync.Client.ViewModels
{
    public class TvShowViewModel
    {
        private readonly IPlexService _plexService;
        private readonly BehaviorSubject<TvShow> _model;

        public TvShowViewModel(TvShow show, IPlexService plexService)
        {
            _plexService = plexService;
            _model = new BehaviorSubject<TvShow>(show);
        }

        public TvShow Model => _model.Value;
    }
}