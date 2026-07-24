using SargentNexus.Domain;
using System.Security.Claims;

namespace SargentNexus.Application.Auth;

public interface ILoginService
{
    Task<LoginResult> LoginAsync(LoginRequestModel request, CancellationToken cancellationToken);
}

public interface IAuthUserLookup
{
    Task<IReadOnlyList<AuthUserRecord>> FindByEmailAsync(string email, CancellationToken cancellationToken);

    Task<User?> FindByIdAsync(Guid userId, CancellationToken cancellationToken);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}

public interface IPasswordHasher
{
    string Hash(string password);

    bool Verify(string providedPassword, string storedPasswordHash);
}

public interface IAccessTokenIssuer
{
    IssuedAccessToken Issue(User user);
}

public interface IAccessTokenReader
{
    AccessTokenPrincipal? Read(string accessToken);
}

public interface IPasswordPolicyValidator
{
    PasswordPolicyValidationResult Validate(string password);
}

public interface IAuthAccountService
{
    Task<AuthenticatedUserModel?> GetCurrentUserAsync(Guid userId, CancellationToken cancellationToken);

    Task<ChangePasswordResult> ChangePasswordAsync(Guid userId, ChangePasswordRequestModel request, CancellationToken cancellationToken);

    Task<TemporaryPasswordResult> IssueTemporaryPasswordAsync(Guid actorUserId, Guid targetUserId, CancellationToken cancellationToken);
}

public interface IAuthSeeder
{
    Task SeedSiteAdminAsync(CancellationToken cancellationToken);
}

public interface IAuthAuditWriter
{
    Task WriteLoginSucceededAsync(User user, CancellationToken cancellationToken);

    Task WriteLoginFailedAsync(string email, Guid? userId, Guid? organizationId, string reason, CancellationToken cancellationToken);

    Task WritePasswordChangedAsync(User user, CancellationToken cancellationToken);

    Task WritePasswordChangeFailedAsync(Guid? userId, Guid? organizationId, string reason, CancellationToken cancellationToken);

    Task WriteTemporaryPasswordIssuedAsync(Guid actorUserId, User user, CancellationToken cancellationToken);
}

public interface ITemporaryPasswordGenerator
{
    string Generate();
}

public sealed class AuthUserRecord
{
    public User User { get; init; } = null!;

    public string? OrganizationName { get; init; }
}

public sealed class IssuedAccessToken
{
    public string AccessToken { get; init; } = string.Empty;

    public int ExpiresInSeconds { get; init; }
}

public sealed class AccessTokenPrincipal
{
    public Guid UserId { get; init; }

    public Guid? OrganizationId { get; init; }

    public string Role { get; init; } = string.Empty;

    public string Email { get; init; } = string.Empty;

    public DateTime ExpiresAtUtc { get; init; }
}

public sealed class LoginService : ILoginService
{
    private readonly IAuthUserLookup _authUserLookup;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IAccessTokenIssuer _accessTokenIssuer;
    private readonly TimeProvider _timeProvider;
    private readonly IAuthAuditWriter _authAuditWriter;

    public LoginService(
        IAuthUserLookup authUserLookup,
        IPasswordHasher passwordHasher,
        IAccessTokenIssuer accessTokenIssuer,
        TimeProvider timeProvider,
        IAuthAuditWriter authAuditWriter)
    {
        _authUserLookup = authUserLookup;
        _passwordHasher = passwordHasher;
        _accessTokenIssuer = accessTokenIssuer;
        _timeProvider = timeProvider;
        _authAuditWriter = authAuditWriter;
    }

