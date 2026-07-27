using SargentNexus.Application.Auth;
using SargentNexus.Domain;

namespace SargentNexus.Application.Tests;

public sealed class AuthAccountServiceTests
{
    [Fact]
    public async Task GivenMissingActor_WhenIssueTemporaryPassword_ThenForbidden()
    {
        var target = TestUsers.CreateDefault();
        target.Id = Guid.NewGuid();

        var lookup = new FakeAuthUserLookup(usersById: new[] { target });
        var service = CreateService(lookup, new FakeAuthAuditWriter());

        var result = await service.IssueTemporaryPasswordAsync(Guid.NewGuid(), target.Id, CancellationToken.None);

        Assert.False(result.Succeeded);
        Assert.Equal(TemporaryPasswordFailureReason.Forbidden, result.FailureReason);
        Assert.Equal(0, lookup.SaveChangesCallCount);
    }

    [Fact]
    public async Task GivenNonAdminActor_WhenIssueTemporaryPassword_ThenForbidden()
    {
        var actor = TestUsers.CreateDefault();
        actor.Role = UserRole.User;
        var target = TestUsers.CreateDefault();
        target.Id = Guid.NewGuid();

        var lookup = new FakeAuthUserLookup(usersById: new[] { actor, target });
        var service = CreateService(lookup, new FakeAuthAuditWriter());

        var result = await service.IssueTemporaryPasswordAsync(actor.Id, target.Id, CancellationToken.None);

        Assert.False(result.Succeeded);
        Assert.Equal(TemporaryPasswordFailureReason.Forbidden, result.FailureReason);
        Assert.Equal(0, lookup.SaveChangesCallCount);
    }

    [Fact]
    public async Task GivenMissingTargetUser_WhenIssueTemporaryPassword_ThenUserNotFound()
    {
        var actor = TestUsers.CreateDefault();
        actor.Role = UserRole.SiteAdmin;

        var lookup = new FakeAuthUserLookup(usersById: new[] { actor });
        var service = CreateService(lookup, new FakeAuthAuditWriter());

        var result = await service.IssueTemporaryPasswordAsync(actor.Id, Guid.NewGuid(), CancellationToken.None);

        Assert.False(result.Succeeded);
        Assert.Equal(TemporaryPasswordFailureReason.UserNotFound, result.FailureReason);
        Assert.Equal(0, lookup.SaveChangesCallCount);
    }

    [Fact]
    public async Task GivenWrongCurrentPassword_WhenChangePassword_ThenInvalidCurrentPasswordReturned()
    {
        var user = TestUsers.CreateDefault();
        var lookup = new FakeAuthUserLookup(usersById: new[] { user });
        var audit = new FakeAuthAuditWriter();
        var service = CreateService(lookup, audit);

        var result = await service.ChangePasswordAsync(user.Id, new ChangePasswordRequestModel
        {
            CurrentPassword = "WrongCurrent1!",
            NewPassword = "NewPassword1!"
        }, CancellationToken.None);

        Assert.False(result.Succeeded);
        Assert.Equal(ChangePasswordFailureReason.InvalidCurrentPassword, result.FailureReason);
        Assert.Single(audit.PasswordChangeFailures);
        Assert.Equal("InvalidCurrentPassword", audit.PasswordChangeFailures[0].Reason);
        Assert.Equal(0, lookup.SaveChangesCallCount);
    }

    [Fact]
    public async Task GivenPasswordPolicyErrors_WhenChangePassword_ThenPolicyFailureReturnedWithErrors()
    {
        var user = TestUsers.CreateDefault();
        var lookup = new FakeAuthUserLookup(usersById: new[] { user });
        var audit = new FakeAuthAuditWriter();
        var validator = new FakePasswordPolicyValidator(new PasswordPolicyValidationResult
        {
            Errors = new[] { "Password must contain at least one number." }
        });
        var service = CreateService(lookup, audit, validator: validator);

        var result = await service.ChangePasswordAsync(user.Id, new ChangePasswordRequestModel
        {
            CurrentPassword = "Password1!",
            NewPassword = "NoNumber!"
        }, CancellationToken.None);

        Assert.False(result.Succeeded);
        Assert.Equal(ChangePasswordFailureReason.InvalidPasswordPolicy, result.FailureReason);
        Assert.Single(result.Errors);
        Assert.Equal("Password must contain at least one number.", result.Errors[0]);
        Assert.Single(audit.PasswordChangeFailures);
        Assert.Equal(0, lookup.SaveChangesCallCount);
    }

