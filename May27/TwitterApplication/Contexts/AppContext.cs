using TwitterApplication.Models;
using Microsoft.EntityFrameworkCore;

namespace TwitterApplication.Contexts
{
    public class ClinicContext : DbContext
    {

        public ClinicContext(DbContextOptions options) : base(options)
        {

        }
        public DbSet<User> Users { get; set; }
        public DbSet<Tweet> Tweets { get; set; }
        public DbSet<Like> Likes { get; set; }
        public DbSet<HashTag> HashTags { get; set; }
        public DbSet<Follow> Follows { get; set; }
        public DbSet<TweetHashTag> TweetHashTags { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Follow>()
                .HasOne(f => f.Follower)
                .WithMany(u => u.Following)
                .HasForeignKey(f => f.FollowerId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Follow>()
                .HasOne(f => f.Following)
                .WithMany(u => u.Followers)
                .HasForeignKey(f => f.FollowingId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<TweetHashTag>()
                .HasOne(th => th.Tweet)
                .WithMany(t => t.TweetHashTags)
                .HasForeignKey(th => th.TweetId);

            modelBuilder.Entity<TweetHashTag>()
                .HasOne(th => th.HashTag)
                .WithMany(h => h.TweetHashTags)
                .HasForeignKey(th => th.HashTagId);

        }

    }
}