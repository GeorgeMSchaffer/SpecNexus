namespace SargentNexus.Client.Workflow;

public sealed class BoardSummaryDto
{
    public Guid BoardId { get; set; }

    public Guid OrganizationId { get; set; }

    public string Name { get; set; } = string.Empty;

    public bool AllowUserStatusUpdate { get; set; }

    public bool IsArchived { get; set; }

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

    public string? Color { get; set; }

    public int SortOrder { get; set; }

    public bool IsDefault { get; set; }
}

public sealed class IdeaTypeSummaryDto
{
    public Guid IdeaTypeId { get; set; }

    public Guid OrganizationId { get; set; }

    public string Name { get; set; } = string.Empty;

    public int SortOrder { get; set; }

    public bool IsDeleted { get; set; }
}

public sealed class IdeaTypeWriteRequestDto
{
    public string Name { get; set; } = string.Empty;
}

public sealed class ReorderIdeaTypesRequestDto
{
    public IReadOnlyList<Guid> OrderedIdeaTypeIds { get; set; } = Array.Empty<Guid>();
}

public sealed class BusinessImpactSummaryDto
{
    public Guid BusinessImpactId { get; set; }

    public Guid OrganizationId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Color { get; set; } = string.Empty;

    public int SortOrder { get; set; }

    public bool IsDeleted { get; set; }
}

public sealed class BusinessImpactWriteRequestDto
{
    public string Name { get; set; } = string.Empty;

    public string Color { get; set; } = string.Empty;
}

public sealed class ReorderBusinessImpactsRequestDto
{
    public IReadOnlyList<Guid> OrderedBusinessImpactIds { get; set; } = Array.Empty<Guid>();
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

    public Guid IdeaTypeId { get; set; }

    public string IdeaTypeName { get; set; } = string.Empty;

    public Guid BusinessImpactId { get; set; }

    public string BusinessImpactName { get; set; } = string.Empty;

    public string BusinessImpactColor { get; set; } = string.Empty;

    public DateOnly? DueDate { get; set; }

    public IReadOnlyList<IdeaAssigneeSummaryDto> Assignees { get; set; } = Array.Empty<IdeaAssigneeSummaryDto>();

    public IReadOnlyList<string> TagNames { get; set; } = Array.Empty<string>();

    public Guid StatusId { get; set; }

    public string StatusName { get; set; } = string.Empty;

    public int UpvoteCount { get; set; }

    public bool HasUpvoted { get; set; }

    public int CommentCount { get; set; }

    public Guid AuthorUserId { get; set; }

    public string? AuthorDisplayName { get; set; }

    public DateTime CreatedAtUtc { get; set; }
}

public sealed class IdeaDetailDto
{
    public Guid IdeaId { get; set; }

    public Guid BoardId { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string Priority { get; set; } = string.Empty;

    public Guid IdeaTypeId { get; set; }

    public string IdeaTypeName { get; set; } = string.Empty;

    public Guid BusinessImpactId { get; set; }

    public string BusinessImpactName { get; set; } = string.Empty;

    public string BusinessImpactColor { get; set; } = string.Empty;

    public DateOnly? DueDate { get; set; }

    public IReadOnlyList<IdeaAssigneeSummaryDto> Assignees { get; set; } = Array.Empty<IdeaAssigneeSummaryDto>();

    public Guid StatusId { get; set; }

    public string StatusName { get; set; } = string.Empty;

    public IReadOnlyList<string> TagNames { get; set; } = Array.Empty<string>();

    public IReadOnlyList<string> Mentions { get; set; } = Array.Empty<string>();

    public IReadOnlyList<CommentDto> Comments { get; set; } = Array.Empty<CommentDto>();

    public int UpvoteCount { get; set; }

    public bool HasUpvoted { get; set; }

    public int CommentCount { get; set; }
}

public sealed class IdeaAssigneeSummaryDto
{
    public Guid UserId { get; set; }

    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public string DisplayName { get; set; } = string.Empty;

    public bool IsActive { get; set; }
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

    public Guid IdeaTypeId { get; set; }

    public Guid BusinessImpactId { get; set; }

    public DateOnly? DueDate { get; set; }

    public IReadOnlyList<Guid> AssigneeUserIds { get; set; } = Array.Empty<Guid>();

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

    public string? Color { get; set; }

    public int SortOrder { get; set; }

    public bool IsDefault { get; set; }
}

public sealed class UpdateStatusRequestDto
{
    public string Name { get; set; } = string.Empty;

    public string? Color { get; set; }

    public int SortOrder { get; set; }

    public bool IsDefault { get; set; }
}

public sealed class ReorderSwimlanesRequestDto
{
    public IReadOnlyList<Guid> OrderedStatusIds { get; set; } = Array.Empty<Guid>();
}
