using SargentNexus.Application.Auth;
using SargentNexus.Domain;

namespace SargentNexus.Application.Tests;

public sealed class LoginServiceTests
{
    [Fact]
    public async Task GivenUnknownEmail_WhenLogin_ThenInvalidCredentialsReturnedAndFailureAudited()
    {
        var lookup = new FakeAuthUserLookup();
        var audit = new FakeAuthAuditWriter();
        var service = CreateService(lookup, audit);

        var result = await service.LoginAsync(new LoginRequestModel
        {
            Email = "  nobody@sargentnexus.test ",
            Password = "irrelevant"
        }, CancellationToken.None);

        Assert.False(result.Succeeded);
        Assert.Equal(LoginFailureReason.InvalidCredentials, result.FailureReason);
        Assert.Single(audit.LoginFailures);
        Assert.Equal("nobody@sargentnexus.test", audit.LoginFailures[0].Email);
        Assert.Equal("InvalidCredentials", audit.LoginFailures[0].Reason);
    }

    [Fact]
    public async Task GivenLockedOutUser_WhenLogin_ThenLockedOutReturnedAndNoTokenIssued()
    {
        var nowUtc = new DateTime(2026, 7, 24, 12, 0, 0, DateTimeKind.Utc);
        var user = TestUsers.CreateDefault();
        user.LockoutEndUtc = nowUtc.AddMinutes(5);

        var lookup = new FakeAuthUserLookup(TestUsers.Record(user));
        var audit = new FakeAuthAuditWriter();
        var issuer = new FakeAccessTokenIssuer();
        var service = CreateService(lookup, audit, issuer, nowUtc);

        var result = await service.LoginAsync(new LoginRequestModel
        {
            Email = user.Email,
            Password = "Password1!"
        }, CancellationToken.None);

        Assert.False(result.Succeeded);
        Assert.Equal(LoginFailureReason.LockedOut, result.FailureReason);
        Assert.Equal(0, issuer.IssueCount);
        Assert.Single(audit.LoginFailures);
        Assert.Equal("LockedOut", audit.LoginFailures[0].Reason);
    }

    [Fact]
    public async Task GivenValidCredentials_WhenLogin_ThenAccessTokenIssuedAndFailedAttemptsReset()
    {
        var user = TestUsers.CreateDefault();
        user.MustChangePassword = false;
        user.FailedLoginAttemptCount = 3;
        user.LastFailedLoginAttemptUtc = new DateTime(2026, 7, 24, 11, 55, 0, DateTimeKind.Utc);
        user.LockoutEndUtc = new DateTime(2026, 7, 24, 11, 59, 0, DateTimeKind.Utc);

        var lookup = new FakeAuthUserLookup(TestUsers.Record(user));
        var audit = new FakeAuthAuditWriter();
        var issuer = new FakeAccessTokenIssuer();
        var service = CreateService(lookup, audit, issuer);

        var result = await service.LoginAsync(new LoginRequestModel
        {
            Email = user.Email,
            Password = "Password1!"
        }, CancellationToken.None);

        Assert.True(result.Succeeded);
        Assert.NotNull(result.Response);
        Assert.Equal("token-1", result.Response!.AccessToken);
        Assert.Equal(3600, result.Response.ExpiresInSeconds);
        Assert.Equal(user.Id, result.Response.User!.UserId);
        Assert.Equal(0, user.FailedLoginAttemptCount);
        Assert.Null(user.LastFailedLoginAttemptUtc);
        Assert.Null(user.LockoutEndUtc);
        Assert.Equal(1, lookup.SaveChangesCallCount);
        Assert.Single(audit.LoginSuccesses);
    }

    [Fact]
    public async Task GivenFourthRecentFailedAttempt_WhenLoginWithWrongPassword_ThenUserIsLockedOutAndFailureReasonIsLockedOut()
    {
        var nowUtc = new DateTime(2026, 7, 24, 12, 0, 0, DateTimeKind.Utc);
        var user = TestUsers.CreateDefault();
        user.FailedLoginAttemptCount = 4;
        user.LastFailedLoginAttemptUtc = nowUtc.AddMinutes(-1);

        var lookup = new FakeAuthUserLookup(TestUsers.Record(user));
        var audit = new FakeAuthAuditWriter();
        var service = CreateService(lookup, audit, nowUtc: nowUtc);

        var result = await service.LoginAsync(new LoginRequestModel
        {
            Email = user.Email,
            Password = "WrongPassword9!"
        }, CancellationToken.None);

        Assert.False(result.Succeeded);
        Assert.Equal(LoginFailureReason.LockedOut, result.FailureReason);
        Assert.Equal(5, user.FailedLoginAttemptCount);
        Assert.Equal(nowUtc.AddMinutes(15), user.LockoutEndUtc);
        Assert.Equal(1, lookup.SaveChangesCallCount);
        Assert.Single(audit.LoginFailures);
        Assert.Equal("LockedOut", audit.LoginFailures[0].Reason);
    }

    [Fact]
    public async Task GivenValidTemporaryPassword_WhenLogin_ThenTemporaryPasswordIsConsumedAndPasswordChangeRequired()
    {
        var nowUtc = new DateTime(2026, 7, 24, 12, 0, 0, DateTimeKind.Utc);
        var user = TestUsers.CreateDefault();
        user.PasswordHash = "hash:PrimaryPassword1!";
        user.TemporaryPasswordHash = "hash:TempPassword1!";
        user.TemporaryPasswordExpiresAtUtc = nowUtc.AddHours(2);

        var lookup = new FakeAuthUserLookup(TestUsers.Record(user));
        var audit = new FakeAuthAuditWriter();
        var issuer = new FakeAccessTokenIssuer();
        var service = CreateService(lookup, audit, issuer, nowUtc);

        var result = await service.LoginAsync(new LoginRequestModel
        {
            Email = user.Email,
            Password = "TempPassword1!"
        }, CancellationToken.None);

        Assert.True(result.Succeeded);
        Assert.True(result.Response!.RequiresPasswordChange);
        Assert.Equal("hash:TempPassword1!", user.PasswordHash);
        Assert.Null(user.TemporaryPasswordHash);
        Assert.Null(user.TemporaryPasswordExpiresAtUtc);
        Assert.True(user.MustChangePassword);
        Assert.Equal(1, issuer.IssueCount);
        Assert.Single(audit.LoginSuccesses);
    }

    private static LoginService CreateService(
        FakeAuthUserLookup lookup,
        FakeAuthAuditWriter audit,
        FakeAccessTokenIssuer? issuer = null,
        DateTime? nowUtc = null)
    {
        return new LoginService(
            lookup,
            new FakePasswordHasher(),
            issuer ?? new FakeAccessTokenIssuer(),
            new FixedTimeProvider(nowUtc ?? new DateTime(2026, 7, 24, 12, 0, 0, DateTimeKind.Utc)),
            audit);
    }
}