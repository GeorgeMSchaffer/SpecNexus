using SargentNexus.Application.Administration;
using SargentNexus.Application.Auth;
using SargentNexus.Domain;

namespace SargentNexus.Application.Tests;

public sealed class OrganizationUserAdministrationServiceTests
{
    [Fact]
    public async Task GivenNonSiteAdmin_WhenCreateOrganization_ThenForbidden()
    {
        var actor = TestUsers.CreateDefault();
        actor.Role = UserRole.OrgAdmin;

        var store = new FakeOrganizationUserAdministrationStore(users: new[] { actor });
        var audit = new FakeOrganizationUserAuditWriter();
        var service = CreateService(store, audit);

        var result = await service.CreateOrganizationAsync(actor.Id, ValidOrganizationRequest(), CancellationToken.None);

        Assert.False(result.Succeeded);
        Assert.Equal(AdministrationFailureReason.Forbidden, result.FailureReason);
        Assert.Empty(audit.OrganizationCreatedEvents);
    }

    [Fact]
    public async Task GivenSiteAdmin_WhenCreateOrganization_ThenOrganizationAndDefaultsCreatedAndAudited()
    {
        var actor = TestUsers.CreateDefault();
        actor.Role = UserRole.SiteAdmin;
        actor.OrganizationId = null;

        var store = new FakeOrganizationUserAdministrationStore(users: new[] { actor })
        {
            AddedOrganizationBoardId = Guid.NewGuid(),
            AddedOrganizationStatusCount = 5
        };
        var audit = new FakeOrganizationUserAuditWriter();
        var service = CreateService(store, audit);

        var result = await service.CreateOrganizationAsync(actor.Id, ValidOrganizationRequest(), CancellationToken.None);

        Assert.True(result.Succeeded);
        Assert.NotEqual(Guid.Empty, result.Value!.OrganizationId);
        Assert.Equal(store.AddedOrganizationBoardId, result.Value.DefaultBoardId);
        Assert.Equal(5, result.Value.DefaultStatusCount);
        Assert.Single(audit.OrganizationCreatedEvents);
    }

    [Fact]
    public async Task GivenOrgAdminOutsideOrganization_WhenListUsers_ThenForbidden()
    {
        var actor = TestUsers.CreateDefault();
        actor.Role = UserRole.OrgAdmin;
        actor.OrganizationId = Guid.NewGuid();

        var targetOrganization = new Organization
        {
            Id = Guid.NewGuid(),
            CompanyName = "Target Org",
            Address = "Addr",
            City = "City",
            State = "ST",
            Zip = "00000",
            Phone = "555-555-5555",
            PrimaryContactFirstName = "A",
            PrimaryContactLastName = "B"
        };

        var store = new FakeOrganizationUserAdministrationStore(
            users: new[] { actor },
            organizations: new[] { targetOrganization });
        var service = CreateService(store, new FakeOrganizationUserAuditWriter());

        var result = await service.ListUsersAsync(actor.Id, targetOrganization.Id, new OrganizationUsersListQueryModel(), CancellationToken.None);

        Assert.False(result.Succeeded);
        Assert.Equal(AdministrationFailureReason.Forbidden, result.FailureReason);
    }

    [Fact]
    public async Task GivenDuplicateEmail_WhenCreateUser_ThenValidationError()
    {
        var actor = TestUsers.CreateDefault();
        actor.Role = UserRole.SiteAdmin;
        actor.OrganizationId = null;

        var organizationId = Guid.NewGuid();
        var organization = CreateOrganization(organizationId);
        var existing = TestUsers.CreateDefault();
        existing.Email = "duplicate@sargentnexus.test";

        var store = new FakeOrganizationUserAdministrationStore(
            users: new[] { actor, existing },
            organizations: new[] { organization });
        var service = CreateService(store, new FakeOrganizationUserAuditWriter());

        var request = new UserCreateRequestModel
        {
            FirstName = "  New  ",
            LastName = "  User  ",
            Email = "duplicate@sargentnexus.test",
            Role = "User",
            InitialPassword = "StrongPass1!",
            Status = "Active"
        };

        var result = await service.CreateUserAsync(actor.Id, organizationId, request, CancellationToken.None);

        Assert.False(result.Succeeded);
        Assert.Equal(AdministrationFailureReason.ValidationFailed, result.FailureReason);
        Assert.True(result.Errors.ContainsKey(nameof(request.Email)));
    }