    [Fact]
    public async Task GivenValidCurrentPasswordAndPolicy_WhenChangePassword_ThenPasswordUpdatedAndMustChangeCleared()
    {
        var user = TestUsers.CreateDefault();
        user.MustChangePassword = true;
        user.FailedLoginAttemptCount = 2;
        user.LastFailedLoginAttemptUtc = new DateTime(2026, 7, 24, 11, 30, 0, DateTimeKind.Utc);
        user.LockoutEndUtc = new DateTime(2026, 7, 24, 11, 45, 0, DateTimeKind.Utc);

        var lookup = new FakeAuthUserLookup(usersById: new[] { user });
        var audit = new FakeAuthAuditWriter();
        var service = CreateService(lookup, audit);

        var result = await service.ChangePasswordAsync(user.Id, new ChangePasswordRequestModel
        {
            CurrentPassword = "Password1!",
            NewPassword = "NewPassword1!"
        }, CancellationToken.None);

        Assert.True(result.Succeeded);
        Assert.Equal("hash:NewPassword1!", user.PasswordHash);
        Assert.False(user.MustChangePassword);
        Assert.Equal(0, user.FailedLoginAttemptCount);
        Assert.Null(user.LastFailedLoginAttemptUtc);
        Assert.Null(user.LockoutEndUtc);
        Assert.Equal(1, lookup.SaveChangesCallCount);
        Assert.Single(audit.PasswordChanges);
    }

    [Fact]
    public async Task GivenOrgAdminInDifferentOrganization_WhenIssueTemporaryPassword_ThenForbidden()
    {
        var actor = TestUsers.CreateDefault();
        actor.Role = UserRole.OrgAdmin;
        actor.OrganizationId = Guid.NewGuid();

        var target = TestUsers.CreateDefault();
        target.Id = Guid.NewGuid();
        target.OrganizationId = Guid.NewGuid();

        var lookup = new FakeAuthUserLookup(usersById: new[] { actor, target });
        var audit = new FakeAuthAuditWriter();
        var service = CreateService(lookup, audit);

        var result = await service.IssueTemporaryPasswordAsync(actor.Id, target.Id, CancellationToken.None);

        Assert.False(result.Succeeded);
        Assert.Equal(TemporaryPasswordFailureReason.Forbidden, result.FailureReason);
        Assert.Null(target.TemporaryPasswordHash);
        Assert.Equal(0, lookup.SaveChangesCallCount);
        Assert.Empty(audit.TemporaryPasswordIssuedEvents);
    }

    [Fact]
    public async Task GivenOrgAdminAndArchivedTargetOrganization_WhenIssueTemporaryPassword_ThenForbidden()
    {
        var orgId = Guid.NewGuid();

        var actor = TestUsers.CreateDefault();
        actor.Role = UserRole.OrgAdmin;
        actor.OrganizationId = orgId;

        var target = TestUsers.CreateDefault();
        target.Id = Guid.NewGuid();
        target.OrganizationId = orgId;

        var organization = new Organization
        {
            Id = orgId,
            CompanyName = "Archived Org",
            Address = "Addr",
            City = "City",
            State = "ST",
            Zip = "00000",
            Phone = "555-555-0100",
            PrimaryContactFirstName = "First",
            PrimaryContactLastName = "Last",
            IsArchived = true
        };

        var lookup = new FakeAuthUserLookup(usersById: new[] { actor, target }, organizationsById: new[] { organization });
        var audit = new FakeAuthAuditWriter();
        var service = CreateService(lookup, audit);

        var result = await service.IssueTemporaryPasswordAsync(actor.Id, target.Id, CancellationToken.None);

        Assert.False(result.Succeeded);
        Assert.Equal(TemporaryPasswordFailureReason.Forbidden, result.FailureReason);
        Assert.Null(target.TemporaryPasswordHash);
        Assert.Null(target.TemporaryPasswordExpiresAtUtc);
        Assert.Equal(0, lookup.SaveChangesCallCount);
        Assert.Empty(audit.TemporaryPasswordIssuedEvents);
    }

