namespace SargentNexus.Client.Shared.Kanban;

public sealed class KanbanColumnModel
{
    public Guid StatusId { get; set; }

    public string StatusName { get; set; } = string.Empty;

    public string StatusColor { get; set; } = "#6c757d";

    public bool IsDeletedStatus { get; set; }

    public int Order { get; set; }

    public IReadOnlyList<KanbanIdeaCardModel> Cards { get; set; } = Array.Empty<KanbanIdeaCardModel>();
}

public sealed class KanbanIdeaCardModel
{
    public Guid IdeaId { get; set; }

    public Guid StatusId { get; set; }

    public Guid AuthorUserId { get; set; }

    public Guid? AssigneeUserId { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Priority { get; set; } = string.Empty;

    public string? AssigneeDisplayName { get; set; }

    public string AssigneeInitials { get; set; } = string.Empty;

    public string CreatedAtDisplay { get; set; } = string.Empty;

    public DateTime CreatedAtUtc { get; set; }

    public int UpvoteCount { get; set; }

    public IReadOnlyList<string> TagNames { get; set; } = Array.Empty<string>();
}
