using System.ComponentModel.DataAnnotations;
using System.Text;
using SargentNexus.Application.Auth;
using SargentNexus.Domain;

namespace SargentNexus.Application.Administration;

public interface IOrganizationUserAdministrationStore
{
    Task<Organization?> FindOrganizationByIdAsync(Guid organizationId, CancellationToken cancellationToken);

    Task<Organization?> FindOrganizationByInviteCodeAsync(string normalizedCode, CancellationToken cancellationToken);

    Task<PagedResultModel<OrganizationListItemModel>> GetOrganizationsAsync(
        string? search,
        bool includeArchived,
        string sortBy,
        string sortDirection,
        int page,
        int pageSize,
        CancellationToken cancellationToken);

    Task<(Guid BoardId, int StatusCount)> AddOrganizationWithDefaultsAsync(Organization organization, CancellationToken cancellationToken);

    Task<Organization?> FindUserOrganizationAsync(Guid userId, CancellationToken cancellationToken);

    Task<User?> FindUserByIdAsync(Guid userId, CancellationToken cancellationToken);

    Task<User?> FindUserByEmailAsync(string normalizedEmail, CancellationToken cancellationToken);

    Task<int> CountOrgAdminsAsync(Guid organizationId, CancellationToken cancellationToken);

    Task<PagedResultModel<UserSummaryModel>> GetUsersByOrganizationAsync(Guid organizationId, UserSearchFilters filters, CancellationToken cancellationToken);

    Task AddUserAsync(User user, CancellationToken cancellationToken);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}

public interface IOrganizationUserAuditWriter
{
    Task WriteOrganizationCreatedAsync(Guid actorUserId, Organization organization, Guid defaultBoardId, int defaultStatusCount, CancellationToken cancellationToken);

    Task WriteOrganizationUpdatedAsync(Guid actorUserId, Organization organization, CancellationToken cancellationToken);

    Task WriteOrganizationArchivedAsync(Guid actorUserId, Organization organization, CancellationToken cancellationToken);

    Task WriteUserCreatedAsync(Guid actorUserId, User user, CancellationToken cancellationToken);

    Task WriteUserUpdatedAsync(Guid actorUserId, User user, string previousRole, string previousStatus, CancellationToken cancellationToken);
}

public interface IOrganizationUserAdministrationService
{
    Task<AdministrationResult<PagedResultModel<OrganizationListItemModel>>> ListOrganizationsAsync(Guid actorUserId, OrganizationListQueryModel request, CancellationToken cancellationToken);

    Task<AdministrationResult<OrganizationCreateResponseModel>> CreateOrganizationAsync(Guid actorUserId, OrganizationUpsertRequestModel request, CancellationToken cancellationToken);

    Task<AdministrationResult<OrganizationDetailModel>> GetOrganizationAsync(Guid actorUserId, Guid organizationId, CancellationToken cancellationToken);

    Task<AdministrationResult<OrganizationDetailModel>> UpdateOrganizationAsync(Guid actorUserId, Guid organizationId, OrganizationUpsertRequestModel request, CancellationToken cancellationToken);

    Task<AdministrationResult> ArchiveOrganizationAsync(Guid actorUserId, Guid organizationId, CancellationToken cancellationToken);

    Task<AdministrationResult<string>> RegenerateInviteCodeAsync(Guid actorUserId, Guid organizationId, CancellationToken cancellationToken);

    Task<AdministrationResult<PagedResultModel<UserSummaryModel>>> ListUsersAsync(Guid actorUserId, Guid organizationId, OrganizationUsersListQueryModel request, CancellationToken cancellationToken);

    Task<AdministrationResult<UserCreateResponseModel>> CreateUserAsync(Guid actorUserId, Guid organizationId, UserCreateRequestModel request, CancellationToken cancellationToken);

    Task<AdministrationResult<string>> GetUserImportTemplateAsync(Guid actorUserId, Guid organizationId, CancellationToken cancellationToken);

    Task<AdministrationResult<UserImportResponseModel>> ImportUsersCsvAsync(
        Guid actorUserId,
        Guid organizationId,
        byte[] csvBytes,
        CancellationToken cancellationToken);

    Task<AdministrationResult<UserDetailModel>> GetUserAsync(Guid actorUserId, Guid userId, CancellationToken cancellationToken);

    Task<AdministrationResult<UserDetailModel>> UpdateUserAsync(Guid actorUserId, Guid userId, UserUpdateRequestModel request, CancellationToken cancellationToken);
}

public sealed class OrganizationUserAdministrationService : IOrganizationUserAdministrationService
{
    private static readonly string[] DefaultStatuses =
    {
        "New / Pending",
        "In Review",
        "In Progress",
        "Client Review",
        "Complete"
    };

    private readonly IOrganizationUserAdministrationStore _store;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IPasswordPolicyValidator _passwordPolicyValidator;
    private readonly IOrganizationUserAuditWriter _auditWriter;

