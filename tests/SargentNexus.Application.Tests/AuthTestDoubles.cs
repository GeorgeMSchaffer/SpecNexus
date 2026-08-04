using SargentNexus.Application.Auth;
using SargentNexus.Domain;

namespace SargentNexus.Application.Tests;

internal static class TestUsers
{
    public static User CreateDefault()
    {
        return new User
        {
            Id = Guid.NewGuid(),
            OrganizationId = Guid.NewGuid(),
            FirstName = "Test",
            LastName = "User",
            Email = "user@sargentnexus.test",
            PasswordHash = "hash:Password1!",
            Role = UserRole.User,
            Status = UserLifecycleStatus.Active,
            MustChangePassword = false
        };
    }

    public static AuthUserRecord Record(User user, string? organizationName = "Org")
    {
        return new AuthUserRecord
        {
            User = user,
            OrganizationName = organizationName
        };
    }
}

internal sealed class FixedTimeProvider : TimeProvider
{
    private readonly DateTimeOffset _utcNow;

    public FixedTimeProvider(DateTime utcNow)
    {
        _utcNow = new DateTimeOffset(utcNow);
    }

    public override DateTimeOffset GetUtcNow()
    {
        return _utcNow;
    }
}

internal sealed class FakeAuthUserLookup : IAuthUserLookup
{
    private readonly List<AuthUserRecord> _emailRecords;
    private readonly Dictionary<Guid, User> _usersById;
    private readonly Dictionary<Guid, Organization> _organizationsById;

    public FakeAuthUserLookup(
        IEnumerable<AuthUserRecord>? emailRecords = null,
        IEnumerable<User>? usersById = null,
        IEnumerable<Organization>? organizationsById = null)
    {
        _emailRecords = emailRecords?.ToList() ?? new List<AuthUserRecord>();
        _usersById = usersById?.ToDictionary(item => item.Id) ?? new Dictionary<Guid, User>();
        _organizationsById = organizationsById?.ToDictionary(item => item.Id) ?? new Dictionary<Guid, Organization>();

        foreach (var record in _emailRecords)
        {
            _usersById[record.User.Id] = record.User;
        }
    }

    public int SaveChangesCallCount { get; private set; }

    public Task<IReadOnlyList<AuthUserRecord>> FindByEmailAsync(string email, CancellationToken cancellationToken)
    {
        IReadOnlyList<AuthUserRecord> matches = _emailRecords
            .Where(item => string.Equals(item.User.Email, email, StringComparison.Ordinal))
            .ToArray();

        return Task.FromResult(matches);
    }

    public Task<User?> FindByIdAsync(Guid userId, CancellationToken cancellationToken)
    {
        _usersById.TryGetValue(userId, out var user);
        return Task.FromResult(user);
    }

    public Task<Organization?> FindOrganizationByIdAsync(Guid organizationId, CancellationToken cancellationToken)
    {
        _organizationsById.TryGetValue(organizationId, out var organization);
        return Task.FromResult(organization);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        SaveChangesCallCount++;
        return Task.CompletedTask;
    }
}

internal sealed class FakePasswordHasher : IPasswordHasher
{
    public string Hash(string password)
    {
        return $"hash:{password}";
    }

    public bool Verify(string providedPassword, string storedPasswordHash)
    {
        return string.Equals(Hash(providedPassword), storedPasswordHash, StringComparison.Ordinal);
    }
}

internal sealed class FakeAccessTokenIssuer : IAccessTokenIssuer
{
    public int IssueCount { get; private set; }

    public IssuedAccessToken Issue(User user)
    {
        IssueCount++;

        return new IssuedAccessToken
        {
            AccessToken = $"token-{IssueCount}",
            ExpiresInSeconds = 3600
        };
    }
}

internal sealed class FakePasswordPolicyValidator : IPasswordPolicyValidator
{
    private readonly PasswordPolicyValidationResult _result;

    public FakePasswordPolicyValidator(PasswordPolicyValidationResult result)
    {
        _result = result;
    }

    public PasswordPolicyValidationResult Validate(string password)
    {
        return _result;
    }
}

internal sealed class FakeTemporaryPasswordGenerator : ITemporaryPasswordGenerator
{
    private readonly string _temporaryPassword;

    public FakeTemporaryPasswordGenerator(string temporaryPassword)
    {
        _temporaryPassword = temporaryPassword;
    }

    public string Generate()
    {
        return _temporaryPassword;
    }
}

internal sealed class FakeAuthAuditWriter : IAuthAuditWriter
{
    public List<User> LoginSuccesses { get; } = new();

    public List<(string Email, Guid? UserId, Guid? OrganizationId, string Reason)> LoginFailures { get; } = new();

    public List<User> PasswordChanges { get; } = new();

    public List<User> ProfileUpdates { get; } = new();

    public List<(Guid? UserId, Guid? OrganizationId, string Reason)> PasswordChangeFailures { get; } = new();

    public List<(Guid ActorUserId, User TargetUser)> TemporaryPasswordIssuedEvents { get; } = new();

    public Task WriteLoginSucceededAsync(User user, CancellationToken cancellationToken)
    {
        LoginSuccesses.Add(user);
        return Task.CompletedTask;
    }

    public Task WriteLoginFailedAsync(string email, Guid? userId, Guid? organizationId, string reason, CancellationToken cancellationToken)
    {
        LoginFailures.Add((email, userId, organizationId, reason));
        return Task.CompletedTask;
    }

    public Task WritePasswordChangedAsync(User user, CancellationToken cancellationToken)
    {
        PasswordChanges.Add(user);
        return Task.CompletedTask;
    }

    public Task WriteProfileUpdatedAsync(User user, CancellationToken cancellationToken)
    {
        ProfileUpdates.Add(user);
        return Task.CompletedTask;
    }

    public Task WritePasswordChangeFailedAsync(Guid? userId, Guid? organizationId, string reason, CancellationToken cancellationToken)
    {
        PasswordChangeFailures.Add((userId, organizationId, reason));
        return Task.CompletedTask;
    }

    public Task WriteTemporaryPasswordIssuedAsync(Guid actorUserId, User user, CancellationToken cancellationToken)
    {
        TemporaryPasswordIssuedEvents.Add((actorUserId, user));
        return Task.CompletedTask;
    }
}