    [Fact]
    public async Task GivenArchivedOrganization_WhenListUsers_ThenNotFound()
    {
        var organizationId = Guid.NewGuid();
        var actor = TestUsers.CreateDefault();
        actor.Role = UserRole.SiteAdmin;
        actor.OrganizationId = null;

        var archivedOrganization = CreateOrganization(organizationId);
        archivedOrganization.IsArchived = true;

        var store = new FakeOrganizationUserAdministrationStore(
            users: new[] { actor },
            organizations: new[] { archivedOrganization });

        var service = CreateService(store, new FakeOrganizationUserAuditWriter());

        var result = await service.ListUsersAsync(actor.Id, organizationId, new OrganizationUsersListQueryModel(), CancellationToken.None);

        Assert.False(result.Succeeded);
        Assert.Equal(AdministrationFailureReason.NotFound, result.FailureReason);
    }

    [Fact]
    public async Task GivenArchivedOrganization_WhenCreateUser_ThenNotFound()
    {
        var organizationId = Guid.NewGuid();
        var actor = TestUsers.CreateDefault();
        actor.Role = UserRole.SiteAdmin;
        actor.OrganizationId = null;

        var archivedOrganization = CreateOrganization(organizationId);
        archivedOrganization.IsArchived = true;

        var store = new FakeOrganizationUserAdministrationStore(
            users: new[] { actor },
            organizations: new[] { archivedOrganization });

        var service = CreateService(store, new FakeOrganizationUserAuditWriter());

        var request = new UserCreateRequestModel
        {
            FirstName = "New",
            LastName = "User",
            Email = "new.user@sargentnexus.test",
            Role = "User",
            InitialPassword = "StrongPass1!",
            Status = "Active"
        };

        var result = await service.CreateUserAsync(actor.Id, organizationId, request, CancellationToken.None);

        Assert.False(result.Succeeded);
        Assert.Equal(AdministrationFailureReason.NotFound, result.FailureReason);
    }

    [Fact]
    public async Task GivenArchivedOrganization_WhenGetUser_ThenNotFound()
    {
        var organizationId = Guid.NewGuid();
        var actor = TestUsers.CreateDefault();
        actor.Role = UserRole.SiteAdmin;
        actor.OrganizationId = null;

        var target = TestUsers.CreateDefault();
        target.Id = Guid.NewGuid();
        target.OrganizationId = organizationId;

        var archivedOrganization = CreateOrganization(organizationId);
        archivedOrganization.IsArchived = true;

        var store = new FakeOrganizationUserAdministrationStore(
            users: new[] { actor, target },
            organizations: new[] { archivedOrganization });

        var service = CreateService(store, new FakeOrganizationUserAuditWriter());

        var result = await service.GetUserAsync(actor.Id, target.Id, CancellationToken.None);

        Assert.False(result.Succeeded);
        Assert.Equal(AdministrationFailureReason.NotFound, result.FailureReason);
    }

    [Fact]
    public async Task GivenArchivedOrganization_WhenUpdateUser_ThenNotFound()
    {
        var organizationId = Guid.NewGuid();
        var actor = TestUsers.CreateDefault();
        actor.Role = UserRole.SiteAdmin;
        actor.OrganizationId = null;

        var target = TestUsers.CreateDefault();
        target.Id = Guid.NewGuid();
        target.OrganizationId = organizationId;

        var archivedOrganization = CreateOrganization(organizationId);
        archivedOrganization.IsArchived = true;

        var store = new FakeOrganizationUserAdministrationStore(
            users: new[] { actor, target },
            organizations: new[] { archivedOrganization });

        var service = CreateService(store, new FakeOrganizationUserAuditWriter());

        var request = new UserUpdateRequestModel
        {
            FirstName = target.FirstName,
            LastName = target.LastName,
            Email = target.Email,
            Role = target.Role.ToString(),
            Status = target.Status.ToString()
        };

        var result = await service.UpdateUserAsync(actor.Id, target.Id, request, CancellationToken.None);

        Assert.False(result.Succeeded);
        Assert.Equal(AdministrationFailureReason.NotFound, result.FailureReason);
    }

