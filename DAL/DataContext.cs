﻿using DAL.Entities;
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

            modelBuilder.Entity<Avatar>().ToTable(nameof(Avatars));
            modelBuilder.Entity<Post>().ToTable(nameof(Posts));
            modelBuilder.Entity<PostContent>().ToTable(nameof(PostContent));
            modelBuilder.Entity<Comment>().ToTable(nameof(Comments));
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
        //TODO: добавить сущности лайков: для комментов, постов и контента
    }
}
