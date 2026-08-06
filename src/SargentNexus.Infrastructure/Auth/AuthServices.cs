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

    public Task<Organization?> FindOrganizationByIdAsync(Guid organizationId, CancellationToken cancellationToken)
    {
        return _dbContext.Organizations.SingleOrDefaultAsync(item => item.Id == organizationId, cancellationToken);
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
            return false;
        }

        if (!int.TryParse(segments[2], out var iterationCount))
        {
            return false;
        }

        try
        {
            var salt = Convert.FromBase64String(segments[3]);
            var expectedHash = Convert.FromBase64String(segments[4]);
            var actualHash = Rfc2898DeriveBytes.Pbkdf2(providedPassword, salt, iterationCount, HashAlgorithmName.SHA256, expectedHash.Length);

            return CryptographicOperations.FixedTimeEquals(actualHash, expectedHash);
        }
        catch (FormatException)
        {
            return false;
        }
    }
}

internal sealed class OpaqueAccessTokenIssuer : IAccessTokenIssuer
{
    private readonly InMemoryAccessTokenStore _accessTokenStore;
    private readonly TimeProvider _timeProvider;

    public OpaqueAccessTokenIssuer(InMemoryAccessTokenStore accessTokenStore, TimeProvider timeProvider)
    {
        _accessTokenStore = accessTokenStore;
        _timeProvider = timeProvider;
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
                ExpiresAtUtc = _timeProvider.GetUtcNow().UtcDateTime.AddSeconds(expiresInSeconds)
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

        if (password.Length < 6)
        {
            errors.Add("Password must be at least 6 characters long.");
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
    private const string DemoPassword = "abc123!";
    private static readonly (string Name, string Color)[] DefaultBusinessImpacts =
    {
        ("Low", "#16A34A"),
        ("Medium", "#2563EB"),
        ("High", "#D97706"),
        ("Critical", "#DC2626")
    };

    private static readonly string[] DefaultIdeaTypeNames =
    {
        "Continuous Improvement",
        "Process Revision"
    };

    private static readonly string[] DefaultStatusNames =
    {
        "New / Pending",
        "In Review",
        "In Progress",
        "Client Review",
        "Complete"
    };

    private static readonly DemoOrganizationSeed[] DemoOrganizations =
    {
        new(
            CompanyName: "Acme Advisory Group",
            Address: "101 Main Street",
            City: "Denver",
            State: "CO",
            Zip: "80202",
            Phone: "303-555-0101",
            PrimaryContactFirstName: "Avery",
            PrimaryContactLastName: "Bennett",
            BoardName: "Acme Innovation Board",
            RoleUsers:
            [
                new DemoUserSeed("Olivia", "Harper", "demo.acme.orgadmin@sargentnexus.local", UserRole.OrgAdmin),
                new DemoUserSeed("Liam", "Cole", "demo.acme.user@sargentnexus.local", UserRole.User),
                new DemoUserSeed("Emma", "Shaw", "demo.acme.readonly@sargentnexus.local", UserRole.ReadOnly)
            ]),
        new(
            CompanyName: "Northwind Services",
            Address: "220 Harbor Avenue",
            City: "Seattle",
            State: "WA",
            Zip: "98101",
            Phone: "206-555-0120",
            PrimaryContactFirstName: "Noah",
            PrimaryContactLastName: "Reed",
            BoardName: "Northwind Product Board",
            RoleUsers:
            [
                new DemoUserSeed("Sophia", "Mills", "demo.northwind.orgadmin@sargentnexus.local", UserRole.OrgAdmin),
                new DemoUserSeed("Mason", "Gray", "demo.northwind.user@sargentnexus.local", UserRole.User),
                new DemoUserSeed("Amelia", "Wells", "demo.northwind.readonly@sargentnexus.local", UserRole.ReadOnly)
            ]),
        new(
            CompanyName: "Summit Dynamics",
            Address: "415 Lakeview Drive",
            City: "Austin",
            State: "TX",
            Zip: "78701",
            Phone: "512-555-0188",
            PrimaryContactFirstName: "Ethan",
            PrimaryContactLastName: "Foster",
            BoardName: "Summit Delivery Board",
            RoleUsers:
            [
                new DemoUserSeed("Isabella", "Lane", "demo.summit.orgadmin@sargentnexus.local", UserRole.OrgAdmin),
                new DemoUserSeed("Logan", "Price", "demo.summit.user@sargentnexus.local", UserRole.User),
                new DemoUserSeed("Mia", "Perry", "demo.summit.readonly@sargentnexus.local", UserRole.ReadOnly)
            ])
    };

    private readonly SargentNexusDbContext _dbContext;
    private readonly IPasswordHasher _passwordHasher;
    private readonly AuthSeedOptions _options;
    private bool _databaseEnsured;

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
            SiteAdminPassword = configuration["Seed:SiteAdminPassword"] ?? "Abc123!Demo"
        };
    }