    public OrganizationUserAdministrationService(
        IOrganizationUserAdministrationStore store,
        IPasswordHasher passwordHasher,
        IPasswordPolicyValidator passwordPolicyValidator,
        IOrganizationUserAuditWriter auditWriter)
    {
        _store = store;
        _passwordHasher = passwordHasher;
        _passwordPolicyValidator = passwordPolicyValidator;
        _auditWriter = auditWriter;
    }

    public async Task<AdministrationResult<PagedResultModel<OrganizationListItemModel>>> ListOrganizationsAsync(
        Guid actorUserId,
        OrganizationListQueryModel request,
        CancellationToken cancellationToken)
    {
        var actor = await _store.FindUserByIdAsync(actorUserId, cancellationToken);

        if (actor is null)
        {
            return AdministrationResult<PagedResultModel<OrganizationListItemModel>>.Fail(AdministrationFailureReason.Forbidden);
        }

        if (actor.Role != UserRole.SiteAdmin)
        {
            return AdministrationResult<PagedResultModel<OrganizationListItemModel>>.Fail(AdministrationFailureReason.Forbidden);
        }

        var sortBy = NormalizeOrganizationSortBy(request.SortBy);
        var sortDirection = NormalizeSortDirection(request.SortDirection);
        var page = request.Page <= 0 ? 1 : request.Page;
        var pageSize = request.PageSize <= 0 ? 25 : request.PageSize;
        var includeArchived = request.IsArchived ?? false;

        var result = await _store.GetOrganizationsAsync(
            request.Search,
            includeArchived,
            sortBy,
            sortDirection,
            page,
            pageSize,
            cancellationToken);

        return AdministrationResult<PagedResultModel<OrganizationListItemModel>>.Success(result);
    }

    public async Task<AdministrationResult<OrganizationCreateResponseModel>> CreateOrganizationAsync(
        Guid actorUserId,
        OrganizationUpsertRequestModel request,
        CancellationToken cancellationToken)
    {
        var actor = await _store.FindUserByIdAsync(actorUserId, cancellationToken);

        if (actor is null || actor.Role != UserRole.SiteAdmin)
        {
            return AdministrationResult<OrganizationCreateResponseModel>.Fail(AdministrationFailureReason.Forbidden);
        }

        var validationErrors = ValidateOrganizationRequest(request);

        if (validationErrors.Count > 0)
        {
            return AdministrationResult<OrganizationCreateResponseModel>.Fail(AdministrationFailureReason.ValidationFailed, validationErrors);
        }

        var organization = new Organization
        {
            Id = Guid.NewGuid(),
            CompanyName = request.CompanyName.Trim(),
            Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim(),
            Address = request.Address.Trim(),
            City = request.City.Trim(),
            State = request.State.Trim(),
            Zip = request.Zip.Trim(),
            Phone = request.Phone.Trim(),
            PrimaryContactFirstName = request.PrimaryContactFirstName.Trim(),
            PrimaryContactLastName = request.PrimaryContactLastName.Trim(),
            InviteCode = GenerateInviteCode(),
            InviteCodeGeneratedAtUtc = DateTime.UtcNow,
            IsArchived = false
        };

        var (defaultBoardId, statusCount) = await _store.AddOrganizationWithDefaultsAsync(organization, cancellationToken);
        await _auditWriter.WriteOrganizationCreatedAsync(actor.Id, organization, defaultBoardId, statusCount, cancellationToken);

        return AdministrationResult<OrganizationCreateResponseModel>.Success(new OrganizationCreateResponseModel
        {
            OrganizationId = organization.Id,
            DefaultBoardId = defaultBoardId,
            DefaultStatusCount = statusCount,
            InviteCode = organization.InviteCode
        });
    }

    public async Task<AdministrationResult<OrganizationDetailModel>> GetOrganizationAsync(
        Guid actorUserId,
        Guid organizationId,
        CancellationToken cancellationToken)
    {
        var actor = await _store.FindUserByIdAsync(actorUserId, cancellationToken);

        if (actor is null)
        {
            return AdministrationResult<OrganizationDetailModel>.Fail(AdministrationFailureReason.Forbidden);
        }

        var organization = await _store.FindOrganizationByIdAsync(organizationId, cancellationToken);

        if (organization is null)
        {
            return AdministrationResult<OrganizationDetailModel>.Fail(AdministrationFailureReason.NotFound);
        }

        if (!CanAccessOrganization(actor, organization.Id))
        {
            return AdministrationResult<OrganizationDetailModel>.Fail(AdministrationFailureReason.Forbidden);
        }

        return AdministrationResult<OrganizationDetailModel>.Success(ToOrganizationDetail(organization));
    }

