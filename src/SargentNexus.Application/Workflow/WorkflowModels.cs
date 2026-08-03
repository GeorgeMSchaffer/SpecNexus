using System.ComponentModel.DataAnnotations;

namespace SargentNexus.Application.Workflow;

public sealed class WorkflowActorContext
{
    public Guid UserId { get; init; }

    public Guid? OrganizationId { get; init; }

    public string Role { get; init; } = string.Empty;
}

public sealed class StatusSummaryModel
{
    public Guid StatusId { get; init; }

    public Guid OrganizationId { get; init; }

    public string Name { get; init; } = string.Empty;

    public bool IsDeleted { get; init; }

    public string? Color { get; init; }

    public int SortOrder { get; init; }

    public bool IsDefault { get; init; }
}

public sealed class CreateStatusRequestModel
{
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [StringLength(20)]
    public string? Color { get; set; }

    public int SortOrder { get; set; }

    public bool IsDefault { get; set; }
}

public sealed class UpdateStatusRequestModel
{
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [StringLength(20)]
    public string? Color { get; set; }

    public int SortOrder { get; set; }

    public bool IsDefault { get; set; }
}

public sealed class SwimlaneModel
{
    public Guid StatusId { get; init; }

    public string StatusName { get; init; } = string.Empty;

    public bool IsDeletedStatus { get; init; }

    public int Order { get; init; }
}

public sealed class BoardSummaryModel
{
    public Guid BoardId { get; init; }

    public Guid OrganizationId { get; init; }

    public string Name { get; init; } = string.Empty;

    public bool AllowUserStatusUpdate { get; init; }

    public bool IsArchived { get; init; }

    public IReadOnlyList<SwimlaneModel> Swimlanes { get; init; } = Array.Empty<SwimlaneModel>();
}

public sealed class BoardDetailModel
{
    public Guid BoardId { get; init; }

    public Guid OrganizationId { get; init; }

    public string Name { get; init; } = string.Empty;

    public bool AllowUserStatusUpdate { get; init; }

    public IReadOnlyList<SwimlaneModel> Swimlanes { get; init; } = Array.Empty<SwimlaneModel>();
}

public sealed class CreateBoardRequestModel
{
    [Required]
    [StringLength(150)]
    public string Name { get; set; } = string.Empty;

    [Required]
    public IReadOnlyList<Guid> StatusIds { get; set; } = Array.Empty<Guid>();

    public bool AllowUserStatusUpdate { get; set; }
}

public sealed class UpdateBoardRequestModel
{
    [Required]
    [StringLength(150)]
    public string Name { get; set; } = string.Empty;

    [Required]
    public IReadOnlyList<Guid> StatusIds { get; set; } = Array.Empty<Guid>();

    public bool AllowUserStatusUpdate { get; set; }
}

public sealed class ReorderSwimlanesRequestModel
{
    [Required]
    public IReadOnlyList<Guid> OrderedStatusIds { get; set; } = Array.Empty<Guid>();
}

public sealed class PagedResultModel<T>
{
    public int Page { get; init; }

    public int PageSize { get; init; }

    public int TotalCount { get; init; }

    public IReadOnlyList<T> Items { get; init; } = Array.Empty<T>();
}

public sealed class IdeaListQueryModel
{
    [Range(1, int.MaxValue)]
    public int Page { get; set; } = 1;

    [Range(1, 100)]
    public int PageSize { get; set; } = 20;

    public string? Search { get; set; }

    public Guid? StatusId { get; set; }

    public string? Tag { get; set; }

    public string? Priority { get; set; }

    public DateOnly? DueBefore { get; set; }

    public string? SortBy { get; set; }

    public string? SortDirection { get; set; }
}

public sealed class CommentListQueryModel
{
    [Range(1, int.MaxValue)]
    public int Page { get; set; } = 1;

    [Range(1, 100)]
    public int PageSize { get; set; } = 20;

    public string? SortDirection { get; set; }
}

public sealed class TagAutocompleteQueryModel
{
    public string Search { get; set; } = string.Empty;

    [Range(1, 50)]
    public int Limit { get; set; } = 10;
}

public sealed class IdeaListItemModel
{
    public Guid IdeaId { get; init; }

    public Guid BoardId { get; init; }

    public string Title { get; init; } = string.Empty;

    public string Priority { get; init; } = string.Empty;

    public DateOnly? DueDate { get; init; }

    public Guid? AssigneeUserId { get; init; }

    public string? AssigneeDisplayName { get; init; }

    public Guid StatusId { get; init; }

    public string StatusName { get; init; } = string.Empty;

    public int UpvoteCount { get; init; }

    public Guid AuthorUserId { get; init; }

