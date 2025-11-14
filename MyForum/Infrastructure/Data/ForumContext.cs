using Microsoft.EntityFrameworkCore;
using MyForum.Core.Entities;

namespace MyForum.Infrastructure.Data;

public partial class ForumDbContext : DbContext
{
    public DbSet<Board> Boards { get; set; }
    public DbSet<Core.Entities.Thread> Threads { get; set; }
    public DbSet<Post> Posts { get; set; }
    public DbSet<PostFile> PostFiles { get; set; }
    public DbSet<Ban> Bans { get; set; }
    public DbSet<BoardModerator> BoardModerators { get; set; }

    public ForumDbContext(DbContextOptions<ForumDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Board
        modelBuilder.Entity<Board>(entity =>
        {
            entity.HasIndex(b => b.ShortName).IsUnique();
            entity.HasIndex(b => b.Position);
            entity.Property(b => b.CreatedAt).HasDefaultValueSql("NOW()");
        });

        // Thread
        modelBuilder.Entity<Core.Entities.Thread>(entity =>
        {
            entity.HasIndex(t => t.BoardId);
            entity.HasIndex(t => t.LastBumpAt);
            entity.HasIndex(t => t.IsPinned);

            entity.HasOne(t => t.Board)
                .WithMany(b => b.Threads)
                .HasForeignKey(t => t.BoardId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(t => t.OriginalPost)
                .WithMany()
                .HasForeignKey(t => t.OriginalPostId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Post
        modelBuilder.Entity<Post>(entity =>
        {
            entity.HasIndex(p => p.ThreadId);
            entity.HasIndex(p => p.CreatedAt);
            entity.HasIndex(p => p.ReplyToPostId);

            entity.HasOne(p => p.Thread)
                .WithMany(t => t.Posts)
                .HasForeignKey(p => p.ThreadId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(p => p.ReplyToPost)
                .WithMany(p => p.Replies)
                .HasForeignKey(p => p.ReplyToPostId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // PostFile
        modelBuilder.Entity<PostFile>(entity =>
        {
            entity.HasIndex(f => f.PostId);

            entity.HasOne(f => f.Post)
                .WithMany(p => p.Files)
                .HasForeignKey(f => f.PostId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Ban
        modelBuilder.Entity<Ban>(entity =>
        {
            entity.HasIndex(b => b.IpAddress);
            entity.HasIndex(b => b.ExpiresAt);
            entity.HasIndex(b => b.IsActive);

            entity.HasOne(b => b.Board)
                .WithMany()
                .HasForeignKey(b => b.BoardId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // BoardModerator
        modelBuilder.Entity<BoardModerator>(entity =>
        {
            entity.HasIndex(m => new { m.BoardId, m.Username }).IsUnique();

            entity.HasOne(m => m.Board)
                .WithMany(b => b.Moderators)
                .HasForeignKey(m => m.BoardId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