    public async Task<AdministrationResult<OrganizationDetailModel>> UpdateOrganizationAsync(
        Guid actorUserId,
        Guid organizationId,
        OrganizationUpsertRequestModel request,
        CancellationToken cancellationToken)
    {
        var actor = await _store.FindUserByIdAsync(actorUserId, cancellationToken);

        if (actor is null)
        {
            return AdministrationResult<OrganizationDetailModel>.Fail(AdministrationFailureReason.Forbidden);
        }

        var organization = await _store.FindOrganizationByIdAsync(organizationId, cancellationToken);

        if (organization is null)
        {
            return AdministrationResult<OrganizationDetailModel>.Fail(AdministrationFailureReason.NotFound);
        }

        if (!CanAccessOrganization(actor, organization.Id))
        {
            return AdministrationResult<OrganizationDetailModel>.Fail(AdministrationFailureReason.Forbidden);
        }

        var validationErrors = ValidateOrganizationRequest(request);

        if (validationErrors.Count > 0)
        {
            return AdministrationResult<OrganizationDetailModel>.Fail(AdministrationFailureReason.ValidationFailed, validationErrors);
        }

        organization.CompanyName = request.CompanyName.Trim();
        organization.Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim();
        organization.Address = request.Address.Trim();
        organization.City = request.City.Trim();
        organization.State = request.State.Trim();
        organization.Zip = request.Zip.Trim();
        organization.Phone = request.Phone.Trim();
        organization.PrimaryContactFirstName = request.PrimaryContactFirstName.Trim();
        organization.PrimaryContactLastName = request.PrimaryContactLastName.Trim();

        await _store.SaveChangesAsync(cancellationToken);
        await _auditWriter.WriteOrganizationUpdatedAsync(actor.Id, organization, cancellationToken);

        return AdministrationResult<OrganizationDetailModel>.Success(ToOrganizationDetail(organization));
    }

    public async Task<AdministrationResult> ArchiveOrganizationAsync(Guid actorUserId, Guid organizationId, CancellationToken cancellationToken)
    {
        var actor = await _store.FindUserByIdAsync(actorUserId, cancellationToken);

        if (actor is null || actor.Role != UserRole.SiteAdmin)
        {
            return AdministrationResult.Fail(AdministrationFailureReason.Forbidden);
        }

        var organization = await _store.FindOrganizationByIdAsync(organizationId, cancellationToken);

        if (organization is null)
        {
            return AdministrationResult.Fail(AdministrationFailureReason.NotFound);
        }

        if (!organization.IsArchived)
        {
            organization.IsArchived = true;
            await _store.SaveChangesAsync(cancellationToken);
            await _auditWriter.WriteOrganizationArchivedAsync(actor.Id, organization, cancellationToken);
        }

        return AdministrationResult.Success();
    }

    public async Task<AdministrationResult<string>> RegenerateInviteCodeAsync(
        Guid actorUserId,
        Guid organizationId,
        CancellationToken cancellationToken)
    {
        var actor = await _store.FindUserByIdAsync(actorUserId, cancellationToken);

        if (actor is null)
        {
            return AdministrationResult<string>.Fail(AdministrationFailureReason.Forbidden);
        }

        var organization = await _store.FindOrganizationByIdAsync(organizationId, cancellationToken);

        if (organization is null)
        {
            return AdministrationResult<string>.Fail(AdministrationFailureReason.NotFound);
        }

        if (!CanAccessOrganization(actor, organization.Id))
        {
            return AdministrationResult<string>.Fail(AdministrationFailureReason.Forbidden);
        }

        organization.InviteCode = GenerateInviteCode();
        organization.InviteCodeGeneratedAtUtc = DateTime.UtcNow;

        await _store.SaveChangesAsync(cancellationToken);

        return AdministrationResult<string>.Success(organization.InviteCode);
    }

    public async Task<AdministrationResult<PagedResultModel<UserSummaryModel>>> ListUsersAsync(
        Guid actorUserId,
        Guid organizationId,
        OrganizationUsersListQueryModel request,
        CancellationToken cancellationToken)
    {
        var actor = await _store.FindUserByIdAsync(actorUserId, cancellationToken);

        if (actor is null)
        {
            return AdministrationResult<PagedResultModel<UserSummaryModel>>.Fail(AdministrationFailureReason.Forbidden);
        }

        var organization = await _store.FindOrganizationByIdAsync(organizationId, cancellationToken);

        if (organization is null)
        {
            return AdministrationResult<PagedResultModel<UserSummaryModel>>.Fail(AdministrationFailureReason.NotFound);
        }

        if (organization.IsArchived)
        {
            return AdministrationResult<PagedResultModel<UserSummaryModel>>.Fail(AdministrationFailureReason.NotFound);
        }

        if (!CanAccessOrganization(actor, organizationId))
        {
            return AdministrationResult<PagedResultModel<UserSummaryModel>>.Fail(AdministrationFailureReason.Forbidden);
        }

        var role = TryParseRole(request.Role);
        var status = TryParseStatus(request.Status);

        if (!string.IsNullOrWhiteSpace(request.Role) && role is null)
        {
            return AdministrationResult<PagedResultModel<UserSummaryModel>>.Fail(
                AdministrationFailureReason.ValidationFailed,
                new Dictionary<string, string[]>
                {
                    [nameof(request.Role)] = new[] { "Role must be SiteAdmin, OrgAdmin, User, or ReadOnly." }
                });
        }

        if (!string.IsNullOrWhiteSpace(request.Status) && status is null)
        {
            return AdministrationResult<PagedResultModel<UserSummaryModel>>.Fail(
                AdministrationFailureReason.ValidationFailed,
                new Dictionary<string, string[]>
                {
                    [nameof(request.Status)] = new[] { "Status must be Active or Inactive." }
                });
        }

        var sortBy = NormalizeUserSortBy(request.SortBy);
        var sortDirection = NormalizeSortDirection(request.SortDirection);
        var page = request.Page <= 0 ? 1 : request.Page;
        var pageSize = request.PageSize <= 0 ? 25 : request.PageSize;

        var result = await _store.GetUsersByOrganizationAsync(
            organizationId,
            new UserSearchFilters
            {
                Search = request.Search,
                Role = role,
                Status = status,
                SortBy = sortBy,
                SortDirection = sortDirection,
                Page = page,
                PageSize = pageSize
            },
            cancellationToken);

        return AdministrationResult<PagedResultModel<UserSummaryModel>>.Success(result);
    }

