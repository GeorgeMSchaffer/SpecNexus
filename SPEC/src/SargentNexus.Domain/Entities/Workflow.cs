namespace SargentNexus.Domain;

public sealed class Status : EntityBase
{
    public Guid OrganizationId { get; set; }

    public Organization Organization { get; set; } = null!;

    public string Name { get; set; } = string.Empty;

    public bool IsDeleted { get; set; }

    public ICollection<BoardSwimlane> BoardSwimlanes { get; set; } = new List<BoardSwimlane>();

    public ICollection<Idea> Ideas { get; set; } = new List<Idea>();
}

public sealed class Board : EntityBase
{
    public Guid OrganizationId { get; set; }

    public Organization Organization { get; set; } = null!;

    public string Name { get; set; } = string.Empty;

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