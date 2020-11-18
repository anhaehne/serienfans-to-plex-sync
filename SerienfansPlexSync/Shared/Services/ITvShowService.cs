using System.Threading.Tasks;
using SerienfansPlexSync.Shared.Models;

namespace SerienfansPlexSync.Shared.Services
{
    public interface ITvShowService
    {
        Task<TvShow?> GetTvShowAsync(string name, int year, string language = "DE");
    }
}