    public async Task<AdministrationResult<UserCreateResponseModel>> CreateUserAsync(
        Guid actorUserId,
        Guid organizationId,
        UserCreateRequestModel request,
        CancellationToken cancellationToken)
    {
        var actor = await _store.FindUserByIdAsync(actorUserId, cancellationToken);

        if (actor is null)
        {
            return AdministrationResult<UserCreateResponseModel>.Fail(AdministrationFailureReason.Forbidden);
        }

        var organization = await _store.FindOrganizationByIdAsync(organizationId, cancellationToken);

        if (organization is null)
        {
            return AdministrationResult<UserCreateResponseModel>.Fail(AdministrationFailureReason.NotFound);
        }

        if (organization.IsArchived)
        {
            return AdministrationResult<UserCreateResponseModel>.Fail(AdministrationFailureReason.NotFound);
        }

        if (!CanAccessOrganization(actor, organizationId))
        {
            return AdministrationResult<UserCreateResponseModel>.Fail(AdministrationFailureReason.Forbidden);
        }

        var role = TryParseRole(request.Role);
        var status = string.IsNullOrWhiteSpace(request.Status)
            ? UserLifecycleStatus.Active
            : TryParseStatus(request.Status);

        var validationErrors = new Dictionary<string, string[]>(ValidateUserNameAndEmail(
            request.FirstName,
            request.LastName,
            request.Email));

        if (role is null)
        {
            validationErrors[nameof(request.Role)] = new[] { "Role must be SiteAdmin, OrgAdmin, User, or ReadOnly." };
        }

        if (status is null)
        {
            validationErrors[nameof(request.Status)] = new[] { "Status must be Active or Inactive." };
        }

        var passwordValidation = _passwordPolicyValidator.Validate(request.InitialPassword);

        if (!passwordValidation.IsValid)
        {
            validationErrors[nameof(request.InitialPassword)] = passwordValidation.Errors.ToArray();
        }

        if (validationErrors.Count > 0)
        {
            return AdministrationResult<UserCreateResponseModel>.Fail(AdministrationFailureReason.ValidationFailed, validationErrors);
        }

        if (role == UserRole.SiteAdmin)
        {
            return AdministrationResult<UserCreateResponseModel>.Fail(
                AdministrationFailureReason.ValidationFailed,
                new Dictionary<string, string[]>
                {
                    [nameof(request.Role)] = new[] { "SiteAdmin accounts are global and cannot be created within an organization." }
                });
        }

        var normalizedEmail = request.Email.Trim();
        var existingUser = await _store.FindUserByEmailAsync(normalizedEmail, cancellationToken);

        if (existingUser is not null)
        {
            return AdministrationResult<UserCreateResponseModel>.Fail(
                AdministrationFailureReason.ValidationFailed,
                new Dictionary<string, string[]>
                {
                    [nameof(request.Email)] = new[] { "Email must be globally unique." }
                });
        }

        var user = new User
        {
            Id = Guid.NewGuid(),
            OrganizationId = organizationId,
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            Email = normalizedEmail,
            PasswordHash = _passwordHasher.Hash(request.InitialPassword),
            Role = role!.Value,
            Status = status!.Value,
            MustChangePassword = true
        };

        await _store.AddUserAsync(user, cancellationToken);
        await _store.SaveChangesAsync(cancellationToken);
        await _auditWriter.WriteUserCreatedAsync(actor.Id, user, cancellationToken);

        return AdministrationResult<UserCreateResponseModel>.Success(new UserCreateResponseModel
        {
            UserId = user.Id,
            OrganizationId = organizationId,
            Email = user.Email,
            Role = user.Role.ToString(),
            Status = user.Status.ToString()
        });
    }

    public async Task<AdministrationResult<string>> GetUserImportTemplateAsync(
        Guid actorUserId,
        Guid organizationId,
        CancellationToken cancellationToken)
    {
        var actor = await _store.FindUserByIdAsync(actorUserId, cancellationToken);
        if (actor is null)
        {
            return AdministrationResult<string>.Fail(AdministrationFailureReason.Forbidden);
        }

        var organization = await _store.FindOrganizationByIdAsync(organizationId, cancellationToken);
        if (organization is null || organization.IsArchived)
        {
            return AdministrationResult<string>.Fail(AdministrationFailureReason.NotFound);
        }

        if (!CanAccessOrganization(actor, organizationId))
        {
            return AdministrationResult<string>.Fail(AdministrationFailureReason.Forbidden);
        }

        const string template = "firstName,lastName,email,role,status,initialPassword\nJane,Doe,jane.doe@example.com,User,Active,ChangeMe123!";
        return AdministrationResult<string>.Success(template);
    }