    public async Task<LoginResult> LoginAsync(LoginRequestModel request, CancellationToken cancellationToken)
    {
        var normalizedEmail = request.Email.Trim();
        var matches = await _authUserLookup.FindByEmailAsync(normalizedEmail, cancellationToken);

        if (matches.Count == 0)
        {
            await _authAuditWriter.WriteLoginFailedAsync(normalizedEmail, null, null, "InvalidCredentials", cancellationToken);
            return LoginResult.Failure(LoginFailureReason.InvalidCredentials);
        }

        if (matches.Count > 1 && request.OrganizationId is null)
        {
            return LoginResult.Success(new LoginResponseModel
            {
                RequiresOrganizationSelection = true,
                Organizations = matches
                    .Where(item => item.User.OrganizationId.HasValue)
                    .Select(item => new LoginOrganizationOptionModel
                    {
                        OrganizationId = item.User.OrganizationId!.Value,
                        OrganizationName = item.OrganizationName ?? string.Empty
                    })
                    .OrderBy(item => item.OrganizationName)
                    .ToArray()
            });
        }

        var selectedUser = request.OrganizationId is null
            ? matches.Single()
            : matches.SingleOrDefault(item => item.User.OrganizationId == request.OrganizationId);

        if (selectedUser is null)
        {
            await _authAuditWriter.WriteLoginFailedAsync(normalizedEmail, null, request.OrganizationId, "InvalidCredentials", cancellationToken);
            return LoginResult.Failure(LoginFailureReason.InvalidCredentials);
        }

        var nowUtc = _timeProvider.GetUtcNow().UtcDateTime;

        if (selectedUser.User.LockoutEndUtc.HasValue && selectedUser.User.LockoutEndUtc.Value > nowUtc)
        {
            await _authAuditWriter.WriteLoginFailedAsync(normalizedEmail, selectedUser.User.Id, selectedUser.User.OrganizationId, "LockedOut", cancellationToken);
            return LoginResult.Failure(LoginFailureReason.LockedOut);
        }

        if (selectedUser.User.Status == UserLifecycleStatus.Inactive)
        {
            await _authAuditWriter.WriteLoginFailedAsync(normalizedEmail, selectedUser.User.Id, selectedUser.User.OrganizationId, "InactiveUser", cancellationToken);
            return LoginResult.Failure(LoginFailureReason.InactiveUser);
        }

        if (!_passwordHasher.Verify(request.Password, selectedUser.User.PasswordHash))
        {
            if (CanAuthenticateWithTemporaryPassword(selectedUser.User, request.Password, nowUtc))
            {
                selectedUser.User.PasswordHash = selectedUser.User.TemporaryPasswordHash!;
                selectedUser.User.TemporaryPasswordHash = null;
                selectedUser.User.TemporaryPasswordExpiresAtUtc = null;
                selectedUser.User.MustChangePassword = true;
                ResetFailedAttempts(selectedUser.User);
                await _authUserLookup.SaveChangesAsync(cancellationToken);
                await _authAuditWriter.WriteLoginSucceededAsync(selectedUser.User, cancellationToken);

                var temporaryPasswordToken = _accessTokenIssuer.Issue(selectedUser.User);

                return LoginResult.Success(new LoginResponseModel
                {
                    AccessToken = temporaryPasswordToken.AccessToken,
                    ExpiresInSeconds = temporaryPasswordToken.ExpiresInSeconds,
                    RequiresPasswordChange = true,
                    User = new LoginUserModel
                    {
                        UserId = selectedUser.User.Id,
                        OrganizationId = selectedUser.User.OrganizationId,
                        Role = selectedUser.User.Role.ToString(),
                        FirstName = selectedUser.User.FirstName,
                        LastName = selectedUser.User.LastName,
                        Email = selectedUser.User.Email,
                        Status = selectedUser.User.Status.ToString()
                    }
                });
            }

            RegisterFailedAttempt(selectedUser.User, nowUtc);
            await _authUserLookup.SaveChangesAsync(cancellationToken);
            var failureReason = selectedUser.User.LockoutEndUtc.HasValue && selectedUser.User.LockoutEndUtc.Value > nowUtc
                ? "LockedOut"
                : "InvalidCredentials";
            await _authAuditWriter.WriteLoginFailedAsync(normalizedEmail, selectedUser.User.Id, selectedUser.User.OrganizationId, failureReason, cancellationToken);

            return selectedUser.User.LockoutEndUtc.HasValue && selectedUser.User.LockoutEndUtc.Value > nowUtc
                ? LoginResult.Failure(LoginFailureReason.LockedOut)
                : LoginResult.Failure(LoginFailureReason.InvalidCredentials);
        }

        ResetFailedAttempts(selectedUser.User);
        await _authUserLookup.SaveChangesAsync(cancellationToken);
        await _authAuditWriter.WriteLoginSucceededAsync(selectedUser.User, cancellationToken);

        var issuedToken = _accessTokenIssuer.Issue(selectedUser.User);

        return LoginResult.Success(new LoginResponseModel
        {
            AccessToken = issuedToken.AccessToken,
            ExpiresInSeconds = issuedToken.ExpiresInSeconds,
            RequiresPasswordChange = selectedUser.User.MustChangePassword,
            User = new LoginUserModel
            {
                UserId = selectedUser.User.Id,
                OrganizationId = selectedUser.User.OrganizationId,
                Role = selectedUser.User.Role.ToString(),
                FirstName = selectedUser.User.FirstName,
                LastName = selectedUser.User.LastName,
                Email = selectedUser.User.Email,
                Status = selectedUser.User.Status.ToString()
            }
        });
    }

