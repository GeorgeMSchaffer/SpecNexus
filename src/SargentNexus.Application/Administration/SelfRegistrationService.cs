using System.ComponentModel.DataAnnotations;

namespace SargentNexus.Application.Administration;

public sealed class SelfRegistrationRequestModel
{
    [Required]
    [MaxLength(16)]
    public string InviteCode { get; set; } = string.Empty;

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
    public string Password { get; set; } = string.Empty;
}

public sealed class SelfRegistrationResponseModel
{
    public Guid UserId { get; init; }

    public Guid OrganizationId { get; init; }

    public string Email { get; init; } = string.Empty;
}

public enum SelfRegistrationFailureReason
{
    InvalidInviteCode = 1,
    ArchivedOrganization = 2,
    EmailAlreadyInUse = 3,
    InvalidPasswordPolicy = 4
}

public sealed class SelfRegistrationResult
{
    private SelfRegistrationResult(bool succeeded, SelfRegistrationResponseModel? response, SelfRegistrationFailureReason? failureReason, IReadOnlyDictionary<string, string[]> errors)
    {
        Succeeded = succeeded;
        Response = response;
        FailureReason = failureReason;
        Errors = errors;
    }

    public bool Succeeded { get; }

    public SelfRegistrationResponseModel? Response { get; }

    public SelfRegistrationFailureReason? FailureReason { get; }

    public IReadOnlyDictionary<string, string[]> Errors { get; }

    public static SelfRegistrationResult Success(SelfRegistrationResponseModel response) =>
        new(true, response, null, new Dictionary<string, string[]>());

    public static SelfRegistrationResult Fail(SelfRegistrationFailureReason reason, IReadOnlyDictionary<string, string[]>? errors = null) =>
        new(false, null, reason, errors ?? new Dictionary<string, string[]>());
}

public interface ISelfRegistrationStore
{
    Task<Domain.Organization?> FindOrganizationByInviteCodeAsync(string normalizedCode, CancellationToken cancellationToken);

    Task<Domain.User?> FindUserByEmailAsync(string normalizedEmail, CancellationToken cancellationToken);

    Task AddUserAsync(Domain.User user, CancellationToken cancellationToken);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}

public interface ISelfRegistrationService
{
    Task<SelfRegistrationResult> RegisterAsync(SelfRegistrationRequestModel request, CancellationToken cancellationToken);
}

public sealed class SelfRegistrationService : ISelfRegistrationService
{
    private readonly ISelfRegistrationStore _store;
    private readonly Auth.IPasswordHasher _passwordHasher;
    private readonly Auth.IPasswordPolicyValidator _passwordPolicyValidator;

    public SelfRegistrationService(
        ISelfRegistrationStore store,
        Auth.IPasswordHasher passwordHasher,
        Auth.IPasswordPolicyValidator passwordPolicyValidator)
    {
        _store = store;
        _passwordHasher = passwordHasher;
        _passwordPolicyValidator = passwordPolicyValidator;
    }

    public async Task<SelfRegistrationResult> RegisterAsync(SelfRegistrationRequestModel request, CancellationToken cancellationToken)
    {
        var normalizedCode = request.InviteCode.Trim().ToUpperInvariant();
        var organization = await _store.FindOrganizationByInviteCodeAsync(normalizedCode, cancellationToken);

        if (organization is null)
        {
            return SelfRegistrationResult.Fail(SelfRegistrationFailureReason.InvalidInviteCode);
        }

        if (organization.IsArchived)
        {
            return SelfRegistrationResult.Fail(SelfRegistrationFailureReason.ArchivedOrganization);
        }

        var normalizedEmail = request.Email.Trim().ToUpperInvariant();
        var existing = await _store.FindUserByEmailAsync(normalizedEmail, cancellationToken);

        if (existing is not null)
        {
            return SelfRegistrationResult.Fail(
                SelfRegistrationFailureReason.EmailAlreadyInUse,
                new Dictionary<string, string[]> { ["email"] = ["Email is already in use."] });
        }

        var passwordValidation = _passwordPolicyValidator.Validate(request.Password);
        if (!passwordValidation.IsValid)
        {
            return SelfRegistrationResult.Fail(
                SelfRegistrationFailureReason.InvalidPasswordPolicy,
                new Dictionary<string, string[]> { ["password"] = passwordValidation.Errors.ToArray() });
        }

        var user = new Domain.User
        {
            Id = Guid.NewGuid(),
            OrganizationId = organization.Id,
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            Email = request.Email.Trim(),
            PasswordHash = _passwordHasher.Hash(request.Password),
            Role = Domain.UserRole.User,
            Status = Domain.UserLifecycleStatus.Active,
            MustChangePassword = false
        };

        await _store.AddUserAsync(user, cancellationToken);
        await _store.SaveChangesAsync(cancellationToken);

        return SelfRegistrationResult.Success(new SelfRegistrationResponseModel
        {
            UserId = user.Id,
            OrganizationId = organization.Id,
            Email = user.Email
        });
    }
}
