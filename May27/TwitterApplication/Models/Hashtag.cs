using TwitterApplication.Models;
namespace TwitterApplication
{
    public class HashTag
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public ICollection<TweetHashTag> TweetHashTags { get; set; }


    }
}