using JHACodingChallenge.Models;

namespace JHACodingChallenge.Services
{
    public class HashTagService : IHashTagService
    {
        public HashTagService(){ }
        public List<string> GetTop10Hashtags(List<Hashtags> hashtaglist)
        {
            List<string> topHashtag = new List<string>();
            List<IGrouping<string, Hashtags>> list;
            list = (from hash in hashtaglist
                    group hash by hash.tag into hashs
                    orderby hashs.Count() descending
                    select hashs).Take(10).ToList();

            topHashtag = (from tag in list
                          select tag.Key  ).ToList();
           
            return topHashtag;
        }
    }
}
