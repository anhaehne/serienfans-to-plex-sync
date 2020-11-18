namespace SerienfansPlexSync.Client.ViewModels
{
    public interface ICompletenessInfo
    {
        int Existing { get; }
        
        int Available { get; }
        
        int Missing { get; }
    }
}