    [Fact]
    public async Task GivenOrgAdmin_WhenArchiveOrganization_ThenForbidden()
    {
        var orgId = Guid.NewGuid();
        var actor = TestUsers.CreateDefault();
        actor.Role = UserRole.OrgAdmin;
        actor.OrganizationId = orgId;

        var organization = CreateOrganization(orgId);
        var store = new FakeOrganizationUserAdministrationStore(
            users: new[] { actor },
            organizations: new[] { organization });
        var audit = new FakeOrganizationUserAuditWriter();
        var service = CreateService(store, audit);

        var result = await service.ArchiveOrganizationAsync(actor.Id, orgId, CancellationToken.None);

        Assert.False(result.Succeeded);
        Assert.Equal(AdministrationFailureReason.Forbidden, result.FailureReason);
        Assert.False(organization.IsArchived);
        Assert.Empty(audit.OrganizationArchivedEvents);
    }

    [Fact]
    public async Task GivenSiteAdmin_WhenArchiveOrganization_ThenArchivedAndAudited()
    {
        var orgId = Guid.NewGuid();
        var actor = TestUsers.CreateDefault();
        actor.Role = UserRole.SiteAdmin;
        actor.OrganizationId = null;

        var organization = CreateOrganization(orgId);
        var store = new FakeOrganizationUserAdministrationStore(
            users: new[] { actor },
            organizations: new[] { organization });
        var audit = new FakeOrganizationUserAuditWriter();
        var service = CreateService(store, audit);

        var result = await service.ArchiveOrganizationAsync(actor.Id, orgId, CancellationToken.None);

        Assert.True(result.Succeeded);
        Assert.True(organization.IsArchived);
        Assert.Single(audit.OrganizationArchivedEvents);
    }

    [Fact]
    public async Task GivenSiteAdminAndAlreadyArchivedOrganization_WhenArchiveOrganization_ThenSuccessWithoutAdditionalAudit()
    {
        var orgId = Guid.NewGuid();
        var actor = TestUsers.CreateDefault();
        actor.Role = UserRole.SiteAdmin;
        actor.OrganizationId = null;

        var organization = CreateOrganization(orgId);
        organization.IsArchived = true;
        var store = new FakeOrganizationUserAdministrationStore(
            users: new[] { actor },
            organizations: new[] { organization });
        var audit = new FakeOrganizationUserAuditWriter();
        var service = CreateService(store, audit);

        var result = await service.ArchiveOrganizationAsync(actor.Id, orgId, CancellationToken.None);

        Assert.True(result.Succeeded);
        Assert.True(organization.IsArchived);
        Assert.Empty(audit.OrganizationArchivedEvents);
    }

    [Fact]
    public async Task GivenOrgAdmin_WhenGetUserTargetsSiteAdmin_ThenForbidden()
    {
        var orgAdmin = TestUsers.CreateDefault();
        orgAdmin.Role = UserRole.OrgAdmin;
        orgAdmin.OrganizationId = Guid.NewGuid();

        var siteAdmin = TestUsers.CreateDefault();
        siteAdmin.Id = Guid.NewGuid();
        siteAdmin.Role = UserRole.SiteAdmin;
        siteAdmin.OrganizationId = null;

        var store = new FakeOrganizationUserAdministrationStore(users: new[] { orgAdmin, siteAdmin });
        var service = CreateService(store, new FakeOrganizationUserAuditWriter());

        var result = await service.GetUserAsync(orgAdmin.Id, siteAdmin.Id, CancellationToken.None);

        Assert.False(result.Succeeded);
        Assert.Equal(AdministrationFailureReason.Forbidden, result.FailureReason);
    }

    [Fact]
    public async Task GivenOrgAdmin_WhenUpdateUserTargetsSiteAdmin_ThenForbidden()
    {
        var orgAdmin = TestUsers.CreateDefault();
        orgAdmin.Role = UserRole.OrgAdmin;
        orgAdmin.OrganizationId = Guid.NewGuid();

        var siteAdmin = TestUsers.CreateDefault();
        siteAdmin.Id = Guid.NewGuid();
        siteAdmin.Role = UserRole.SiteAdmin;
        siteAdmin.OrganizationId = null;

        var store = new FakeOrganizationUserAdministrationStore(users: new[] { orgAdmin, siteAdmin });
        var service = CreateService(store, new FakeOrganizationUserAuditWriter());

        var request = new UserUpdateRequestModel
        {
            FirstName = siteAdmin.FirstName,
            LastName = siteAdmin.LastName,
            Email = siteAdmin.Email,
            Role = "OrgAdmin",
            Status = "Active"
        };

        var result = await service.UpdateUserAsync(orgAdmin.Id, siteAdmin.Id, request, CancellationToken.None);

        Assert.False(result.Succeeded);
        Assert.Equal(AdministrationFailureReason.Forbidden, result.FailureReason);
    }