    private static void ResetFailedAttempts(User user)
    {
        user.FailedLoginAttemptCount = 0;
        user.LastFailedLoginAttemptUtc = null;
        user.LockoutEndUtc = null;
    }

    private static void RegisterFailedAttempt(User user, DateTime nowUtc)
    {
        var windowStartUtc = nowUtc.AddMinutes(-15);
        var isOutsideWindow = !user.LastFailedLoginAttemptUtc.HasValue || user.LastFailedLoginAttemptUtc.Value < windowStartUtc;

        user.FailedLoginAttemptCount = isOutsideWindow
            ? 1
            : user.FailedLoginAttemptCount + 1;
        user.LastFailedLoginAttemptUtc = nowUtc;

        if (user.FailedLoginAttemptCount >= 5)
        {
            user.LockoutEndUtc = nowUtc.AddMinutes(15);
        }
    }

    private bool CanAuthenticateWithTemporaryPassword(User user, string providedPassword, DateTime nowUtc)
    {
        if (string.IsNullOrWhiteSpace(user.TemporaryPasswordHash) ||
            !user.TemporaryPasswordExpiresAtUtc.HasValue ||
            user.TemporaryPasswordExpiresAtUtc.Value <= nowUtc)
        {
            return false;
        }

        return _passwordHasher.Verify(providedPassword, user.TemporaryPasswordHash);
    }
}

public sealed class AuthAccountService : IAuthAccountService
{
    private readonly IAuthUserLookup _authUserLookup;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IPasswordPolicyValidator _passwordPolicyValidator;
    private readonly IAuthAuditWriter _authAuditWriter;
    private readonly ITemporaryPasswordGenerator _temporaryPasswordGenerator;
    private readonly TimeProvider _timeProvider;

    public AuthAccountService(
        IAuthUserLookup authUserLookup,
        IPasswordHasher passwordHasher,
        IPasswordPolicyValidator passwordPolicyValidator,
        IAuthAuditWriter authAuditWriter,
        ITemporaryPasswordGenerator temporaryPasswordGenerator,
        TimeProvider timeProvider)
    {
        _authUserLookup = authUserLookup;
        _passwordHasher = passwordHasher;
        _passwordPolicyValidator = passwordPolicyValidator;
        _authAuditWriter = authAuditWriter;
        _temporaryPasswordGenerator = temporaryPasswordGenerator;
        _timeProvider = timeProvider;
    }

