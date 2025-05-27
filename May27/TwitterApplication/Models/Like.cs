using System.ComponentModel.DataAnnotations.Schema;
using TwitterApplication.Models;
namespace TwitterApplication.Models
{
    public class Like
    {
        public int Id { get; set; }
        public int TweetId { get; set; }
        public int UserId { get; set; } //this indicates the user that liked the post

        [ForeignKey("TweetId")]
        public Tweet? Tweet { get; set; }

        [ForeignKey("UserId")]

        public User? User { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;


    }
}