    [Fact]
    public async Task GivenLastOrgAdminUpdatingSelfToInactive_WhenUpdateUser_ThenValidationError()
    {
        var orgId = Guid.NewGuid();
        var actor = TestUsers.CreateDefault();
        actor.Role = UserRole.OrgAdmin;
        actor.OrganizationId = orgId;
        actor.Status = UserLifecycleStatus.Active;

        var organization = CreateOrganization(orgId);
        var store = new FakeOrganizationUserAdministrationStore(
            users: new[] { actor },
            organizations: new[] { organization })
        {
            OrgAdminCount = 1
        };

        var service = CreateService(store, new FakeOrganizationUserAuditWriter());

        var request = new UserUpdateRequestModel
        {
            FirstName = actor.FirstName,
            LastName = actor.LastName,
            Email = actor.Email,
            Role = "OrgAdmin",
            Status = "Inactive"
        };

        var result = await service.UpdateUserAsync(actor.Id, actor.Id, request, CancellationToken.None);

        Assert.False(result.Succeeded);
        Assert.Equal(AdministrationFailureReason.ValidationFailed, result.FailureReason);
    }

    private static OrganizationUserAdministrationService CreateService(
        FakeOrganizationUserAdministrationStore store,
        FakeOrganizationUserAuditWriter audit)
    {
        return new OrganizationUserAdministrationService(
            store,
            new FakePasswordHasher(),
            new FakePasswordPolicyValidator(new PasswordPolicyValidationResult()),
            audit);
    }

    private static OrganizationUpsertRequestModel ValidOrganizationRequest()
    {
        return new OrganizationUpsertRequestModel
        {
            CompanyName = "  Acme Corp  ",
            Address = "  1 Main St  ",
            City = "  Denver  ",
            State = "  CO  ",
            Zip = "  80202  ",
            Phone = "  303-555-0100  ",
            PrimaryContactFirstName = "  Avery  ",
            PrimaryContactLastName = "  Bennett  "
        };
    }

    private static Organization CreateOrganization(Guid id)
    {
        return new Organization
        {
            Id = id,
            CompanyName = "Org",
            Address = "Addr",
            City = "City",
            State = "State",
            Zip = "Zip",
            Phone = "555",
            PrimaryContactFirstName = "P",
            PrimaryContactLastName = "C"
        };
    }

    private sealed class FakeOrganizationUserAdministrationStore : IOrganizationUserAdministrationStore
    {
        private readonly Dictionary<Guid, User> _users;
        private readonly Dictionary<Guid, Organization> _organizations;

        public FakeOrganizationUserAdministrationStore(
            IEnumerable<User>? users = null,
            IEnumerable<Organization>? organizations = null)
        {
            _users = users?.ToDictionary(item => item.Id) ?? new Dictionary<Guid, User>();
            _organizations = organizations?.ToDictionary(item => item.Id) ?? new Dictionary<Guid, Organization>();
        }

        public Guid AddedOrganizationBoardId { get; set; } = Guid.NewGuid();

        public int AddedOrganizationStatusCount { get; set; } = 5;

        public int OrgAdminCount { get; set; } = 2;

        public Task AddUserAsync(User user, CancellationToken cancellationToken)
        {
            _users[user.Id] = user;
            return Task.CompletedTask;
        }

        public Task<(Guid BoardId, int StatusCount)> AddOrganizationWithDefaultsAsync(Organization organization, CancellationToken cancellationToken)
        {
            _organizations[organization.Id] = organization;
            return Task.FromResult((AddedOrganizationBoardId, AddedOrganizationStatusCount));
        }

        public Task<int> CountOrgAdminsAsync(Guid organizationId, CancellationToken cancellationToken)
        {
            return Task.FromResult(OrgAdminCount);
        }

        public Task<Organization?> FindOrganizationByIdAsync(Guid organizationId, CancellationToken cancellationToken)
        {
            _organizations.TryGetValue(organizationId, out var organization);
            return Task.FromResult(organization);
        }

        public Task<Organization?> FindOrganizationByInviteCodeAsync(string normalizedCode, CancellationToken cancellationToken)
        {
            var organization = _organizations.Values.SingleOrDefault(item => item.InviteCode == normalizedCode);
            return Task.FromResult(organization);
        }

        public Task<User?> FindUserByEmailAsync(string normalizedEmail, CancellationToken cancellationToken)
        {
            var user = _users.Values.SingleOrDefault(item => item.Email == normalizedEmail);
            return Task.FromResult(user);
        }

