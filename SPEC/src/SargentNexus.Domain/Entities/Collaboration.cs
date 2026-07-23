namespace SargentNexus.Domain;

public sealed class Idea : AuditableEntityBase
{
    public Guid BoardId { get; set; }

    public Board Board { get; set; } = null!;

    public Guid OrganizationId { get; set; }

    public Organization Organization { get; set; } = null!;

    public Guid AuthorUserId { get; set; }

    public User AuthorUser { get; set; } = null!;

    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public Guid StatusId { get; set; }

    public Status Status { get; set; } = null!;

    public ICollection<IdeaTag> IdeaTags { get; set; } = new List<IdeaTag>();

    public ICollection<Mention> Mentions { get; set; } = new List<Mention>();

    public ICollection<Comment> Comments { get; set; } = new List<Comment>();

    public ICollection<Upvote> Upvotes { get; set; } = new List<Upvote>();
}

public sealed class Tag : EntityBase
{
    public Guid OrganizationId { get; set; }

    public Organization Organization { get; set; } = null!;

    public string Name { get; set; } = string.Empty;

    public string NormalizedName { get; set; } = string.Empty;

    public ICollection<IdeaTag> IdeaTags { get; set; } = new List<IdeaTag>();
}

public sealed class IdeaTag
{
    public Guid IdeaId { get; set; }

    public Idea Idea { get; set; } = null!;

    public Guid TagId { get; set; }

    public Tag Tag { get; set; } = null!;
}

public sealed class Mention : EntityBase
{
    public Guid OrganizationId { get; set; }

    public Organization Organization { get; set; } = null!;

    public Guid? IdeaId { get; set; }

    public Idea? Idea { get; set; }

    public Guid? CommentId { get; set; }

    public Comment? Comment { get; set; }

    public Guid MentionedUserId { get; set; }

    public User MentionedUser { get; set; } = null!;

    public string SourceText { get; set; } = string.Empty;
}

public sealed class Comment : AuditableEntityBase
{
    public Guid IdeaId { get; set; }

    public Idea Idea { get; set; } = null!;

    public Guid AuthorUserId { get; set; }

    public User AuthorUser { get; set; } = null!;

    public string Body { get; set; } = string.Empty;

    public ICollection<Mention> Mentions { get; set; } = new List<Mention>();
}

public sealed class Upvote
{
    public Guid IdeaId { get; set; }

    public Idea Idea { get; set; } = null!;

    public Guid UserId { get; set; }

    public User User { get; set; } = null!;

    public DateTime CreatedAtUtc { get; set; }
}