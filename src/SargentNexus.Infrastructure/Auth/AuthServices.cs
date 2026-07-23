using System.Security.Cryptography;
using System.Collections.Concurrent;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using SargentNexus.Application.Auth;
using SargentNexus.Domain;

namespace SargentNexus.Infrastructure;

internal sealed class AuthUserLookup : IAuthUserLookup
{
    private readonly SargentNexusDbContext _dbContext;

    public AuthUserLookup(SargentNexusDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<AuthUserRecord>> FindByEmailAsync(string email, CancellationToken cancellationToken)
    {
        return await _dbContext.Users
            .Include(item => item.Organization)
            .Where(item => item.Email == email)
            .Select(item => new AuthUserRecord
            {
                User = item,
                OrganizationName = item.Organization != null ? item.Organization.CompanyName : null
            })
            .ToArrayAsync(cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }

    public Task<User?> FindByIdAsync(Guid userId, CancellationToken cancellationToken)
    {
        return _dbContext.Users.SingleOrDefaultAsync(item => item.Id == userId, cancellationToken);
    }
}

internal sealed class Pbkdf2PasswordHasher : IPasswordHasher
{
    private const int IterationCount = 100_000;
    private const int SaltSize = 16;
    private const int KeySize = 32;
    private const string AlgorithmName = "SHA256";

    public string Hash(string password)
    {
        var salt = RandomNumberGenerator.GetBytes(SaltSize);
        var hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, IterationCount, HashAlgorithmName.SHA256, KeySize);

        return string.Join('$', "pbkdf2", AlgorithmName, IterationCount.ToString(), Convert.ToBase64String(salt), Convert.ToBase64String(hash));
    }

    public bool Verify(string providedPassword, string storedPasswordHash)
    {
        var segments = storedPasswordHash.Split('$');

        if (segments.Length != 5 || !string.Equals(segments[0], "pbkdf2", StringComparison.Ordinal))
        {
            return string.Equals(providedPassword, storedPasswordHash, StringComparison.Ordinal);
        }

        if (!int.TryParse(segments[2], out var iterationCount))
        {
            return false;
        }

        var salt = Convert.FromBase64String(segments[3]);
        var expectedHash = Convert.FromBase64String(segments[4]);
        var algorithm = string.Equals(segments[1], AlgorithmName, StringComparison.OrdinalIgnoreCase)
            ? HashAlgorithmName.SHA256
            : HashAlgorithmName.SHA256;
        var actualHash = Rfc2898DeriveBytes.Pbkdf2(providedPassword, salt, iterationCount, algorithm, expectedHash.Length);

        return CryptographicOperations.FixedTimeEquals(actualHash, expectedHash);
    }
}

internal sealed class OpaqueAccessTokenIssuer : IAccessTokenIssuer
{
    private readonly InMemoryAccessTokenStore _accessTokenStore;

    public OpaqueAccessTokenIssuer(InMemoryAccessTokenStore accessTokenStore)
    {
        _accessTokenStore = accessTokenStore;
    }

    public IssuedAccessToken Issue(User user)
    {
        var expiresInSeconds = 3600;
        var accessToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
        _accessTokenStore.Store(
            accessToken,
            new AccessTokenPrincipal
            {
                UserId = user.Id,
                OrganizationId = user.OrganizationId,
                Role = user.Role.ToString(),
                Email = user.Email,
                ExpiresAtUtc = DateTime.UtcNow.AddSeconds(expiresInSeconds)
            });

        return new IssuedAccessToken
        {
            AccessToken = accessToken,
            ExpiresInSeconds = expiresInSeconds
        };
    }
}

internal sealed class InMemoryAccessTokenStore : IAccessTokenReader
{
    private readonly ConcurrentDictionary<string, AccessTokenPrincipal> _tokens = new();

    public void Store(string accessToken, AccessTokenPrincipal principal)
    {
        _tokens[accessToken] = principal;
    }

    public AccessTokenPrincipal? Read(string accessToken)
    {
        if (!_tokens.TryGetValue(accessToken, out var principal))
        {
            return null;
        }

        if (principal.ExpiresAtUtc <= DateTime.UtcNow)
        {
            _tokens.TryRemove(accessToken, out _);
            return null;
        }

        return principal;
    }
}

internal sealed class TemporaryPasswordGenerator : ITemporaryPasswordGenerator
{
    public string Generate()
    {
        const string specialCharacters = "!@#$%^&*()-_=+[]{}";
        var passwordBytes = RandomNumberGenerator.GetBytes(8);

        return $"Tmp{passwordBytes[0]:X2}a{passwordBytes[1]:X2}Z{passwordBytes[2]:X2}{specialCharacters[passwordBytes[3] % specialCharacters.Length]}{passwordBytes[4]:X2}{passwordBytes[5]:X2}";
    }
}

internal sealed class PasswordPolicyValidator : IPasswordPolicyValidator
{
    public PasswordPolicyValidationResult Validate(string password)
    {
        var errors = new List<string>();

        if (password.Length < 8)
        {
            errors.Add("Password must be at least 8 characters long.");
        }

        if (!password.Any(char.IsUpper))
        {
            errors.Add("Password must contain at least one uppercase letter.");
        }

        if (!password.Any(char.IsLower))
        {
            errors.Add("Password must contain at least one lowercase letter.");
        }

        if (!password.Any(char.IsDigit))
        {
            errors.Add("Password must contain at least one number.");
        }

        if (!password.Any(ch => !char.IsLetterOrDigit(ch)))
        {
            errors.Add("Password must contain at least one special character.");
        }

        return new PasswordPolicyValidationResult
        {
            Errors = errors
        };
    }
}

internal sealed class AuthSeedOptions
{
    public string SiteAdminEmail { get; init; } = "siteadmin@sargentnexus.local";