        public Task<User?> FindUserByIdAsync(Guid userId, CancellationToken cancellationToken)
        {
            _users.TryGetValue(userId, out var user);
            return Task.FromResult(user);
        }

        public Task<Organization?> FindUserOrganizationAsync(Guid userId, CancellationToken cancellationToken)
        {
            if (!_users.TryGetValue(userId, out var user) || !user.OrganizationId.HasValue)
            {
                return Task.FromResult<Organization?>(null);
            }

            _organizations.TryGetValue(user.OrganizationId.Value, out var organization);
            return Task.FromResult(organization);
        }

        public Task<PagedResultModel<OrganizationListItemModel>> GetOrganizationsAsync(
            string? search,
            bool includeArchived,
            string sortBy,
            string sortDirection,
            int page,
            int pageSize,
            CancellationToken cancellationToken)
        {
            return Task.FromResult(new PagedResultModel<OrganizationListItemModel>
            {
                Items = _organizations.Values.Select(item => new OrganizationListItemModel
                {
                    OrganizationId = item.Id,
                    CompanyName = item.CompanyName,
                    City = item.City,
                    State = item.State,
                    Phone = item.Phone,
                    InviteCode = item.InviteCode,
                    IsArchived = item.IsArchived
                }).ToArray(),
                Page = page,
                PageSize = pageSize,
                TotalCount = _organizations.Count,
                SortBy = sortBy,
                SortDirection = sortDirection
            });
        }

        public Task<PagedResultModel<UserSummaryModel>> GetUsersByOrganizationAsync(Guid organizationId, UserSearchFilters filters, CancellationToken cancellationToken)
        {
            var users = _users.Values
                .Where(item => item.OrganizationId == organizationId)
                .Select(item => new UserSummaryModel
                {
                    UserId = item.Id,
                    OrganizationId = item.OrganizationId,
                    FirstName = item.FirstName,
                    LastName = item.LastName,
                    Email = item.Email,
                    Role = item.Role.ToString(),
                    Status = item.Status.ToString()
                })
                .ToArray();

            return Task.FromResult(new PagedResultModel<UserSummaryModel>
            {
                Items = users,
                Page = filters.Page,
                PageSize = filters.PageSize,
                TotalCount = users.Length,
                SortBy = filters.SortBy,
                SortDirection = filters.SortDirection
            });
        }

        public Task SaveChangesAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }

    private sealed class FakeOrganizationUserAuditWriter : IOrganizationUserAuditWriter
    {
        public List<(Guid ActorUserId, Guid OrganizationId, Guid DefaultBoardId, int StatusCount)> OrganizationCreatedEvents { get; } = new();

        public List<(Guid ActorUserId, Guid OrganizationId)> OrganizationUpdatedEvents { get; } = new();

        public List<(Guid ActorUserId, Guid OrganizationId)> OrganizationArchivedEvents { get; } = new();

        public List<(Guid ActorUserId, Guid UserId)> UserCreatedEvents { get; } = new();

        public List<(Guid ActorUserId, Guid UserId, string PreviousRole, string PreviousStatus)> UserUpdatedEvents { get; } = new();

        public Task WriteOrganizationCreatedAsync(Guid actorUserId, Organization organization, Guid defaultBoardId, int defaultStatusCount, CancellationToken cancellationToken)
        {
            OrganizationCreatedEvents.Add((actorUserId, organization.Id, defaultBoardId, defaultStatusCount));
            return Task.CompletedTask;
        }

        public Task WriteOrganizationUpdatedAsync(Guid actorUserId, Organization organization, CancellationToken cancellationToken)
        {
            OrganizationUpdatedEvents.Add((actorUserId, organization.Id));
            return Task.CompletedTask;
        }

        public Task WriteOrganizationArchivedAsync(Guid actorUserId, Organization organization, CancellationToken cancellationToken)
        {
            OrganizationArchivedEvents.Add((actorUserId, organization.Id));
            return Task.CompletedTask;
        }

        public Task WriteUserCreatedAsync(Guid actorUserId, User user, CancellationToken cancellationToken)
        {
            UserCreatedEvents.Add((actorUserId, user.Id));
            return Task.CompletedTask;
        }

        public Task WriteUserUpdatedAsync(Guid actorUserId, User user, string previousRole, string previousStatus, CancellationToken cancellationToken)
        {
            UserUpdatedEvents.Add((actorUserId, user.Id, previousRole, previousStatus));
            return Task.CompletedTask;
        }
    }
}
