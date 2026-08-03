namespace SargentNexus.Client.Administration;

public sealed class PagedResultDto<TItem>
{
    public IReadOnlyList<TItem> Items { get; set; } = Array.Empty<TItem>();

    public int Page { get; set; }

    public int PageSize { get; set; }

    public int TotalCount { get; set; }

    public string SortBy { get; set; } = string.Empty;

    public string SortDirection { get; set; } = string.Empty;
}

public sealed class OrganizationListItemDto
{
    public Guid OrganizationId { get; set; }

    public string CompanyName { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string City { get; set; } = string.Empty;

    public string State { get; set; } = string.Empty;

    public string Phone { get; set; } = string.Empty;

    public string? LogoThumbnailUrl { get; set; }

    public string? InviteCode { get; set; }

    public bool IsArchived { get; set; }
}

public sealed class OrganizationUpsertRequestDto
{
    public string CompanyName { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string Address { get; set; } = string.Empty;

    public string City { get; set; } = string.Empty;

    public string State { get; set; } = string.Empty;

    public string Zip { get; set; } = string.Empty;

    public string Phone { get; set; } = string.Empty;

    public string PrimaryContactFirstName { get; set; } = string.Empty;

    public string PrimaryContactLastName { get; set; } = string.Empty;
}

public sealed class OrganizationCreateResponseDto
{
    public Guid OrganizationId { get; set; }

    public Guid DefaultBoardId { get; set; }

    public int DefaultStatusCount { get; set; }
}

public sealed class OrganizationDetailDto
{
    public Guid OrganizationId { get; set; }

    public string CompanyName { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string Address { get; set; } = string.Empty;

    public string City { get; set; } = string.Empty;

    public string State { get; set; } = string.Empty;

    public string Zip { get; set; } = string.Empty;

    public string Phone { get; set; } = string.Empty;

    public string PrimaryContactFirstName { get; set; } = string.Empty;

    public string PrimaryContactLastName { get; set; } = string.Empty;

    public string? LogoUrl { get; set; }

    public string? LogoThumbnailUrl { get; set; }

    public int? LogoHeightPx { get; set; }

    public bool IsArchived { get; set; }

    public string? InviteCode { get; set; }
}

public sealed class InviteCodeResponseDto
{
    public string InviteCode { get; set; } = string.Empty;
}

public sealed class UserListItemDto
{
    public Guid UserId { get; set; }

    public Guid? OrganizationId { get; set; }

    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string Role { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;
}

public sealed class UserDetailDto
{
    public Guid UserId { get; set; }

    public Guid? OrganizationId { get; set; }

    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string Role { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;
}

public sealed class UserCreateRequestDto
{
    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string Role { get; set; } = string.Empty;

    public string InitialPassword { get; set; } = string.Empty;

    public string? Status { get; set; }
}

public sealed class UserCreateResponseDto
{
    public Guid UserId { get; set; }

    public Guid OrganizationId { get; set; }

    public string Email { get; set; } = string.Empty;

    public string Role { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;
}

public sealed class UserImportResponseDto
{
    public Guid OrganizationId { get; set; }

    public int CreatedCount { get; set; }
}

public sealed class UserUpdateRequestDto
{
    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string Role { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;
}
