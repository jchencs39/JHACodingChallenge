using JHACodingChallenge.Models;

namespace JHACodingChallenge.Services
{
    public interface IHashTagService
    {
        List<string> GetTop10Hashtags(List<Hashtags> hashtaglist);
    }
}