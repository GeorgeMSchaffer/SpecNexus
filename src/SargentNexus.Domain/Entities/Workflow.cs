namespace SargentNexus.Domain;

public sealed class Status : EntityBase
{
    public Guid OrganizationId { get; set; }

    public Organization Organization { get; set; } = null!;

    public string Name { get; set; } = string.Empty;

    public bool IsDeleted { get; set; }

    public string? Color { get; set; }

    public int SortOrder { get; set; }

    public bool IsDefault { get; set; }

    public ICollection<BoardSwimlane> BoardSwimlanes { get; set; } = new List<BoardSwimlane>();

    public ICollection<Idea> Ideas { get; set; } = new List<Idea>();
}

public sealed class IdeaType : EntityBase
{
    public Guid OrganizationId { get; set; }

    public Organization Organization { get; set; } = null!;

    public string Name { get; set; } = string.Empty;

    public int SortOrder { get; set; }

    public bool IsDeleted { get; set; }

    public ICollection<Idea> Ideas { get; set; } = new List<Idea>();
}

public sealed class BusinessImpact : EntityBase
{
    public Guid OrganizationId { get; set; }

    public Organization Organization { get; set; } = null!;

    public string Name { get; set; } = string.Empty;

    public string Color { get; set; } = string.Empty;

    public int SortOrder { get; set; }

    public bool IsDeleted { get; set; }

    public ICollection<Idea> Ideas { get; set; } = new List<Idea>();
}

public sealed class Board : EntityBase
{
    public Guid OrganizationId { get; set; }

    public Organization Organization { get; set; } = null!;

    public string Name { get; set; } = string.Empty;

    public bool AllowUserStatusUpdate { get; set; }

    public bool IsArchived { get; set; }

    public ICollection<BoardSwimlane> Swimlanes { get; set; } = new List<BoardSwimlane>();

    public ICollection<Idea> Ideas { get; set; } = new List<Idea>();
}

public sealed class BoardSwimlane
{
    public Guid BoardId { get; set; }

    public Board Board { get; set; } = null!;

    public Guid StatusId { get; set; }

    public Status Status { get; set; } = null!;

    public int Order { get; set; }
}