    public async Task<AdministrationResult<UserImportResponseModel>> ImportUsersCsvAsync(
        Guid actorUserId,
        Guid organizationId,
        byte[] csvBytes,
        CancellationToken cancellationToken)
    {
        var actor = await _store.FindUserByIdAsync(actorUserId, cancellationToken);
        if (actor is null)
        {
            return AdministrationResult<UserImportResponseModel>.Fail(AdministrationFailureReason.Forbidden);
        }

        var organization = await _store.FindOrganizationByIdAsync(organizationId, cancellationToken);
        if (organization is null || organization.IsArchived)
        {
            return AdministrationResult<UserImportResponseModel>.Fail(AdministrationFailureReason.NotFound);
        }

        if (!CanAccessOrganization(actor, organizationId))
        {
            return AdministrationResult<UserImportResponseModel>.Fail(AdministrationFailureReason.Forbidden);
        }

        if (csvBytes.Length == 0 || csvBytes.Length > 5 * 1024 * 1024)
        {
            return AdministrationResult<UserImportResponseModel>.Fail(
                AdministrationFailureReason.ValidationFailed,
                new Dictionary<string, string[]>
                {
                    ["csvFile"] = new[] { "CSV file must be between 1 byte and 5 MB." }
                });
        }

        string csvText;
        try
        {
            csvText = Encoding.UTF8.GetString(csvBytes);
        }
        catch
        {
            return AdministrationResult<UserImportResponseModel>.Fail(
                AdministrationFailureReason.ValidationFailed,
                new Dictionary<string, string[]>
                {
                    ["csvFile"] = new[] { "CSV file must be UTF-8 encoded." }
                });
        }

        using var reader = new StringReader(csvText);
        var lineNumber = 0;
        var headerLine = reader.ReadLine();
        lineNumber++;

        if (string.IsNullOrWhiteSpace(headerLine))
        {
            return AdministrationResult<UserImportResponseModel>.Fail(
                AdministrationFailureReason.ValidationFailed,
                new Dictionary<string, string[]>
                {
                    ["csvFile"] = new[] { "CSV header row is required." }
                });
        }

        var header = ParseCsvLine(headerLine.TrimStart('\uFEFF'));
        var expectedHeader = new[] { "firstName", "lastName", "email", "role", "status", "initialPassword" };
        if (header.Count != expectedHeader.Length || !header.SequenceEqual(expectedHeader))
        {
            return AdministrationResult<UserImportResponseModel>.Fail(
                AdministrationFailureReason.ValidationFailed,
                new Dictionary<string, string[]>
                {
                    ["csvFile"] = new[] { "CSV header must be exactly: firstName,lastName,email,role,status,initialPassword" }
                });
        }

        var parsedRows = new List<(int RowNumber, string FirstName, string LastName, string Email, string Role, string Status, string InitialPassword)>();
        var errors = new Dictionary<string, string[]>();
        var seenEmails = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        string? line;
        while ((line = reader.ReadLine()) is not null)
        {
            lineNumber++;
            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            var cols = ParseCsvLine(line);
            if (cols.Count == 1 && string.IsNullOrWhiteSpace(cols[0]))
            {
                continue;
            }

            if (cols.Count != expectedHeader.Length)
            {
                errors[$"rows[{lineNumber}].csv"] = new[] { "Row must contain exactly 6 columns." };
                continue;
            }

            var firstName = cols[0].Trim();
            var lastName = cols[1].Trim();
            var email = cols[2].Trim();
            var roleRaw = cols[3].Trim();
            var statusRaw = cols[4].Trim();
            var initialPassword = cols[5];

            if (firstName.Length == 0)
            {
                errors[$"rows[{lineNumber}].firstName"] = new[] { "FirstName is required." };
            }
            else if (firstName.Length > 100)
            {
                errors[$"rows[{lineNumber}].firstName"] = new[] { "FirstName must be 100 characters or fewer." };
            }

            if (lastName.Length == 0)
            {
                errors[$"rows[{lineNumber}].lastName"] = new[] { "LastName is required." };
            }
            else if (lastName.Length > 100)
            {
                errors[$"rows[{lineNumber}].lastName"] = new[] { "LastName must be 100 characters or fewer." };
            }

            if (email.Length == 0)
            {
                errors[$"rows[{lineNumber}].email"] = new[] { "Email is required." };
            }
            else if (!new EmailAddressAttribute().IsValid(email))
            {
                errors[$"rows[{lineNumber}].email"] = new[] { "Email must be a valid email address." };
            }
            else if (!seenEmails.Add(email))
            {
                errors[$"rows[{lineNumber}].email"] = new[] { "Email must be unique within the import file." };
            }

            var role = TryParseRole(roleRaw);
            if (role is null || role == UserRole.SiteAdmin)
            {
                errors[$"rows[{lineNumber}].role"] = new[] { "Role must be Org Admin, User, or Read Only." };
            }

            UserLifecycleStatus? status = string.IsNullOrWhiteSpace(statusRaw)
                ? UserLifecycleStatus.Active
                : TryParseStatus(statusRaw);
            if (status is null)
            {
                errors[$"rows[{lineNumber}].status"] = new[] { "Status must be Active or Inactive." };
            }

            if (string.IsNullOrEmpty(initialPassword))
            {
                errors[$"rows[{lineNumber}].initialPassword"] = new[] { "InitialPassword is required." };
            }
            else
            {
                var passwordValidation = _passwordPolicyValidator.Validate(initialPassword);
                if (!passwordValidation.IsValid)
                {
                    errors[$"rows[{lineNumber}].initialPassword"] = passwordValidation.Errors.ToArray();
                }
            }

            parsedRows.Add((lineNumber, firstName, lastName, email, roleRaw, statusRaw, initialPassword));
        }

        if (parsedRows.Count == 0)
        {
            errors["csvFile"] = new[] { "CSV file must include at least one non-blank data row." };
        }
        else if (parsedRows.Count > 1000)
        {
            errors["csvFile"] = new[] { "CSV file cannot exceed 1,000 data rows." };
        }

        if (errors.Count > 0)
        {
            return AdministrationResult<UserImportResponseModel>.Fail(AdministrationFailureReason.ValidationFailed, errors);
        }

        // Validate against existing users before persisting any row.
        var rowsToCreate = new List<User>(parsedRows.Count);
        foreach (var row in parsedRows)
        {
            var existingUser = await _store.FindUserByEmailAsync(row.Email, cancellationToken);
            if (existingUser is not null)
            {
                errors[$"rows[{row.RowNumber}].email"] = new[] { "Email must be globally unique." };
                continue;
            }

            var parsedRole = TryParseRole(row.Role)!;
            var parsedStatus = string.IsNullOrWhiteSpace(row.Status)
                ? UserLifecycleStatus.Active
                : TryParseStatus(row.Status)!.Value;

            rowsToCreate.Add(new User
            {
                Id = Guid.NewGuid(),
                OrganizationId = organizationId,
                FirstName = row.FirstName,
                LastName = row.LastName,
                Email = row.Email,
                PasswordHash = _passwordHasher.Hash(row.InitialPassword),
                Role = parsedRole.Value,
                Status = parsedStatus,
                MustChangePassword = true
            });
        }

        if (errors.Count > 0)
        {
            return AdministrationResult<UserImportResponseModel>.Fail(AdministrationFailureReason.ValidationFailed, errors);
        }

        foreach (var user in rowsToCreate)
        {
            await _store.AddUserAsync(user, cancellationToken);
        }

        await _store.SaveChangesAsync(cancellationToken);

        foreach (var user in rowsToCreate)
        {
            await _auditWriter.WriteUserCreatedAsync(actor.Id, user, cancellationToken);
        }

        return AdministrationResult<UserImportResponseModel>.Success(new UserImportResponseModel
        {
            OrganizationId = organizationId,
            CreatedCount = rowsToCreate.Count
        });
    }

