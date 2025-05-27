using TwitterApplication.Models;
namespace TwitterApplication.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;

        public string Bio { get; set; }

        public string Password { get; set; }

        public bool IsActive { get; set; }


        public ICollection<Tweet>? Tweets { get; set; }

        public ICollection<Like>? Likes { get; set; }

        public ICollection<Follow>? Followers { get; set; }

        public ICollection<Follow>? Following { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;


    }
}