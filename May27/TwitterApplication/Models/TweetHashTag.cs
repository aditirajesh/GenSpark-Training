using System.ComponentModel.DataAnnotations.Schema;

namespace TwitterApplication.Models
{
    public class TweetHashTag
    {

        public int Id { get; set; }

        public int TweetId { get; set; }

        [ForeignKey("TweetId")]
        public Tweet Tweet { get; set; }

        [ForeignKey("HashTagId")]
        public int HashTagId { get; set; }
        public HashTag HashTag { get; set; }
    }
}