    public async Task<AdministrationResult<UserDetailModel>> GetUserAsync(Guid actorUserId, Guid userId, CancellationToken cancellationToken)
    {
        var actor = await _store.FindUserByIdAsync(actorUserId, cancellationToken);

        if (actor is null)
        {
            return AdministrationResult<UserDetailModel>.Fail(AdministrationFailureReason.Forbidden);
        }

        var user = await _store.FindUserByIdAsync(userId, cancellationToken);

        if (user is null)
        {
            return AdministrationResult<UserDetailModel>.Fail(AdministrationFailureReason.NotFound);
        }

        if (user.Role == UserRole.SiteAdmin && actor.Role != UserRole.SiteAdmin)
        {
            return AdministrationResult<UserDetailModel>.Fail(AdministrationFailureReason.Forbidden);
        }

        if (user.OrganizationId.HasValue)
        {
            var organization = await _store.FindOrganizationByIdAsync(user.OrganizationId.Value, cancellationToken);

            if (organization is null || organization.IsArchived)
            {
                return AdministrationResult<UserDetailModel>.Fail(AdministrationFailureReason.NotFound);
            }
        }

        if (user.Role != UserRole.SiteAdmin && !CanAccessOrganization(actor, user.OrganizationId))
        {
            return AdministrationResult<UserDetailModel>.Fail(AdministrationFailureReason.Forbidden);
        }

        return AdministrationResult<UserDetailModel>.Success(ToUserDetail(user));
    }

