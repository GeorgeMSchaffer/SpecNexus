using Microsoft.EntityFrameworkCore;
using SargentNexus.Domain;

namespace SargentNexus.Infrastructure;

public sealed class SargentNexusDbContext : DbContext
{
    public SargentNexusDbContext(DbContextOptions<SargentNexusDbContext> options)
        : base(options)
    {
    }

    public DbSet<Organization> Organizations => Set<Organization>();

    public DbSet<User> Users => Set<User>();

    public DbSet<Status> Statuses => Set<Status>();

    public DbSet<Board> Boards => Set<Board>();

    public DbSet<BoardSwimlane> BoardSwimlanes => Set<BoardSwimlane>();

    public DbSet<Idea> Ideas => Set<Idea>();

    public DbSet<Tag> Tags => Set<Tag>();

    public DbSet<IdeaTag> IdeaTags => Set<IdeaTag>();

    public DbSet<Mention> Mentions => Set<Mention>();

    public DbSet<Comment> Comments => Set<Comment>();

    public DbSet<Upvote> Upvotes => Set<Upvote>();

    public DbSet<AuditEvent> AuditEvents => Set<AuditEvent>();

    public DbSet<NotificationEvent> NotificationEvents => Set<NotificationEvent>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Organization>(entity =>
        {
            entity.ToTable("organizations");
            entity.HasKey(item => item.Id);
            entity.Property(item => item.CompanyName).HasMaxLength(200).IsRequired();
            entity.Property(item => item.Address).HasMaxLength(250).IsRequired();
            entity.Property(item => item.City).HasMaxLength(100).IsRequired();
            entity.Property(item => item.State).HasMaxLength(50).IsRequired();
            entity.Property(item => item.Zip).HasMaxLength(20).IsRequired();
            entity.Property(item => item.Phone).HasMaxLength(50).IsRequired();
            entity.Property(item => item.PrimaryContactFirstName).HasMaxLength(100).IsRequired();
            entity.Property(item => item.PrimaryContactLastName).HasMaxLength(100).IsRequired();
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("users");
            entity.HasKey(item => item.Id);
            entity.Property(item => item.FirstName).HasMaxLength(100).IsRequired();
            entity.Property(item => item.LastName).HasMaxLength(100).IsRequired();
            entity.Property(item => item.Email).HasMaxLength(320).IsRequired();
            entity.Property(item => item.PasswordHash).HasMaxLength(500).IsRequired();
            entity.HasIndex(item => new { item.OrganizationId, item.Email }).IsUnique();
            entity.HasOne(item => item.Organization)
                .WithMany(item => item.Users)
                .HasForeignKey(item => item.OrganizationId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Status>(entity =>
        {
            entity.ToTable("statuses");
            entity.HasKey(item => item.Id);
            entity.Property(item => item.Name).HasMaxLength(100).IsRequired();
            entity.HasIndex(item => new { item.OrganizationId, item.Name }).IsUnique();
        });

        modelBuilder.Entity<Board>(entity =>
        {
            entity.ToTable("boards");
            entity.HasKey(item => item.Id);
            entity.Property(item => item.Name).HasMaxLength(150).IsRequired();
        });

        modelBuilder.Entity<BoardSwimlane>(entity =>
        {
            entity.ToTable("board_swimlanes");
            entity.HasKey(item => new { item.BoardId, item.StatusId });
            entity.HasIndex(item => new { item.BoardId, item.Order }).IsUnique();
        });

        modelBuilder.Entity<Idea>(entity =>
        {
            entity.ToTable("ideas");
            entity.HasKey(item => item.Id);
            entity.Property(item => item.Title).HasMaxLength(150).IsRequired();
            entity.Property(item => item.Description).HasMaxLength(4000).IsRequired();
        });

        modelBuilder.Entity<Tag>(entity =>
        {
            entity.ToTable("tags");
            entity.HasKey(item => item.Id);
            entity.Property(item => item.Name).HasMaxLength(100).IsRequired();
            entity.Property(item => item.NormalizedName).HasMaxLength(100).IsRequired();
            entity.HasIndex(item => new { item.OrganizationId, item.NormalizedName }).IsUnique();
        });

        modelBuilder.Entity<IdeaTag>(entity =>
        {
            entity.ToTable("idea_tags");
            entity.HasKey(item => new { item.IdeaId, item.TagId });
        });

        modelBuilder.Entity<Mention>(entity =>
        {
            entity.ToTable("mentions");
            entity.HasKey(item => item.Id);
            entity.Property(item => item.SourceText).HasMaxLength(500).IsRequired();
        });

        modelBuilder.Entity<Comment>(entity =>
        {
            entity.ToTable("comments");
            entity.HasKey(item => item.Id);
            entity.Property(item => item.Body).HasMaxLength(2000).IsRequired();
        });

        modelBuilder.Entity<Upvote>(entity =>
        {
            entity.ToTable("upvotes");
            entity.HasKey(item => new { item.IdeaId, item.UserId });
        });

        modelBuilder.Entity<AuditEvent>(entity =>
        {
            entity.ToTable("audit_events");
            entity.HasKey(item => item.Id);
            entity.Property(item => item.EventType).HasMaxLength(100).IsRequired();
            entity.Property(item => item.EntityType).HasMaxLength(100).IsRequired();
            entity.Property(item => item.Metadata).HasColumnType("nvarchar(max)").IsRequired();
        });

        modelBuilder.Entity<NotificationEvent>(entity =>
        {
            entity.ToTable("notification_events");
            entity.HasKey(item => item.Id);
            entity.Property(item => item.EventType).HasMaxLength(100).IsRequired();
            entity.Property(item => item.IdeaLink).HasMaxLength(500).IsRequired();
            entity.Property(item => item.Metadata).HasColumnType("nvarchar(max)").IsRequired();
        });
    }
}