    public async Task<AuthenticatedUserModel?> GetCurrentUserAsync(Guid userId, CancellationToken cancellationToken)
    {
        var user = await _authUserLookup.FindByIdAsync(userId, cancellationToken);

        if (user is null)
        {
            return null;
        }

        return new AuthenticatedUserModel
        {
            UserId = user.Id,
            OrganizationId = user.OrganizationId,
            Role = user.Role.ToString(),
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            Status = user.Status.ToString()
        };
    }

    public async Task<ChangePasswordResult> ChangePasswordAsync(Guid userId, ChangePasswordRequestModel request, CancellationToken cancellationToken)
    {
        var user = await _authUserLookup.FindByIdAsync(userId, cancellationToken);

        if (user is null)
        {
            await _authAuditWriter.WritePasswordChangeFailedAsync(null, null, "UserNotFound", cancellationToken);
            return ChangePasswordResult.Failure(ChangePasswordFailureReason.UserNotFound);
        }

        if (!_passwordHasher.Verify(request.CurrentPassword, user.PasswordHash))
        {
            await _authAuditWriter.WritePasswordChangeFailedAsync(user.Id, user.OrganizationId, "InvalidCurrentPassword", cancellationToken);
            return ChangePasswordResult.Failure(ChangePasswordFailureReason.InvalidCurrentPassword);
        }

        var policyResult = _passwordPolicyValidator.Validate(request.NewPassword);

        if (!policyResult.IsValid)
        {
            await _authAuditWriter.WritePasswordChangeFailedAsync(user.Id, user.OrganizationId, string.Join(';', policyResult.Errors), cancellationToken);
            return ChangePasswordResult.Failure(ChangePasswordFailureReason.InvalidPasswordPolicy, policyResult.Errors);
        }

        user.PasswordHash = _passwordHasher.Hash(request.NewPassword);
        user.MustChangePassword = false;
        user.FailedLoginAttemptCount = 0;
        user.LastFailedLoginAttemptUtc = null;
        user.LockoutEndUtc = null;
        await _authUserLookup.SaveChangesAsync(cancellationToken);
        await _authAuditWriter.WritePasswordChangedAsync(user, cancellationToken);

        return ChangePasswordResult.Success();
    }

    public async Task<TemporaryPasswordResult> IssueTemporaryPasswordAsync(Guid actorUserId, Guid targetUserId, CancellationToken cancellationToken)
    {
        var actor = await _authUserLookup.FindByIdAsync(actorUserId, cancellationToken);

        if (actor is null || (actor.Role != UserRole.SiteAdmin && actor.Role != UserRole.OrgAdmin))
        {
            return TemporaryPasswordResult.Failure(TemporaryPasswordFailureReason.Forbidden);
        }

        var targetUser = await _authUserLookup.FindByIdAsync(targetUserId, cancellationToken);

        if (targetUser is null)
        {
            return TemporaryPasswordResult.Failure(TemporaryPasswordFailureReason.UserNotFound);
        }

        if (actor.Role == UserRole.OrgAdmin && actor.OrganizationId != targetUser.OrganizationId)
        {
            return TemporaryPasswordResult.Failure(TemporaryPasswordFailureReason.Forbidden);
        }

        var temporaryPassword = _temporaryPasswordGenerator.Generate();
        targetUser.TemporaryPasswordHash = _passwordHasher.Hash(temporaryPassword);
        targetUser.TemporaryPasswordExpiresAtUtc = _timeProvider.GetUtcNow().UtcDateTime.AddHours(24);
        targetUser.MustChangePassword = true;
        await _authUserLookup.SaveChangesAsync(cancellationToken);
        await _authAuditWriter.WriteTemporaryPasswordIssuedAsync(actor.Id, targetUser, cancellationToken);

        return TemporaryPasswordResult.Success(new TemporaryPasswordResponseModel
        {
            TemporaryPassword = temporaryPassword,
            MustChangePassword = true
        });
    }
}