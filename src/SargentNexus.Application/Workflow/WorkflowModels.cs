using System.ComponentModel.DataAnnotations;

namespace SargentNexus.Application.Workflow;

public sealed class WorkflowActorContext
{
    public Guid UserId { get; init; }

    public Guid? OrganizationId { get; init; }

    [Required]
    public string Role { get; init; } = string.Empty;
}

public sealed class StatusSummaryModel
{
    public Guid StatusId { get; init; }

    public Guid OrganizationId { get; init; }

    public string Name { get; init; } = string.Empty;

    public bool IsDeleted { get; init; }
}

public sealed class CreateStatusRequestModel
{
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;
}

public sealed class UpdateStatusRequestModel
{
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;
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

    public IReadOnlyList<SwimlaneModel> Swimlanes { get; init; } = Array.Empty<SwimlaneModel>();
}

public sealed class BoardDetailModel
{
    public Guid BoardId { get; init; }

    public Guid OrganizationId { get; init; }

    public string Name { get; init; } = string.Empty;

    public IReadOnlyList<SwimlaneModel> Swimlanes { get; init; } = Array.Empty<SwimlaneModel>();
}

public sealed class CreateBoardRequestModel
{
    [Required]
    [StringLength(150)]
    public string Name { get; set; } = string.Empty;

    [Required]
    public IReadOnlyList<Guid> StatusIds { get; set; } = Array.Empty<Guid>();
}

public sealed class UpdateBoardRequestModel
{
    [Required]
    [StringLength(150)]
    public string Name { get; set; } = string.Empty;

    [Required]
    public IReadOnlyList<Guid> StatusIds { get; set; } = Array.Empty<Guid>();
}

public sealed class ReorderSwimlanesRequestModel
{
    [Required]
    public IReadOnlyList<Guid> OrderedStatusIds { get; set; } = Array.Empty<Guid>();
}

public enum WorkflowFailureReason
{
    Unauthorized = 1,
    Forbidden = 2,
    OrganizationNotFound = 3,
    StatusNotFound = 4,
    BoardNotFound = 5,
    ValidationError = 6
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