    public string SiteAdminFirstName { get; init; } = "Site";

    public string SiteAdminLastName { get; init; } = "Admin";

    public string? SiteAdminPassword { get; init; }
}

internal sealed class AuthSeeder : IAuthSeeder
{
    private readonly SargentNexusDbContext _dbContext;
    private readonly IPasswordHasher _passwordHasher;
    private readonly AuthSeedOptions _options;

    public AuthSeeder(
        SargentNexusDbContext dbContext,
        IPasswordHasher passwordHasher,
        IConfiguration configuration)
    {
        _dbContext = dbContext;
        _passwordHasher = passwordHasher;
        _options = new AuthSeedOptions
        {
            SiteAdminEmail = configuration["Seed:SiteAdminEmail"] ?? "siteadmin@sargentnexus.local",
            SiteAdminFirstName = configuration["Seed:SiteAdminFirstName"] ?? "Site",
            SiteAdminLastName = configuration["Seed:SiteAdminLastName"] ?? "Admin",
            SiteAdminPassword = configuration["Seed:SiteAdminPassword"]
        };
    }

    public async Task SeedSiteAdminAsync(CancellationToken cancellationToken)
    {
        await _dbContext.Database.MigrateAsync(cancellationToken);

        var siteAdminExists = await _dbContext.Users.AnyAsync(item => item.Role == UserRole.SiteAdmin, cancellationToken);

        if (siteAdminExists)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(_options.SiteAdminPassword))
        {
            throw new InvalidOperationException("Seed:SiteAdminPassword must be configured to create the initial Site Admin account.");
        }

        var siteAdmin = new User
        {
            Id = Guid.NewGuid(),
            FirstName = _options.SiteAdminFirstName,
            LastName = _options.SiteAdminLastName,
            Email = _options.SiteAdminEmail.Trim(),
            PasswordHash = _passwordHasher.Hash(_options.SiteAdminPassword),
            Role = UserRole.SiteAdmin,
            Status = UserLifecycleStatus.Active,
            MustChangePassword = true
        };

        _dbContext.Users.Add(siteAdmin);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}

internal sealed class AuthAuditWriter : IAuthAuditWriter
{
    private readonly SargentNexusDbContext _dbContext;
    private readonly TimeProvider _timeProvider;

    public AuthAuditWriter(SargentNexusDbContext dbContext, TimeProvider timeProvider)
    {
        _dbContext = dbContext;
        _timeProvider = timeProvider;
    }

    public Task WriteLoginSucceededAsync(User user, CancellationToken cancellationToken)
    {
        return WriteAsync(
            actorUserId: user.Id,
            organizationId: user.OrganizationId,
            entityId: user.Id,
            eventType: "Authentication.LoginSucceeded",
            metadata: new
            {
                user.Email,
                Role = user.Role.ToString()
            },
            cancellationToken);
    }

    public Task WriteLoginFailedAsync(string email, Guid? userId, Guid? organizationId, string reason, CancellationToken cancellationToken)
    {
        return WriteAsync(
            actorUserId: userId ?? Guid.Empty,
            organizationId: organizationId,
            entityId: userId ?? Guid.Empty,
            eventType: "Authentication.LoginFailed",
            metadata: new
            {
                email,
                reason
            },
            cancellationToken);
    }

    public Task WritePasswordChangedAsync(User user, CancellationToken cancellationToken)
    {
        return WriteAsync(
            actorUserId: user.Id,
            organizationId: user.OrganizationId,
            entityId: user.Id,
            eventType: "Authentication.PasswordChanged",
            metadata: new
            {
                user.Email,
                user.MustChangePassword
            },
            cancellationToken);
    }

    public Task WritePasswordChangeFailedAsync(Guid? userId, Guid? organizationId, string reason, CancellationToken cancellationToken)
    {
        return WriteAsync(
            actorUserId: userId ?? Guid.Empty,
            organizationId: organizationId,
            entityId: userId ?? Guid.Empty,
            eventType: "Authentication.PasswordChangeFailed",
            metadata: new
            {
                reason
            },
            cancellationToken);
    }

    public Task WriteTemporaryPasswordIssuedAsync(Guid actorUserId, User user, CancellationToken cancellationToken)
    {
        return WriteAsync(
            actorUserId: actorUserId,
            organizationId: user.OrganizationId,
            entityId: user.Id,
            eventType: "Authentication.TemporaryPasswordIssued",
            metadata: new
            {
                user.Email,
                TemporaryPasswordExpiresAtUtc = user.TemporaryPasswordExpiresAtUtc
            },
            cancellationToken);
    }

    private async Task WriteAsync(
        Guid actorUserId,
        Guid? organizationId,
        Guid entityId,
        string eventType,
        object metadata,
        CancellationToken cancellationToken)
    {
        _dbContext.AuditEvents.Add(new AuditEvent
        {
            Id = Guid.NewGuid(),
            OrganizationId = organizationId,
            ActorUserId = actorUserId,
            EventType = eventType,
            EntityType = "User",
            EntityId = entityId,
            OccurredAtUtc = _timeProvider.GetUtcNow().UtcDateTime,
            Metadata = JsonSerializer.Serialize(metadata)
        });

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}