using System.Reflection;
using System.Text.RegularExpressions;
using SargentNexus.Application.Auth;

namespace SargentNexus.Infrastructure.Tests;

public sealed class Pbkdf2PasswordHasherTests
{
    [Fact]
    public void Hash_AndVerify_WithCorrectAndWrongPasswords_BehavesAsExpected()
    {
        var hasher = TestActivator.CreateInternal<IPasswordHasher>("SargentNexus.Infrastructure.Pbkdf2PasswordHasher");

        const string password = "Password1!";
        var hash = hasher.Hash(password);

        var parts = hash.Split('$');
        Assert.Equal(5, parts.Length);
        Assert.Equal("pbkdf2", parts[0]);
        Assert.Equal("SHA256", parts[1]);
        Assert.Equal("100000", parts[2]);
        _ = Convert.FromBase64String(parts[3]);
        _ = Convert.FromBase64String(parts[4]);

        Assert.True(hasher.Verify(password, hash));
        Assert.False(hasher.Verify("WrongPassword1!", hash));
    }

    [Theory]
    [InlineData("")]
    [InlineData("plain-text-password")]
    [InlineData("bcrypt$something$else")]
    [InlineData("pbkdf2$SHA256$not-a-number$salt$hash")]
    [InlineData("pbkdf2$SHA256$100000$***$###")]
    public void Verify_WithMalformedHashFormat_ReturnsFalse(string storedHash)
    {
        var hasher = TestActivator.CreateInternal<IPasswordHasher>("SargentNexus.Infrastructure.Pbkdf2PasswordHasher");

        var verified = hasher.Verify("Password1!", storedHash);

        Assert.False(verified);
    }
}

public sealed class PasswordPolicyValidatorTests
{
    [Fact]
    public void Validate_WithCompliantPassword_ReturnsNoErrors()
    {
        var validator = TestActivator.CreateInternal<IPasswordPolicyValidator>("SargentNexus.Infrastructure.PasswordPolicyValidator");

        var result = validator.Validate("ValidPassword1!");

        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void Validate_WithMissingRequirements_ReturnsAllMatchingErrors()
    {
        var validator = TestActivator.CreateInternal<IPasswordPolicyValidator>("SargentNexus.Infrastructure.PasswordPolicyValidator");

        var result = validator.Validate("abc");

        Assert.False(result.IsValid);
        Assert.Equal(4, result.Errors.Count);
        Assert.Contains("Password must be at least 8 characters long.", result.Errors);
        Assert.Contains("Password must contain at least one uppercase letter.", result.Errors);
        Assert.Contains("Password must contain at least one number.", result.Errors);
        Assert.Contains("Password must contain at least one special character.", result.Errors);
    }

    [Fact]
    public void Validate_WithNoLowercase_ReturnsLowercaseError()
    {
        var validator = TestActivator.CreateInternal<IPasswordPolicyValidator>("SargentNexus.Infrastructure.PasswordPolicyValidator");

        var result = validator.Validate("UPPER123!");

        Assert.False(result.IsValid);
        Assert.Contains("Password must contain at least one lowercase letter.", result.Errors);
    }
}

public sealed class InMemoryAccessTokenStoreTests
{
    [Fact]
    public void Read_WithStoredUnexpiredToken_ReturnsPrincipal()
    {
        var store = TestActivator.CreateInternalObject("SargentNexus.Infrastructure.InMemoryAccessTokenStore");
        var reader = (IAccessTokenReader)store;
        var accessToken = Guid.NewGuid().ToString("N");
        var principal = new AccessTokenPrincipal
        {
            UserId = Guid.NewGuid(),
            OrganizationId = Guid.NewGuid(),
            Role = "OrgAdmin",
            Email = "user@sargentnexus.test",
            ExpiresAtUtc = DateTime.UtcNow.AddHours(2)
        };

        TestActivator.InvokeStore(store, accessToken, principal);

        var readPrincipal = reader.Read(accessToken);

        Assert.NotNull(readPrincipal);
        Assert.Equal(principal.UserId, readPrincipal!.UserId);
        Assert.Equal(principal.OrganizationId, readPrincipal.OrganizationId);
        Assert.Equal(principal.Role, readPrincipal.Role);
        Assert.Equal(principal.Email, readPrincipal.Email);
        Assert.Equal(principal.ExpiresAtUtc, readPrincipal.ExpiresAtUtc);
    }

    [Fact]
    public void Read_WithExpiredToken_ReturnsNull()
    {
        var store = TestActivator.CreateInternalObject("SargentNexus.Infrastructure.InMemoryAccessTokenStore");
        var reader = (IAccessTokenReader)store;
        var accessToken = Guid.NewGuid().ToString("N");
        var principal = new AccessTokenPrincipal
        {
            UserId = Guid.NewGuid(),
            OrganizationId = null,
            Role = "User",
            Email = "expired@sargentnexus.test",
            ExpiresAtUtc = DateTime.UtcNow.AddMinutes(-5)
        };

        TestActivator.InvokeStore(store, accessToken, principal);

        Assert.Null(reader.Read(accessToken));
        Assert.Null(reader.Read(accessToken));
    }
}

public sealed class TemporaryPasswordGeneratorTests
{
    [Fact]
    public void Generate_ReturnsPasswordMatchingExpectedContract()
    {
        var generator = TestActivator.CreateInternal<ITemporaryPasswordGenerator>("SargentNexus.Infrastructure.TemporaryPasswordGenerator");

        var password = generator.Generate();

        Assert.Equal(16, password.Length);
        Assert.Matches(new Regex("^Tmp[0-9A-F]{2}a[0-9A-F]{2}Z[0-9A-F]{2}[!@#$%^&*()\\-_=+\\[\\]{}][0-9A-F]{4}$"), password);
    }
}

internal static class TestActivator
{
    public static T CreateInternal<T>(string fullTypeName) where T : class
    {
        return (T)CreateInternalObject(fullTypeName);
    }

    public static object CreateInternalObject(string fullTypeName)
    {
        var assembly = typeof(SargentNexusDbContext).Assembly;
        var type = assembly.GetType(fullTypeName, throwOnError: true)!;

        return Activator.CreateInstance(type, nonPublic: true)
            ?? throw new InvalidOperationException($"Unable to create type {fullTypeName}.");
    }

    public static void InvokeStore(object instance, string accessToken, AccessTokenPrincipal principal)
    {
        var storeMethod = instance.GetType().GetMethod(
            "Store",
            BindingFlags.Instance | BindingFlags.Public,
            binder: null,
            types: new[] { typeof(string), typeof(AccessTokenPrincipal) },
            modifiers: null);

        if (storeMethod is null)
        {
            throw new InvalidOperationException("Store method not found on InMemoryAccessTokenStore.");
        }

        storeMethod.Invoke(instance, new object[] { accessToken, principal });
    }
}