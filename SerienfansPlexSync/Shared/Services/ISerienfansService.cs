using System.Collections.Generic;
using System.Threading.Tasks;
using SerienfansPlexSync.Shared.Models;

namespace SerienfansPlexSync.Shared.Services
{
    public interface ISerienfansService
    {
        Task<SerienfansSeason?> CheckSeasonAsync(TvShow show, Season season, string language, string quality, IReadOnlyCollection<Hoster> hoster);
    }
}