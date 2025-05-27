using System.ComponentModel.DataAnnotations.Schema;
using TwitterApplication.Models;
namespace TwitterApplication
{
    public class Tweet
    {
        public int Id { get; set; }
        public string Content { get; set; } = string.Empty;
        public int UserId { get; set; }

        [ForeignKey("UserId")]
        public User? User { get; set; }
        public ICollection<Like>? Likes { get; set; }

        public ICollection<HashTag>? HashTags { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public ICollection<TweetHashTag> TweetHashTags { get; set; }


    }
}