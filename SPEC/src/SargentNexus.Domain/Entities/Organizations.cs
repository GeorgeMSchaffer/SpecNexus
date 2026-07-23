namespace SargentNexus.Domain;

public sealed class Organization : EntityBase
{
    public string CompanyName { get; set; } = string.Empty;

    public string Address { get; set; } = string.Empty;

    public string City { get; set; } = string.Empty;

    public string State { get; set; } = string.Empty;

    public string Zip { get; set; } = string.Empty;

    public string Phone { get; set; } = string.Empty;

    public string PrimaryContactFirstName { get; set; } = string.Empty;

    public string PrimaryContactLastName { get; set; } = string.Empty;

    public bool IsArchived { get; set; }

    public ICollection<User> Users { get; set; } = new List<User>();

    public ICollection<Status> Statuses { get; set; } = new List<Status>();

    public ICollection<Board> Boards { get; set; } = new List<Board>();

    public ICollection<Idea> Ideas { get; set; } = new List<Idea>();
}

public sealed class User : EntityBase
{
    public Guid? OrganizationId { get; set; }

    public Organization? Organization { get; set; }

    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string PasswordHash { get; set; } = string.Empty;

    public UserRole Role { get; set; }

    public UserLifecycleStatus Status { get; set; }

    public bool MustChangePassword { get; set; }

    public ICollection<Idea> AuthoredIdeas { get; set; } = new List<Idea>();

    public ICollection<Comment> Comments { get; set; } = new List<Comment>();

    public ICollection<Upvote> Upvotes { get; set; } = new List<Upvote>();
}