    public async Task<AdministrationResult<UserDetailModel>> UpdateUserAsync(
        Guid actorUserId,
        Guid userId,
        UserUpdateRequestModel request,
        CancellationToken cancellationToken)
    {
        var actor = await _store.FindUserByIdAsync(actorUserId, cancellationToken);

        if (actor is null)
        {
            return AdministrationResult<UserDetailModel>.Fail(AdministrationFailureReason.Forbidden);
        }

        var user = await _store.FindUserByIdAsync(userId, cancellationToken);

        if (user is null)
        {
            return AdministrationResult<UserDetailModel>.Fail(AdministrationFailureReason.NotFound);
        }

        if (user.Role == UserRole.SiteAdmin && actor.Role != UserRole.SiteAdmin)
        {
            return AdministrationResult<UserDetailModel>.Fail(AdministrationFailureReason.Forbidden);
        }

        if (user.OrganizationId.HasValue)
        {
            var organization = await _store.FindOrganizationByIdAsync(user.OrganizationId.Value, cancellationToken);

            if (organization is null || organization.IsArchived)
            {
                return AdministrationResult<UserDetailModel>.Fail(AdministrationFailureReason.NotFound);
            }
        }

        if (user.Role != UserRole.SiteAdmin && !CanAccessOrganization(actor, user.OrganizationId))
        {
            return AdministrationResult<UserDetailModel>.Fail(AdministrationFailureReason.Forbidden);
        }

        var role = TryParseRole(request.Role);
        var status = TryParseStatus(request.Status);

        var validationErrors = new Dictionary<string, string[]>(ValidateUserNameAndEmail(
            request.FirstName,
            request.LastName,
            request.Email));

        if (role is null)
        {
            validationErrors[nameof(request.Role)] = new[] { "Role must be SiteAdmin, OrgAdmin, User, or ReadOnly." };
        }

        if (status is null)
        {
            validationErrors[nameof(request.Status)] = new[] { "Status must be Active or Inactive." };
        }

        if (validationErrors.Count > 0)
        {
            return AdministrationResult<UserDetailModel>.Fail(AdministrationFailureReason.ValidationFailed, validationErrors);
        }

        if (role == UserRole.SiteAdmin)
        {
            return AdministrationResult<UserDetailModel>.Fail(
                AdministrationFailureReason.ValidationFailed,
                new Dictionary<string, string[]>
                {
                    [nameof(request.Role)] = new[] { "SiteAdmin accounts are global and cannot be set through organization user administration." }
                });
        }

        if (user.OrganizationId is null)
        {
            return AdministrationResult<UserDetailModel>.Fail(AdministrationFailureReason.Forbidden);
        }

        var normalizedEmail = request.Email.Trim();
        var existingUser = await _store.FindUserByEmailAsync(normalizedEmail, cancellationToken);

        if (existingUser is not null && existingUser.Id != user.Id)
        {
            return AdministrationResult<UserDetailModel>.Fail(
                AdministrationFailureReason.ValidationFailed,
                new Dictionary<string, string[]>
                {
                    [nameof(request.Email)] = new[] { "Email must be globally unique." }
                });
        }

        var nextRole = role!.Value;
        var nextStatus = status!.Value;
        var isSelf = actor.Id == user.Id;

        if (isSelf && actor.Role == UserRole.OrgAdmin)
        {
            var willLoseAdminRole = user.Role == UserRole.OrgAdmin && nextRole != UserRole.OrgAdmin;
            var willBecomeInactive = user.Status == UserLifecycleStatus.Active && nextStatus == UserLifecycleStatus.Inactive;

            if (willLoseAdminRole || willBecomeInactive)
            {
                var orgAdminCount = await _store.CountOrgAdminsAsync(user.OrganizationId.Value, cancellationToken);

                if (orgAdminCount <= 1)
                {
                    return AdministrationResult<UserDetailModel>.Fail(
                        AdministrationFailureReason.ValidationFailed,
                        new Dictionary<string, string[]>
                        {
                            [nameof(request.Role)] = new[] { "The last Org Admin cannot remove their own Org Admin role or deactivate their own account." }
                        });
                }
            }
        }

        var previousRole = user.Role.ToString();
        var previousStatus = user.Status.ToString();

        user.FirstName = request.FirstName.Trim();
        user.LastName = request.LastName.Trim();
        user.Email = normalizedEmail;
        user.Role = nextRole;
        user.Status = nextStatus;

        await _store.SaveChangesAsync(cancellationToken);
        await _auditWriter.WriteUserUpdatedAsync(actor.Id, user, previousRole, previousStatus, cancellationToken);

        return AdministrationResult<UserDetailModel>.Success(ToUserDetail(user));
    }

    private static string NormalizeOrganizationSortBy(string? sortBy)
    {
        return string.Equals(sortBy, "createdAt", StringComparison.OrdinalIgnoreCase)
            ? "createdAt"
            : "companyName";
    }

    private static string NormalizeUserSortBy(string? sortBy)
    {
        if (string.Equals(sortBy, "email", StringComparison.OrdinalIgnoreCase))
        {
            return "email";
        }

        if (string.Equals(sortBy, "createdAt", StringComparison.OrdinalIgnoreCase))
        {
            return "createdAt";
        }

        return "lastName";
    }