    public async Task SeedSiteAdminAsync(CancellationToken cancellationToken)
    {
        await EnsureDatabaseAsync(cancellationToken);

        var siteAdmin = await _dbContext.Users.SingleOrDefaultAsync(item => item.Role == UserRole.SiteAdmin, cancellationToken);

        if (string.IsNullOrWhiteSpace(_options.SiteAdminPassword))
        {
            throw new InvalidOperationException("Seed:SiteAdminPassword must be configured to create the initial Site Admin account.");
        }

        var organization = await _dbContext.Organizations.FirstOrDefaultAsync(cancellationToken);
        if (organization is null)
        {
            organization = new Organization
            {
                Id = Guid.NewGuid(),
                CompanyName = "Sargent Nexus",
                Address = "1 Demo Street",
                City = "Seattle",
                State = "WA",
                Zip = "98101",
                Phone = "206-555-0100",
                PrimaryContactFirstName = "Demo",
                PrimaryContactLastName = "Admin",
                IsArchived = false,
                InviteCode = GenerateSeedInviteCode(),
                InviteCodeGeneratedAtUtc = DateTime.UtcNow
            };

            _dbContext.Organizations.Add(organization);
        }

        if (siteAdmin is not null)
        {
            siteAdmin.OrganizationId = organization.Id;
            siteAdmin.FirstName = _options.SiteAdminFirstName;
            siteAdmin.LastName = _options.SiteAdminLastName;
            siteAdmin.Email = _options.SiteAdminEmail.Trim();
            siteAdmin.Role = UserRole.SiteAdmin;
            await _dbContext.SaveChangesAsync(cancellationToken);
            return;
        }

        var createdSiteAdmin = new User
        {
            Id = Guid.NewGuid(),
            OrganizationId = organization.Id,
            FirstName = _options.SiteAdminFirstName,
            LastName = _options.SiteAdminLastName,
            Email = _options.SiteAdminEmail.Trim(),
            PasswordHash = _passwordHasher.Hash(_options.SiteAdminPassword),
            Role = UserRole.SiteAdmin,
            Status = UserLifecycleStatus.Active,
            MustChangePassword = true
        };

        _dbContext.Users.Add(createdSiteAdmin);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task SeedDevelopmentDemoEnvironmentAsync(CancellationToken cancellationToken)
    {
        await EnsureDatabaseAsync(cancellationToken);

        var siteAdmin = await _dbContext.Users.SingleOrDefaultAsync(item => item.Role == UserRole.SiteAdmin, cancellationToken);
        if (siteAdmin is null)
        {
            siteAdmin = new User
            {
                Id = Guid.NewGuid(),
                OrganizationId = null,
                FirstName = _options.SiteAdminFirstName,
                LastName = _options.SiteAdminLastName,
                Email = _options.SiteAdminEmail.Trim(),
                PasswordHash = _passwordHasher.Hash(_options.SiteAdminPassword ?? "Abc123!Demo"),
                Role = UserRole.SiteAdmin,
                Status = UserLifecycleStatus.Active,
                MustChangePassword = true
            };

            _dbContext.Users.Add(siteAdmin);
        }

        var nowUtc = DateTime.UtcNow;
        var demoPasswordHash = _passwordHasher.Hash(DemoPassword);

        foreach (var demoOrganization in DemoOrganizations)
        {
            var organization = await FindOrCreateOrganizationAsync(demoOrganization, cancellationToken);
            if (siteAdmin.OrganizationId is null)
            {
                siteAdmin.OrganizationId = organization.Id;
            }

            var statuses = await EnsureStatusesAsync(organization, cancellationToken);
            var board = await EnsureBoardAsync(organization, demoOrganization, cancellationToken);
            var ideaTypes = await EnsureIdeaTypesAsync(organization, cancellationToken);
            var businessImpacts = await EnsureBusinessImpactsAsync(organization, cancellationToken);
            await EnsureBoardSwimlanesAsync(board, statuses, cancellationToken);
            var roleUsers = await EnsureRoleUsersAsync(organization, demoOrganization, demoPasswordHash, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
            await EnsureIdeasAndCommentsAsync(organization, board, statuses, ideaTypes, businessImpacts, roleUsers, nowUtc, cancellationToken);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task EnsureDatabaseAsync(CancellationToken cancellationToken)
    {
        if (_databaseEnsured)
        {
            return;
        }

        if (_dbContext.Database.IsRelational())
        {
            await _dbContext.Database.MigrateAsync(cancellationToken);
            _databaseEnsured = true;
            return;
        }
        await _dbContext.Database.EnsureCreatedAsync(cancellationToken);
        _databaseEnsured = true;
    }

    private async Task<Organization> FindOrCreateOrganizationAsync(
        DemoOrganizationSeed seed,
        CancellationToken cancellationToken)
    {
        var companyName = seed.CompanyName.Trim();
        var organization = await _dbContext.Organizations
            .SingleOrDefaultAsync(item => item.CompanyName == companyName, cancellationToken);

        if (organization is not null)
        {
            return organization;
        }

        organization = new Organization
        {
            Id = Guid.NewGuid(),
            CompanyName = companyName,
            Address = seed.Address,
            City = seed.City,
            State = seed.State,
            Zip = seed.Zip,
            Phone = seed.Phone,
            PrimaryContactFirstName = seed.PrimaryContactFirstName,
            PrimaryContactLastName = seed.PrimaryContactLastName,
            IsArchived = false,
            InviteCode = GenerateSeedInviteCode(),
            InviteCodeGeneratedAtUtc = DateTime.UtcNow
        };

        _dbContext.Organizations.Add(organization);
        return organization;
    }

    private async Task<IReadOnlyList<Status>> EnsureStatusesAsync(Organization organization, CancellationToken cancellationToken)
    {
        var existingStatuses = await _dbContext.Statuses
            .Where(item => item.OrganizationId == organization.Id)
            .ToListAsync(cancellationToken);

        var results = new List<Status>(DefaultStatusNames.Length);

        for (var i = 0; i < DefaultStatusNames.Length; i++)
        {
            var statusName = DefaultStatusNames[i];
            var status = existingStatuses.SingleOrDefault(item => item.Name == statusName);

            if (status is null)
            {
                status = new Status
                {
                    Id = Guid.NewGuid(),
                    OrganizationId = organization.Id,
                    Name = statusName,
                    IsDeleted = false,
                    SortOrder = i,
                    IsDefault = i == 0,
                    Color = null
                };

                _dbContext.Statuses.Add(status);
                existingStatuses.Add(status);
            }
            else if (status.IsDeleted)
            {
                status.IsDeleted = false;
            }

            results.Add(status);
        }

        return results;
    }

    private async Task<IReadOnlyList<IdeaType>> EnsureIdeaTypesAsync(Organization organization, CancellationToken cancellationToken)
    {
        var existingIdeaTypes = await _dbContext.IdeaTypes
            .Where(item => item.OrganizationId == organization.Id)
            .ToListAsync(cancellationToken);

        if (existingIdeaTypes.Count == 0)
        {
            foreach (var (name, index) in DefaultIdeaTypeNames.Select((name, index) => (name, index)))
            {
                var ideaType = new IdeaType
                {
                    Id = Guid.NewGuid(),
                    OrganizationId = organization.Id,
                    Name = name,
                    SortOrder = index,
                    IsDeleted = false
                };

                _dbContext.IdeaTypes.Add(ideaType);
                existingIdeaTypes.Add(ideaType);
            }
        }

        return existingIdeaTypes
            .OrderBy(item => item.IsDeleted)
            .ThenBy(item => item.SortOrder)
            .ThenBy(item => item.Name)
            .ToList();
    }

    private async Task<IReadOnlyList<BusinessImpact>> EnsureBusinessImpactsAsync(Organization organization, CancellationToken cancellationToken)
    {
        var existingBusinessImpacts = await _dbContext.BusinessImpacts
            .Where(item => item.OrganizationId == organization.Id)
            .ToListAsync(cancellationToken);

        if (existingBusinessImpacts.Count == 0)
        {
            foreach (var (item, index) in DefaultBusinessImpacts.Select((item, index) => (item, index)))
            {
                var businessImpact = new BusinessImpact
                {
                    Id = Guid.NewGuid(),
                    OrganizationId = organization.Id,
                    Name = item.Name,
                    Color = item.Color,
                    SortOrder = index,
                    IsDeleted = false
                };

                _dbContext.BusinessImpacts.Add(businessImpact);
                existingBusinessImpacts.Add(businessImpact);
            }
        }

        return existingBusinessImpacts
            .OrderBy(item => item.IsDeleted)
            .ThenBy(item => item.SortOrder)
            .ThenBy(item => item.Name)
            .ToList();
    }

    private async Task<Board> EnsureBoardAsync(
        Organization organization,
        DemoOrganizationSeed seed,
        CancellationToken cancellationToken)
    {
        var boardName = seed.BoardName.Trim();
        var board = await _dbContext.Boards
            .SingleOrDefaultAsync(item => item.OrganizationId == organization.Id && item.Name == boardName, cancellationToken);

        if (board is not null)
        {
            return board;
        }

        board = new Board
        {
            Id = Guid.NewGuid(),
            OrganizationId = organization.Id,
            Name = boardName,
            IsArchived = false
        };

        _dbContext.Boards.Add(board);
        return board;
    }

    private async Task EnsureBoardSwimlanesAsync(
        Board board,
        IReadOnlyList<Status> statuses,
        CancellationToken cancellationToken)
    {
        var existingSwimlanes = await _dbContext.BoardSwimlanes
            .Where(item => item.BoardId == board.Id)
            .ToListAsync(cancellationToken);

        for (var index = 0; index < statuses.Count; index++)
        {
            var status = statuses[index];
            var swimlane = existingSwimlanes.SingleOrDefault(item => item.StatusId == status.Id);

            if (swimlane is null)
            {
                _dbContext.BoardSwimlanes.Add(new BoardSwimlane
                {
                    BoardId = board.Id,
                    StatusId = status.Id,
                    Order = index
                });

                continue;
            }

            swimlane.Order = index;
        }
    }

    private async Task<IReadOnlyDictionary<UserRole, User>> EnsureRoleUsersAsync(
        Organization organization,
        DemoOrganizationSeed seed,
        string demoPasswordHash,
        CancellationToken cancellationToken)
    {
        var seedEmails = seed.RoleUsers
            .Select(item => item.Email.Trim())
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var existingUsers = await _dbContext.Users
            .Where(item => item.OrganizationId == organization.Id && seedEmails.Contains(item.Email))
            .ToListAsync(cancellationToken);

        var roleUsers = new Dictionary<UserRole, User>();

        foreach (var userSeed in seed.RoleUsers)
        {
            var email = userSeed.Email.Trim();
            var user = existingUsers.SingleOrDefault(item => string.Equals(item.Email, email, StringComparison.OrdinalIgnoreCase));

            if (user is null)
            {
                user = new User
                {
                    Id = Guid.NewGuid(),
                    OrganizationId = organization.Id,
                    FirstName = userSeed.FirstName,
                    LastName = userSeed.LastName,
                    Email = email,
                    PasswordHash = demoPasswordHash,
                    Role = userSeed.Role,
                    Status = UserLifecycleStatus.Active,
                    MustChangePassword = false,
                    TemporaryPasswordHash = null,
                    TemporaryPasswordExpiresAtUtc = null,
                    FailedLoginAttemptCount = 0,
                    LastFailedLoginAttemptUtc = null,
                    LockoutEndUtc = null
                };

                _dbContext.Users.Add(user);
                existingUsers.Add(user);
            }
            else
            {
                user.FirstName = userSeed.FirstName;
                user.LastName = userSeed.LastName;
                user.Role = userSeed.Role;

                if (user.MustChangePassword
                    && user.TemporaryPasswordHash is null
                    && user.TemporaryPasswordExpiresAtUtc is null
                    && _passwordHasher.Verify(DemoPassword, user.PasswordHash))
                {
                    user.MustChangePassword = false;
                }
            }

            roleUsers[userSeed.Role] = user;
        }

        return roleUsers;
    }

    private async Task EnsureIdeasAndCommentsAsync(
        Organization organization,
        Board board,
        IReadOnlyList<Status> statuses,
        IReadOnlyList<IdeaType> ideaTypes,
        IReadOnlyList<BusinessImpact> businessImpacts,
        IReadOnlyDictionary<UserRole, User> roleUsers,
        DateTime nowUtc,
        CancellationToken cancellationToken)
    {
        var existingIdeas = await _dbContext.Ideas
            .Where(item => item.BoardId == board.Id)
            .ToListAsync(cancellationToken);

        var existingIdeaIds = existingIdeas
            .Select(item => item.Id)
            .ToArray();

        var existingComments = await _dbContext.Comments
            .Where(item => existingIdeaIds.Contains(item.IdeaId))
            .ToListAsync(cancellationToken);

        var author = roleUsers[UserRole.User];
        var orgAdmin = roleUsers[UserRole.OrgAdmin];
        var readOnly = roleUsers[UserRole.ReadOnly];
        var ideaType = ideaTypes.First();
        var businessImpact = businessImpacts.First();
        for (var index = 0; index < statuses.Count; index++)
        {
            var status = statuses[index];
            var title = $"{status.Name} - Demo Idea";
            var description =
                $"Example specification for {organization.CompanyName} in the {status.Name} swimlane. " +
                "This seeded idea demonstrates expected detail formatting, acceptance context, and stakeholder notes for demos.";

            var idea = existingIdeas.SingleOrDefault(item => item.Title == title);

            if (idea is null)
            {
                idea = new Idea
                {
                    Id = Guid.NewGuid(),
                    BoardId = board.Id,
                    OrganizationId = organization.Id,
                    AuthorUserId = author.Id,
                    Title = title,
                    Description = description,
                    Priority = IdeaPriority.Medium,
                    DueDate = null,
                    BusinessImpactId = businessImpact.Id,
                    BusinessImpact = businessImpact,
                    IdeaTypeId = ideaType.Id,
                    StatusId = status.Id,
                    CreatedAtUtc = nowUtc,
                    UpdatedAtUtc = null
                };

                _dbContext.Ideas.Add(idea);
                existingIdeas.Add(idea);
            }
            else
            {
                idea.Description = description;
                idea.Priority = IdeaPriority.Medium;
                idea.DueDate = null;
                idea.BusinessImpactId = businessImpact.Id;
                idea.BusinessImpact = businessImpact;
                idea.IdeaTypeId = ideaType.Id;
                idea.StatusId = status.Id;
            }

            EnsureCommentExists(
                existingComments,
                idea,
                orgAdmin,
                $"Org Admin review note for {status.Name}: this example item is seeded for walkthrough and validation.",
                nowUtc);

            EnsureCommentExists(
                existingComments,
                idea,
                readOnly,
                $"Read Only feedback sample for {status.Name}: visibility and collaboration behavior can be validated here.",
                nowUtc);
        }
    }

    private void EnsureCommentExists(
        List<Comment> existingComments,
        Idea idea,
        User author,
        string body,
        DateTime nowUtc)
    {
        if (existingComments.Any(item => item.IdeaId == idea.Id && item.AuthorUserId == author.Id && item.Body == body))
        {
            return;
        }

        var comment = new Comment
        {
            Id = Guid.NewGuid(),
            IdeaId = idea.Id,
            AuthorUserId = author.Id,
            Body = body,
            CreatedAtUtc = nowUtc,
            UpdatedAtUtc = null
        };

        _dbContext.Comments.Add(comment);
        existingComments.Add(comment);
    }

    private sealed record DemoOrganizationSeed(
        string CompanyName,
        string Address,
        string City,
        string State,
        string Zip,
        string Phone,
        string PrimaryContactFirstName,
        string PrimaryContactLastName,
        string BoardName,
        IReadOnlyList<DemoUserSeed> RoleUsers);

    private sealed record DemoUserSeed(
        string FirstName,
        string LastName,
        string Email,
        UserRole Role);

    private static string GenerateSeedInviteCode()
    {
        const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
        return new string(Enumerable.Range(0, 8).Select(_ => chars[Random.Shared.Next(chars.Length)]).ToArray());
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

    public Task WriteProfileUpdatedAsync(User user, CancellationToken cancellationToken)
    {
        return WriteAsync(
            actorUserId: user.Id,
            organizationId: user.OrganizationId,
            entityId: user.Id,
            eventType: "User.ProfileUpdated",
            metadata: new
            {
                user.FirstName,
                user.LastName
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








