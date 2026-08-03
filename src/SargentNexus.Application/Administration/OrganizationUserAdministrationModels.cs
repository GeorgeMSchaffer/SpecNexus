using System.ComponentModel.DataAnnotations;
using SargentNexus.Domain;

namespace SargentNexus.Application.Administration;

public sealed class PagedResultModel<TItem>
{
    public IReadOnlyList<TItem> Items { get; init; } = Array.Empty<TItem>();

    public int Page { get; init; }

    public int PageSize { get; init; }

    public int TotalCount { get; init; }

    public string SortBy { get; init; } = string.Empty;

    public string SortDirection { get; init; } = string.Empty;
}

public sealed class OrganizationListQueryModel
{
    [Range(1, int.MaxValue)]
    public int Page { get; set; } = 1;

    [Range(1, 200)]
    public int PageSize { get; set; } = 25;

    public string? Search { get; set; }

    public bool? IsArchived { get; set; }

    public string? SortBy { get; set; }

    public string? SortDirection { get; set; }
}

public sealed class OrganizationListItemModel
{
    public Guid OrganizationId { get; init; }

    public string CompanyName { get; init; } = string.Empty;

    public string? Description { get; init; }

    public string City { get; init; } = string.Empty;

    public string State { get; init; } = string.Empty;

    public string Phone { get; init; } = string.Empty;

    public string InviteCode { get; init; } = string.Empty;

    public string? LogoThumbnailUrl { get; init; }

    public bool IsArchived { get; init; }
}

public sealed class OrganizationUpsertRequestModel
{
    [Required]
    [MaxLength(200)]
    public string CompanyName { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    [Required]
    [MaxLength(200)]
    public string Address { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string City { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    public string State { get; set; } = string.Empty;

    [Required]
    [MaxLength(20)]
    public string Zip { get; set; } = string.Empty;

    [Required]
    [MaxLength(25)]
    public string Phone { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string PrimaryContactFirstName { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string PrimaryContactLastName { get; set; } = string.Empty;
}

public sealed class OrganizationCreateResponseModel
{
    public Guid OrganizationId { get; init; }

    public Guid DefaultBoardId { get; init; }

    public int DefaultStatusCount { get; init; }

    public string InviteCode { get; init; } = string.Empty;
}

public sealed class OrganizationDetailModel
{
    public Guid OrganizationId { get; init; }

    public string CompanyName { get; init; } = string.Empty;

    public string? Description { get; init; }

    public string Address { get; init; } = string.Empty;

    public string City { get; init; } = string.Empty;

    public string State { get; init; } = string.Empty;

    public string Zip { get; init; } = string.Empty;

    public string Phone { get; init; } = string.Empty;

    public string PrimaryContactFirstName { get; init; } = string.Empty;

    public string PrimaryContactLastName { get; init; } = string.Empty;

    public string InviteCode { get; init; } = string.Empty;

    public string? LogoUrl { get; init; }

    public string? LogoThumbnailUrl { get; init; }

    public int? LogoHeightPx { get; init; }

    public bool IsArchived { get; init; }
}

public sealed class OrganizationUsersListQueryModel
{
    [Range(1, int.MaxValue)]
    public int Page { get; set; } = 1;

    [Range(1, 200)]
    public int PageSize { get; set; } = 25;

    public string? Search { get; set; }

    public string? Role { get; set; }

    public string? Status { get; set; }

    public string? SortBy { get; set; }

    public string? SortDirection { get; set; }
}

public sealed class UserSummaryModel
{
    public Guid UserId { get; init; }

    public Guid? OrganizationId { get; init; }

    public string FirstName { get; init; } = string.Empty;

    public string LastName { get; init; } = string.Empty;

    public string Email { get; init; } = string.Empty;

    public string Role { get; init; } = string.Empty;

    public string Status { get; init; } = string.Empty;
}

public sealed class UserDetailModel
{
    public Guid UserId { get; init; }

    public Guid? OrganizationId { get; init; }

    public string FirstName { get; init; } = string.Empty;

    public string LastName { get; init; } = string.Empty;

    public string Email { get; init; } = string.Empty;

    public string Role { get; init; } = string.Empty;

    public string Status { get; init; } = string.Empty;
}

public sealed class UserCreateRequestModel
{
    [Required]
    [MaxLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string LastName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Role { get; set; } = string.Empty;

    [Required]
    public string InitialPassword { get; set; } = string.Empty;

    public string? Status { get; set; }
}

public sealed class UserCreateResponseModel
{
    public Guid UserId { get; init; }

    public Guid OrganizationId { get; init; }

    public string Email { get; init; } = string.Empty;

    public string Role { get; init; } = string.Empty;

    public string Status { get; init; } = string.Empty;
}

public sealed class UserImportResponseModel
{
    public Guid OrganizationId { get; init; }

    public int CreatedCount { get; init; }
}

public sealed class UserUpdateRequestModel
{
    [Required]
    [MaxLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string LastName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Role { get; set; } = string.Empty;

    [Required]
    public string Status { get; set; } = string.Empty;
}

public enum AdministrationFailureReason
{
    Forbidden = 1,
    NotFound = 2,
    ValidationFailed = 3
}
public sealed class AdministrationResult
{
    private AdministrationResult(bool succeeded, AdministrationFailureReason? failureReason, IReadOnlyDictionary<string, string[]> errors)
    {
        Succeeded = succeeded;
        FailureReason = failureReason;
        Errors = errors;
    }

    public bool Succeeded { get; }

    public AdministrationFailureReason? FailureReason { get; }

    public IReadOnlyDictionary<string, string[]> Errors { get; }

    public static AdministrationResult Success() =>
        new(true, null, new Dictionary<string, string[]>());

    public static AdministrationResult Fail(AdministrationFailureReason reason, IReadOnlyDictionary<string, string[]>? errors = null) =>
        new(false, reason, errors ?? new Dictionary<string, string[]>());
}

public sealed class AdministrationResult<T>
{
    private AdministrationResult(bool succeeded, T? value, AdministrationFailureReason? failureReason, IReadOnlyDictionary<string, string[]> errors)
    {
        Succeeded = succeeded;
        Value = value;
        FailureReason = failureReason;
        Errors = errors;
    }

    public bool Succeeded { get; }

    public T? Value { get; }

    public AdministrationFailureReason? FailureReason { get; }

    public IReadOnlyDictionary<string, string[]> Errors { get; }

    public static AdministrationResult<T> Success(T value) =>
        new(true, value, null, new Dictionary<string, string[]>());

    public static AdministrationResult<T> Fail(AdministrationFailureReason reason, IReadOnlyDictionary<string, string[]>? errors = null) =>
        new(false, default, reason, errors ?? new Dictionary<string, string[]>());
}

public sealed class UserSearchFilters
{
    public string? Search { get; init; }

    public UserRole? Role { get; init; }

    public UserLifecycleStatus? Status { get; init; }

    public string SortBy { get; init; } = "lastName";

    public string SortDirection { get; init; } = "asc";

    public int Page { get; init; }

    public int PageSize { get; init; }
}
