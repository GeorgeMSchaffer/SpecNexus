namespace SargentNexus.Client.Workflow;

public sealed class BoardSummaryDto
{
    public Guid BoardId { get; set; }

    public Guid OrganizationId { get; set; }

    public string Name { get; set; } = string.Empty;

    public bool AllowUserStatusUpdate { get; set; }

    public IReadOnlyList<SwimlaneDto> Swimlanes { get; set; } = Array.Empty<SwimlaneDto>();
}

public sealed class SwimlaneDto
{
    public Guid StatusId { get; set; }

    public string StatusName { get; set; } = string.Empty;

    public bool IsDeletedStatus { get; set; }

    public int Order { get; set; }
}

public sealed class StatusSummaryDto
{
    public Guid StatusId { get; set; }

    public Guid OrganizationId { get; set; }

    public string Name { get; set; } = string.Empty;

    public bool IsDeleted { get; set; }
}

public sealed class PagedResultDto<T>
{
    public int Page { get; set; }

    public int PageSize { get; set; }

    public int TotalCount { get; set; }

    public IReadOnlyList<T> Items { get; set; } = Array.Empty<T>();
}

public sealed class IdeaListItemDto
{
    public Guid IdeaId { get; set; }

    public Guid BoardId { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Priority { get; set; } = string.Empty;

    public DateOnly? DueDate { get; set; }

    public Guid? AssigneeUserId { get; set; }

    public string? AssigneeDisplayName { get; set; }

    public Guid StatusId { get; set; }

    public string StatusName { get; set; } = string.Empty;

    public int UpvoteCount { get; set; }

    public Guid AuthorUserId { get; set; }

    public DateTime CreatedAtUtc { get; set; }
}

public sealed class IdeaDetailDto
{
    public Guid IdeaId { get; set; }

    public Guid BoardId { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string Priority { get; set; } = string.Empty;

    public DateOnly? DueDate { get; set; }

    public Guid? AssigneeUserId { get; set; }

    public string? AssigneeDisplayName { get; set; }

    public Guid StatusId { get; set; }

    public string StatusName { get; set; } = string.Empty;

    public IReadOnlyList<string> TagNames { get; set; } = Array.Empty<string>();

    public IReadOnlyList<string> Mentions { get; set; } = Array.Empty<string>();

    public IReadOnlyList<CommentDto> Comments { get; set; } = Array.Empty<CommentDto>();

    public int UpvoteCount { get; set; }
}

public sealed class CommentDto
{
    public Guid CommentId { get; set; }

    public Guid IdeaId { get; set; }

    public Guid AuthorUserId { get; set; }

    public string Body { get; set; } = string.Empty;

    public DateTime CreatedAtUtc { get; set; }

    public DateTime? UpdatedAtUtc { get; set; }
}

public sealed class CommentWriteRequestDto
{
    public string Body { get; set; } = string.Empty;
}

public sealed class CreateBoardRequestDto
{
    public string Name { get; set; } = string.Empty;

    public IReadOnlyList<Guid> StatusIds { get; set; } = Array.Empty<Guid>();

    public bool AllowUserStatusUpdate { get; set; }
}

public sealed class IdeaWriteRequestDto
{
    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string Priority { get; set; } = string.Empty;

    public DateOnly? DueDate { get; set; }

    public Guid? AssigneeUserId { get; set; }

    public Guid? StatusId { get; set; }

    public IReadOnlyList<string> TagNames { get; set; } = Array.Empty<string>();

    public IReadOnlyList<string> MentionEmails { get; set; } = Array.Empty<string>();
}

public sealed class UpvoteToggleResultDto
{
    public Guid IdeaId { get; set; }

    public bool HasUpvoted { get; set; }

    public int UpvoteCount { get; set; }
}

public sealed class MoveIdeaStatusRequestDto
{
    public Guid StatusId { get; set; }
}

public sealed class UpdateBoardRequestDto
{
    public string Name { get; set; } = string.Empty;

    public IReadOnlyList<Guid> StatusIds { get; set; } = Array.Empty<Guid>();

    public bool AllowUserStatusUpdate { get; set; }
}

public sealed class CreateStatusRequestDto
{
    public string Name { get; set; } = string.Empty;
}

public sealed class UpdateStatusRequestDto
{
    public string Name { get; set; } = string.Empty;
}

public sealed class ReorderSwimlanesRequestDto
{
    public IReadOnlyList<Guid> OrderedStatusIds { get; set; } = Array.Empty<Guid>();
}
