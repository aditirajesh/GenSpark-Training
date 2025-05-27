using TwitterApplication.Models;
using System.ComponentModel.DataAnnotations.Schema;

namespace TwitterApplication
{
    public class Follow
    {
        public int Id { get; set; }
        public int FollowerId { get; set; }
        public int FollowingId { get; set; }

        [ForeignKey("FollowerId")]
        public User? Follower { get; set; }

        [ForeignKey("FollowingId")]
        public User? Following { get; set; }

        public DateTime FollowedAt { get; set; } = DateTime.Now;

    }
}