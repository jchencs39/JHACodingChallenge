namespace JHACodingChallenge.Services
{
    public interface ITwitterStreamService
    {
        Task<int> GetStreamCount(DateTime runingdate);
        Task<List<string>> CalculateTop10Hashtags(int stream_count);
    }
}