    public string? AuthorDisplayName { get; init; }

    public DateTime CreatedAtUtc { get; init; }
}

public sealed class IdeaDetailModel
{
    public Guid IdeaId { get; init; }

    public Guid BoardId { get; init; }

    public string Title { get; init; } = string.Empty;

    public string Description { get; init; } = string.Empty;

    public string Priority { get; init; } = string.Empty;

    public DateOnly? DueDate { get; init; }

    public Guid? AssigneeUserId { get; init; }

    public string? AssigneeDisplayName { get; init; }

    public Guid StatusId { get; init; }

    public string StatusName { get; init; } = string.Empty;

    public string ApprovalState { get; init; } = string.Empty;

    public Guid? PendingApprovalTargetStatusId { get; init; }

    public Guid? PendingApprovalPreviousStatusId { get; init; }

    public DateTime? PendingApprovalRequestedAtUtc { get; init; }

    public DateTime? PendingApprovalExpiresAtUtc { get; init; }

    public IReadOnlyList<string> TagNames { get; init; } = Array.Empty<string>();

    public IReadOnlyList<string> Mentions { get; init; } = Array.Empty<string>();

    public IReadOnlyList<CommentModel> Comments { get; init; } = Array.Empty<CommentModel>();

    public int UpvoteCount { get; init; }
}

public sealed class IdeaWriteRequestModel
{
    [Required]
    [StringLength(150)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [StringLength(4000)]
    public string Description { get; set; } = string.Empty;

    [Required]
    [RegularExpression("Low|Medium|High|Critical")]
    public string Priority { get; set; } = string.Empty;

    public DateOnly? DueDate { get; set; }

    public Guid? AssigneeUserId { get; set; }

    public Guid? StatusId { get; set; }

    public IReadOnlyList<string> TagNames { get; set; } = Array.Empty<string>();

    public IReadOnlyList<string> MentionEmails { get; set; } = Array.Empty<string>();
}

public sealed class MoveIdeaStatusRequestModel
{
    [Required]
    public Guid StatusId { get; set; }

    public bool SubmitForApproval { get; set; }

    public bool Approve { get; set; }

    public bool Reject { get; set; }
}

public sealed class CommentModel
{
    public Guid CommentId { get; init; }

    public Guid IdeaId { get; init; }

    public Guid AuthorUserId { get; init; }

    public string Body { get; init; } = string.Empty;

    public DateTime CreatedAtUtc { get; init; }

    public DateTime? UpdatedAtUtc { get; init; }
}

public sealed class CommentWriteRequestModel
{
    [Required]
    [StringLength(2000)]
    public string Body { get; set; } = string.Empty;
}

public sealed class UpvoteToggleResultModel
{
    public Guid IdeaId { get; init; }

    public bool HasUpvoted { get; init; }

    public int UpvoteCount { get; init; }
}

public enum WorkflowFailureReason
{
    Unauthorized = 1,
    Forbidden = 2,
    OrganizationNotFound = 3,
    StatusNotFound = 4,
    BoardNotFound = 5,
    ValidationError = 6,
    IdeaNotFound = 7,
    CommentNotFound = 8
}

public sealed class WorkflowResult<T>
{
    private WorkflowResult(bool succeeded, T? response, WorkflowFailureReason? failureReason, IReadOnlyList<string>? errors)
    {
        Succeeded = succeeded;
        Response = response;
        FailureReason = failureReason;
        Errors = errors ?? Array.Empty<string>();
    }

    public bool Succeeded { get; }

    public T? Response { get; }

    public WorkflowFailureReason? FailureReason { get; }

    public IReadOnlyList<string> Errors { get; }

    public static WorkflowResult<T> Success(T response)
    {
        return new WorkflowResult<T>(true, response, null, null);
    }

    public static WorkflowResult<T> Failure(WorkflowFailureReason failureReason, IReadOnlyList<string>? errors = null)
    {
        return new WorkflowResult<T>(false, default, failureReason, errors);
    }
}

public sealed class WorkflowResult
{
    private WorkflowResult(bool succeeded, WorkflowFailureReason? failureReason, IReadOnlyList<string>? errors)
    {
        Succeeded = succeeded;
        FailureReason = failureReason;
        Errors = errors ?? Array.Empty<string>();
    }

    public bool Succeeded { get; }

    public WorkflowFailureReason? FailureReason { get; }

    public IReadOnlyList<string> Errors { get; }

    public static WorkflowResult Success()
    {
        return new WorkflowResult(true, null, null);
    }

    public static WorkflowResult Failure(WorkflowFailureReason failureReason, IReadOnlyList<string>? errors = null)
    {
        return new WorkflowResult(false, failureReason, errors);
    }
}
