using DAL.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options)
        {

        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder
                .Entity<User>()
                .HasIndex(f => f.Email)
                .IsUnique();
            modelBuilder
               .Entity<User>()
               .HasIndex(f => f.Username)
               .IsUnique();
            modelBuilder
               .Entity<Relation>()
               .HasIndex(f => new { f.FollowerId, f.FollowedId })
               .IsUnique();

            modelBuilder
               .Entity<User>()
               .HasMany(x => x.Followers)
               .WithOne(y => y.Followed)
               .HasForeignKey(f => f.FollowedId)
               ;

            modelBuilder
               .Entity<User>()
               .HasMany(x => x.Followed)
               .WithOne(y => y.Follower)
               .HasForeignKey(f => f.FollowerId)
                ;

            modelBuilder.Entity<Avatar>().ToTable(nameof(Avatars));
            modelBuilder.Entity<Post>().ToTable(nameof(Posts));
            modelBuilder.Entity<PostContent>().ToTable(nameof(PostContent));
            modelBuilder.Entity<Comment>().ToTable(nameof(Comments));
            modelBuilder.Entity<Like>().ToTable(nameof(Likes));
            modelBuilder.Entity<CommentLike>().ToTable(nameof(CommentLikes));
            modelBuilder.Entity<PostLike>().ToTable(nameof(PostLikes));
            modelBuilder.Entity<ContentLike>().ToTable(nameof(ContentLikes));
            modelBuilder.Entity<Message>().ToTable(nameof(Messages));
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder.UseNpgsql(b => b.MigrationsAssembly("Api"));

        public DbSet<User> Users => Set<User>();
        public DbSet<UserSession> UserSessions => Set<UserSession>();
        public DbSet<Attach> Attaches => Set<Attach>();
        public DbSet<Avatar> Avatars => Set<Avatar>();
        public DbSet<Post> Posts => Set<Post>();
        public DbSet<PostContent> PostContent => Set<PostContent>();
        public DbSet<Comment> Comments => Set<Comment>();
        public DbSet<Like> Likes => Set<Like>();
        public DbSet<PostLike> PostLikes => Set<PostLike>();
        public DbSet<ContentLike> ContentLikes => Set<ContentLike>();
        public DbSet<CommentLike> CommentLikes => Set<CommentLike>();
        public DbSet<Relation> Relations => Set<Relation>();
        public DbSet<Message> Messages => Set<Message>();
    }
}