    [Fact]
    public async Task GivenOrgAdminInSameOrganization_WhenIssueTemporaryPassword_ThenTemporaryPasswordIsIssued()
    {
        var orgId = Guid.NewGuid();
        var nowUtc = new DateTime(2026, 7, 24, 12, 0, 0, DateTimeKind.Utc);

        var actor = TestUsers.CreateDefault();
        actor.Role = UserRole.OrgAdmin;
        actor.OrganizationId = orgId;

        var target = TestUsers.CreateDefault();
        target.Id = Guid.NewGuid();
        target.OrganizationId = orgId;

        var lookup = new FakeAuthUserLookup(usersById: new[] { actor, target });
        var audit = new FakeAuthAuditWriter();
        var service = CreateService(
            lookup,
            audit,
            temporaryPasswordGenerator: new FakeTemporaryPasswordGenerator("TmpOrg11!"),
            nowUtc: nowUtc);

        var result = await service.IssueTemporaryPasswordAsync(actor.Id, target.Id, CancellationToken.None);

        Assert.True(result.Succeeded);
        Assert.Equal("TmpOrg11!", result.Response!.TemporaryPassword);
        Assert.Equal("hash:TmpOrg11!", target.TemporaryPasswordHash);
        Assert.Equal(nowUtc.AddHours(24), target.TemporaryPasswordExpiresAtUtc);
        Assert.True(target.MustChangePassword);
        Assert.Equal(1, lookup.SaveChangesCallCount);
        Assert.Single(audit.TemporaryPasswordIssuedEvents);
    }

    [Fact]
    public async Task GivenAuthorizedAdmin_WhenIssueTemporaryPassword_ThenPasswordIsGeneratedPersistedAndReturned()
    {
        var nowUtc = new DateTime(2026, 7, 24, 12, 0, 0, DateTimeKind.Utc);
        var actor = TestUsers.CreateDefault();
        actor.Role = UserRole.SiteAdmin;

        var target = TestUsers.CreateDefault();
        target.Id = Guid.NewGuid();

        var lookup = new FakeAuthUserLookup(usersById: new[] { actor, target });
        var audit = new FakeAuthAuditWriter();
        var service = CreateService(
            lookup,
            audit,
            temporaryPasswordGenerator: new FakeTemporaryPasswordGenerator("TmpAa11!!"),
            nowUtc: nowUtc);

        var result = await service.IssueTemporaryPasswordAsync(actor.Id, target.Id, CancellationToken.None);

        Assert.True(result.Succeeded);
        Assert.Equal("TmpAa11!!", result.Response!.TemporaryPassword);
        Assert.True(result.Response.MustChangePassword);
        Assert.Equal("hash:TmpAa11!!", target.TemporaryPasswordHash);
        Assert.Equal(nowUtc.AddHours(24), target.TemporaryPasswordExpiresAtUtc);
        Assert.True(target.MustChangePassword);
        Assert.Equal(1, lookup.SaveChangesCallCount);
        Assert.Single(audit.TemporaryPasswordIssuedEvents);
    }

    private static AuthAccountService CreateService(
        FakeAuthUserLookup lookup,
        FakeAuthAuditWriter audit,
        FakePasswordPolicyValidator? validator = null,
        FakeTemporaryPasswordGenerator? temporaryPasswordGenerator = null,
        DateTime? nowUtc = null)
    {
        return new AuthAccountService(
            lookup,
            new FakePasswordHasher(),
            validator ?? new FakePasswordPolicyValidator(new PasswordPolicyValidationResult()),
            audit,
            temporaryPasswordGenerator ?? new FakeTemporaryPasswordGenerator("TmpDefault1!"),
            new FixedTimeProvider(nowUtc ?? new DateTime(2026, 7, 24, 12, 0, 0, DateTimeKind.Utc)));
    }
}