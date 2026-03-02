using Microsoft.EntityFrameworkCore;
using MyForum.Api.Core.Entities;

namespace MyForum.Api.Infrastructure.Data;

public partial class ForumDbContext : DbContext
{
    public DbSet<Board> Boards { get; set; }
    public DbSet<Core.Entities.Thread> Threads { get; set; }
    public DbSet<Post> Posts { get; set; }
    public DbSet<PostFile> PostFiles { get; set; }
    public DbSet<Ban> Bans { get; set; }
    public DbSet<StaffAccount> StaffAccounts { get; set; }

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

            entity.HasQueryFilter(b => !b.IsHidden);
        });

        // Thread
        modelBuilder.Entity<Core.Entities.Thread>(entity =>
        {
            entity.HasIndex(t => t.BoardId);
            entity.HasIndex(t => t.IsPinned);
            entity.HasIndex(t => new { t.IsPinned, t.LastBumpAt });

            entity.HasOne(t => t.Board)
                .WithMany(b => b.Threads)
                .HasForeignKey(t => t.BoardId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasQueryFilter(t => !t.IsDeleted);
        });

        // Post
        modelBuilder.Entity<Post>(entity =>
        {
            entity.HasIndex(p => p.ThreadId);
            entity.HasIndex(p => p.CreatedAt);
            entity.HasIndex(p => p.ReplyToPostId);
            entity.HasIndex(p => new { p.ThreadId, p.IsOriginal });

            entity.HasOne(p => p.Thread)
                .WithMany(t => t.Posts)
                .HasForeignKey(p => p.ThreadId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(p => p.ReplyToPost)
                .WithMany(p => p.Replies)
                .HasForeignKey(p => p.ReplyToPostId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasQueryFilter(p => !p.IsDeleted);
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
            entity.HasIndex(x => new { x.IpAddressHash, x.BoardId })
                .IsUnique();

            entity.HasOne(b => b.Board)
                .WithMany()
                .HasForeignKey(b => b.BoardId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // StaffAccount
        modelBuilder.Entity<StaffAccount>(entity =>
        {
            entity.HasDiscriminator<string>("Discriminator")
                .HasValue<Admin>("Admin")
                .HasValue<BoardModerator>("BoardModerator");
        });

        base.OnModelCreating(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