    private static string NormalizeSortDirection(string? sortDirection)
    {
        return string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase)
            ? "desc"
            : "asc";
    }

    private static bool CanAccessOrganization(User actor, Guid? organizationId)
    {
        if (actor.Role == UserRole.SiteAdmin)
        {
            return true;
        }

        if (actor.Role != UserRole.OrgAdmin)
        {
            return false;
        }

        return actor.OrganizationId.HasValue && actor.OrganizationId == organizationId;
    }

    private static UserRole? TryParseRole(string? role)
    {
        var normalized = NormalizeRoleValue(role);
        return Enum.TryParse<UserRole>(normalized, ignoreCase: true, out var value)
            ? value
            : null;
    }

    private static UserLifecycleStatus? TryParseStatus(string? status)
    {
        var normalized = status?.Trim();
        return Enum.TryParse<UserLifecycleStatus>(normalized, ignoreCase: true, out var value)
            ? value
            : null;
    }

    private static string? NormalizeRoleValue(string? role)
    {
        if (string.IsNullOrWhiteSpace(role))
        {
            return role;
        }

        var normalized = role.Trim().Replace(" ", string.Empty).Replace("-", string.Empty);
        return normalized;
    }

    private static List<string> ParseCsvLine(string line)
    {
        var columns = new List<string>();
        var current = new StringBuilder();
        var inQuotes = false;

        for (var i = 0; i < line.Length; i++)
        {
            var ch = line[i];

            if (ch == '"')
            {
                if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
                {
                    current.Append('"');
                    i++;
                }
                else
                {
                    inQuotes = !inQuotes;
                }

                continue;
            }

            if (ch == ',' && !inQuotes)
            {
                columns.Add(current.ToString());
                current.Clear();
                continue;
            }

            current.Append(ch);
        }

        columns.Add(current.ToString());
        return columns;
    }

    private static Dictionary<string, string[]> ValidateOrganizationRequest(OrganizationUpsertRequestModel request)
    {
        var errors = new Dictionary<string, string[]>();

        ValidateRequiredTrimmed(request.CompanyName, nameof(request.CompanyName), 200, errors);
        ValidateRequiredTrimmed(request.Address, nameof(request.Address), 200, errors);
        ValidateRequiredTrimmed(request.City, nameof(request.City), 100, errors);
        ValidateRequiredTrimmed(request.State, nameof(request.State), 50, errors);
        ValidateRequiredTrimmed(request.Zip, nameof(request.Zip), 20, errors);
        ValidateRequiredTrimmed(request.Phone, nameof(request.Phone), 25, errors);
        ValidateRequiredTrimmed(request.PrimaryContactFirstName, nameof(request.PrimaryContactFirstName), 100, errors);
        ValidateRequiredTrimmed(request.PrimaryContactLastName, nameof(request.PrimaryContactLastName), 100, errors);

        return errors;
    }

    private static Dictionary<string, string[]> ValidateUserNameAndEmail(string firstName, string lastName, string email)
    {
        var errors = new Dictionary<string, string[]>();

        ValidateRequiredTrimmed(firstName, nameof(UserCreateRequestModel.FirstName), 100, errors);
        ValidateRequiredTrimmed(lastName, nameof(UserCreateRequestModel.LastName), 100, errors);

        var trimmedEmail = email.Trim();

        if (trimmedEmail.Length == 0)
        {
            errors[nameof(UserCreateRequestModel.Email)] = new[] { "Email is required." };
        }
        else if (!new EmailAddressAttribute().IsValid(trimmedEmail))
        {
            errors[nameof(UserCreateRequestModel.Email)] = new[] { "Email must be a valid email address." };
        }

        return errors;
    }

    private static void ValidateRequiredTrimmed(
        string value,
        string fieldName,
        int maxLength,
        Dictionary<string, string[]> errors)
    {
        var trimmed = value.Trim();

        if (trimmed.Length == 0)
        {
            errors[fieldName] = new[] { $"{fieldName} is required." };
            return;
        }

        if (trimmed.Length > maxLength)
        {
            errors[fieldName] = new[] { $"{fieldName} must be {maxLength} characters or fewer." };
        }
    }

    private static OrganizationDetailModel ToOrganizationDetail(Organization organization)
    {
        return new OrganizationDetailModel
        {
            OrganizationId = organization.Id,
            CompanyName = organization.CompanyName,
            Description = organization.Description,
            Address = organization.Address,
            City = organization.City,
            State = organization.State,
            Zip = organization.Zip,
            Phone = organization.Phone,
            PrimaryContactFirstName = organization.PrimaryContactFirstName,
            PrimaryContactLastName = organization.PrimaryContactLastName,
            InviteCode = organization.InviteCode,
            IsArchived = organization.IsArchived
        };
    }

    private static string GenerateInviteCode()
    {
        const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
        return new string(Enumerable.Range(0, 8).Select(_ => chars[Random.Shared.Next(chars.Length)]).ToArray());
    }

    private static UserDetailModel ToUserDetail(User user)
    {
        return new UserDetailModel
        {
            UserId = user.Id,
            OrganizationId = user.OrganizationId,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            Role = user.Role.ToString(),
            Status = user.Status.ToString()
        };
    }
}
