using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SargentNexus.API.Controllers;
using SargentNexus.Application.Administration;
using SargentNexus.Application.Auth;

namespace SargentNexus.API.Tests;

public sealed class AuthControllerTests
{
    [Fact]
    public void LoginContracts_DoNotExposeLegacyOrganizationSelectionFields()
    {
        Assert.Null(typeof(LoginRequestModel).GetProperty("OrganizationId"));
        Assert.Null(typeof(LoginResponseModel).GetProperty("RequiresOrganizationSelection"));
        Assert.Null(typeof(LoginResponseModel).GetProperty("Organizations"));
    }

    [Fact]
    public async Task Login_WhenServiceSucceeds_ReturnsOkWithResponse()
    {
        var response = new LoginResponseModel
        {
            AccessToken = "token-1",
            ExpiresInSeconds = 3600
        };

        var loginService = new StubLoginService(_ => Task.FromResult(LoginResult.Success(response)));
        var authAccountService = new StubAuthAccountService();
        var controller = CreateController(loginService, authAccountService);

        var result = await controller.Login(new LoginRequestModel
        {
            Email = "user@sargentnexus.test",
            Password = "Password1!"
        }, CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(StatusCodes.Status200OK, ok.StatusCode);
        Assert.Same(response, ok.Value);
        Assert.NotNull(loginService.LastRequest);
        Assert.Equal("user@sargentnexus.test", loginService.LastRequest!.Email);
        Assert.Equal("Password1!", loginService.LastRequest.Password);
    }

    [Fact]
    public async Task Login_WhenSeededAdminLoginSucceeds_ReturnsOkWithPasswordChangeFlag()
    {
        var response = new LoginResponseModel
        {
            AccessToken = "admin-token",
            ExpiresInSeconds = 3600,
            RequiresPasswordChange = true,
            User = new LoginUserModel
            {
                UserId = Guid.NewGuid(),
                OrganizationId = null,
                Role = "SiteAdmin",
                FirstName = "Site",
                LastName = "Admin",
                Email = "siteadmin@sargentnexus.local",
                Status = "Active"
            }
        };

        var loginService = new StubLoginService(_ => Task.FromResult(LoginResult.Success(response)));
        var controller = CreateController(loginService, new StubAuthAccountService());

        var result = await controller.Login(new LoginRequestModel
        {
            Email = "siteadmin@sargentnexus.local",
            Password = "Abc123!Demo"
        }, CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(StatusCodes.Status200OK, ok.StatusCode);
        Assert.Same(response, ok.Value);
        Assert.NotNull(loginService.LastRequest);
        Assert.Equal("siteadmin@sargentnexus.local", loginService.LastRequest!.Email);
        Assert.Equal("Abc123!Demo", loginService.LastRequest.Password);
    }

    [Theory]
    [InlineData(LoginFailureReason.InvalidCredentials, StatusCodes.Status401Unauthorized, "Invalid credentials.")]
    [InlineData(LoginFailureReason.InactiveUser, StatusCodes.Status403Forbidden, "User account is inactive.")]
    [InlineData(LoginFailureReason.LockedOut, StatusCodes.Status429TooManyRequests, "User account is locked out.")]
    public async Task Login_WhenServiceFails_MapsFailureReasonToProblemDetails(
        LoginFailureReason failureReason,
        int expectedStatus,
        string expectedTitle)
    {
        var loginService = new StubLoginService(_ => Task.FromResult(LoginResult.Failure(failureReason)));
        var authAccountService = new StubAuthAccountService();
        var controller = CreateController(loginService, authAccountService);

        var result = await controller.Login(new LoginRequestModel
        {
            Email = "user@sargentnexus.test",
            Password = "wrong"
        }, CancellationToken.None);

        AssertProblem(result, expectedStatus, expectedTitle);
    }

    [Fact]
    public async Task Me_WhenNoAuthenticatedUserId_ReturnsUnauthorizedProblem()
    {
        var controller = CreateController(new StubLoginService(), new StubAuthAccountService());

        var result = await controller.Me(CancellationToken.None);

        AssertProblem(result, StatusCodes.Status401Unauthorized, "Authentication required.");
    }

    [Fact]
    public async Task Me_WhenAuthenticatedUserIsMissing_ReturnsUnauthorizedProblem()
    {
        var userId = Guid.NewGuid();
        var authAccountService = new StubAuthAccountService
        {
            GetCurrentUserAsyncHandler = (_, _) => Task.FromResult<AuthenticatedUserModel?>(null)
        };
        var controller = CreateController(new StubLoginService(), authAccountService, userId);

        var result = await controller.Me(CancellationToken.None);

        AssertProblem(result, StatusCodes.Status401Unauthorized, "Authentication required.");
    }

    [Fact]
    public async Task Me_WhenAuthenticatedUserExists_ReturnsOkWithUser()
    {
        var userId = Guid.NewGuid();
        var user = new AuthenticatedUserModel
        {
            UserId = userId,
            Email = "user@sargentnexus.test",
            Role = "User",
            FirstName = "Test",
            LastName = "User",
            Status = "Active"
        };

        var authAccountService = new StubAuthAccountService
        {
            GetCurrentUserAsyncHandler = (_, _) => Task.FromResult<AuthenticatedUserModel?>(user)
        };

        var controller = CreateController(new StubLoginService(), authAccountService, userId);

        var result = await controller.Me(CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(StatusCodes.Status200OK, ok.StatusCode);
        Assert.Same(user, ok.Value);
        Assert.Equal(userId, authAccountService.LastGetCurrentUserId);
    }

    [Fact]
    public async Task UpdateMe_WhenServiceSucceeds_ReturnsUpdatedUser()
    {
        var userId = Guid.NewGuid();
        var updatedUser = new AuthenticatedUserModel
        {
            UserId = userId,
            FirstName = "Ada",
            LastName = "Lovelace",
            Email = "ada@sargentnexus.test",
            Role = "User",
            Status = "Active"
        };
        var authAccountService = new StubAuthAccountService
        {
            UpdateProfileAsyncHandler = (_, _, _) => Task.FromResult(UpdateProfileResult.Success(updatedUser))
        };
        var controller = CreateController(new StubLoginService(), authAccountService, userId);
        var request = new UpdateProfileRequestModel
        {
            FirstName = "Ada",
            LastName = "Lovelace"
        };

        var result = await controller.UpdateMe(request, CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Same(updatedUser, ok.Value);
        Assert.Equal(userId, authAccountService.LastUpdateProfileUserId);
        Assert.Same(request, authAccountService.LastUpdateProfileRequest);
    }

    [Fact]
    public async Task ChangePassword_WhenNoAuthenticatedUserId_ReturnsUnauthorizedProblem()
    {
        var controller = CreateController(new StubLoginService(), new StubAuthAccountService());

        var result = await controller.ChangePassword(new ChangePasswordRequestModel
        {
            CurrentPassword = "old",
            NewPassword = "new"
        }, CancellationToken.None);

        AssertProblem(result, StatusCodes.Status401Unauthorized, "Authentication required.");
    }

    [Fact]
    public async Task ChangePassword_WhenServiceSucceeds_ReturnsNoContent()
    {
        var userId = Guid.NewGuid();
        var authAccountService = new StubAuthAccountService
        {
            ChangePasswordAsyncHandler = (_, _, _) => Task.FromResult(ChangePasswordResult.Success())
        };
        var controller = CreateController(new StubLoginService(), authAccountService, userId);

        var request = new ChangePasswordRequestModel
        {
            CurrentPassword = "old",
            NewPassword = "new"
        };

        var result = await controller.ChangePassword(request, CancellationToken.None);

        var noContent = Assert.IsType<NoContentResult>(result);
        Assert.Equal(StatusCodes.Status204NoContent, noContent.StatusCode);
        Assert.Equal(userId, authAccountService.LastChangePasswordUserId);
        Assert.Same(request, authAccountService.LastChangePasswordRequest);
    }

    [Fact]
    public async Task ChangePassword_WhenInvalidCurrentPassword_ReturnsUnauthorizedProblem()
    {
        var userId = Guid.NewGuid();
        var authAccountService = new StubAuthAccountService
        {
            ChangePasswordAsyncHandler = (_, _, _) => Task.FromResult(ChangePasswordResult.Failure(ChangePasswordFailureReason.InvalidCurrentPassword))
        };
        var controller = CreateController(new StubLoginService(), authAccountService, userId);

        var result = await controller.ChangePassword(new ChangePasswordRequestModel
        {
            CurrentPassword = "wrong",
            NewPassword = "new"
        }, CancellationToken.None);

        AssertProblem(result, StatusCodes.Status401Unauthorized, "Invalid current password.");
    }

    [Fact]
    public async Task ChangePassword_WhenPasswordPolicyFails_ReturnsValidationProblemDetails()
    {
        var userId = Guid.NewGuid();
        var policyErrors = new[] { "Must include uppercase", "Must include number" };
        var authAccountService = new StubAuthAccountService
        {
            ChangePasswordAsyncHandler = (_, _, _) => Task.FromResult(
                ChangePasswordResult.Failure(ChangePasswordFailureReason.InvalidPasswordPolicy, policyErrors))
        };
        var controller = CreateController(new StubLoginService(), authAccountService, userId);

        var request = new ChangePasswordRequestModel
        {
            CurrentPassword = "old",
            NewPassword = "weak"
        };

        var result = await controller.ChangePassword(request, CancellationToken.None);

        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal(StatusCodes.Status400BadRequest, badRequest.StatusCode);

        var validationProblem = Assert.IsType<ValidationProblemDetails>(badRequest.Value);
        Assert.Equal(StatusCodes.Status400BadRequest, validationProblem.Status);
        Assert.Equal("One or more validation errors occurred.", validationProblem.Title);
        Assert.True(validationProblem.Errors.TryGetValue(nameof(request.NewPassword), out var errors));
        Assert.Equal(policyErrors, errors);
    }

    [Fact]
    public async Task ChangePassword_WhenUserCannotBeResolved_ReturnsUnauthorizedProblem()
    {
        var userId = Guid.NewGuid();
        var authAccountService = new StubAuthAccountService
        {
            ChangePasswordAsyncHandler = (_, _, _) => Task.FromResult(
                ChangePasswordResult.Failure(ChangePasswordFailureReason.UserNotFound))
        };
        var controller = CreateController(new StubLoginService(), authAccountService, userId);

        var result = await controller.ChangePassword(new ChangePasswordRequestModel
        {
            CurrentPassword = "old",
            NewPassword = "new"
        }, CancellationToken.None);

        AssertProblem(result, StatusCodes.Status401Unauthorized, "Authentication required.");
    }

    [Fact]
    public async Task IssueTemporaryPassword_WhenNoAuthenticatedUserId_ReturnsUnauthorizedProblem()
    {
        var controller = CreateController(new StubLoginService(), new StubAuthAccountService());

        var result = await controller.IssueTemporaryPassword(Guid.NewGuid(), CancellationToken.None);

        AssertProblem(result, StatusCodes.Status401Unauthorized, "Authentication required.");
    }

    [Fact]
    public async Task IssueTemporaryPassword_WhenServiceSucceeds_ReturnsOkWithResponse()
    {
        var actorUserId = Guid.NewGuid();
        var targetUserId = Guid.NewGuid();
        var response = new TemporaryPasswordResponseModel
        {
            TemporaryPassword = "Temp123!",
            MustChangePassword = true
        };

        var authAccountService = new StubAuthAccountService
        {
            IssueTemporaryPasswordAsyncHandler = (_, _, _) => Task.FromResult(TemporaryPasswordResult.Success(response))
        };
        var controller = CreateController(new StubLoginService(), authAccountService, actorUserId);

        var result = await controller.IssueTemporaryPassword(targetUserId, CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(StatusCodes.Status200OK, ok.StatusCode);
        Assert.Same(response, ok.Value);
        Assert.Equal(actorUserId, authAccountService.LastIssueTemporaryPasswordActorUserId);
        Assert.Equal(targetUserId, authAccountService.LastIssueTemporaryPasswordTargetUserId);
    }

    [Theory]
    [InlineData(TemporaryPasswordFailureReason.Forbidden, StatusCodes.Status403Forbidden, "Forbidden.")]
    [InlineData(TemporaryPasswordFailureReason.UserNotFound, StatusCodes.Status404NotFound, "User not found.")]
    [InlineData((TemporaryPasswordFailureReason)999, StatusCodes.Status400BadRequest, "Unable to issue temporary password.")]
    public async Task IssueTemporaryPassword_WhenServiceFails_MapsFailureReasonToProblemDetails(
        TemporaryPasswordFailureReason failureReason,
        int expectedStatus,
        string expectedTitle)
    {
        var actorUserId = Guid.NewGuid();
        var authAccountService = new StubAuthAccountService
        {
            IssueTemporaryPasswordAsyncHandler = (_, _, _) => Task.FromResult(TemporaryPasswordResult.Failure(failureReason))
        };
        var controller = CreateController(new StubLoginService(), authAccountService, actorUserId);

        var result = await controller.IssueTemporaryPassword(Guid.NewGuid(), CancellationToken.None);

        AssertProblem(result, expectedStatus, expectedTitle);
    }

    private static AuthController CreateController(
        ILoginService loginService,
        IAuthAccountService authAccountService,
        Guid? authenticatedUserId = null)
    {
        var selfRegService = new StubSelfRegistrationService();
        var controller = new AuthController(loginService, authAccountService, selfRegService)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            }
        };

        if (authenticatedUserId.HasValue)
        {
            controller.ControllerContext.HttpContext.User = CreatePrincipal(authenticatedUserId.Value);
        }

        return controller;
    }

    private static ClaimsPrincipal CreatePrincipal(Guid userId)
    {
        var identity = new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString())
        }, "TestAuth");

        return new ClaimsPrincipal(identity);
    }

    private static void AssertProblem(IActionResult result, int expectedStatus, string expectedTitle)
    {
        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(expectedStatus, objectResult.StatusCode);

        var problem = Assert.IsType<ProblemDetails>(objectResult.Value);
        Assert.Equal(expectedStatus, problem.Status);
        Assert.Equal(expectedTitle, problem.Title);
    }

    private sealed class StubLoginService : ILoginService
    {
        private readonly Func<LoginRequestModel, Task<LoginResult>> _handler;
        public LoginRequestModel? LastRequest { get; private set; }

        public StubLoginService(Func<LoginRequestModel, Task<LoginResult>>? handler = null)
        {
            _handler = handler ?? (_ => Task.FromResult(LoginResult.Failure(LoginFailureReason.InvalidCredentials)));
        }

        public Task<LoginResult> LoginAsync(LoginRequestModel request, CancellationToken cancellationToken)
        {
            LastRequest = request;
            return _handler(request);
        }
    }

    private sealed class StubAuthAccountService : IAuthAccountService
    {
        public Func<Guid, CancellationToken, Task<AuthenticatedUserModel?>>? GetCurrentUserAsyncHandler { get; init; }

        public Func<Guid, UpdateProfileRequestModel, CancellationToken, Task<UpdateProfileResult>>? UpdateProfileAsyncHandler { get; init; }

        public Func<Guid, ChangePasswordRequestModel, CancellationToken, Task<ChangePasswordResult>>? ChangePasswordAsyncHandler { get; init; }

        public Func<Guid, Guid, CancellationToken, Task<TemporaryPasswordResult>>? IssueTemporaryPasswordAsyncHandler { get; init; }

        public Guid? LastGetCurrentUserId { get; private set; }

        public Guid? LastUpdateProfileUserId { get; private set; }

        public UpdateProfileRequestModel? LastUpdateProfileRequest { get; private set; }

        public Guid? LastChangePasswordUserId { get; private set; }

        public ChangePasswordRequestModel? LastChangePasswordRequest { get; private set; }

        public Guid? LastIssueTemporaryPasswordActorUserId { get; private set; }

        public Guid? LastIssueTemporaryPasswordTargetUserId { get; private set; }

        public Task<AuthenticatedUserModel?> GetCurrentUserAsync(Guid userId, CancellationToken cancellationToken)
        {
            LastGetCurrentUserId = userId;
            return GetCurrentUserAsyncHandler is null
                ? Task.FromResult<AuthenticatedUserModel?>(null)
                : GetCurrentUserAsyncHandler(userId, cancellationToken);
        }

        public Task<UpdateProfileResult> UpdateProfileAsync(
            Guid userId,
            UpdateProfileRequestModel request,
            CancellationToken cancellationToken)
        {
            LastUpdateProfileUserId = userId;
            LastUpdateProfileRequest = request;
            return UpdateProfileAsyncHandler is null
                ? Task.FromResult(UpdateProfileResult.Failure(UpdateProfileFailureReason.UserNotFound))
                : UpdateProfileAsyncHandler(userId, request, cancellationToken);
        }

        public Task<ChangePasswordResult> ChangePasswordAsync(
            Guid userId,
            ChangePasswordRequestModel request,
            CancellationToken cancellationToken)
        {
            LastChangePasswordUserId = userId;
            LastChangePasswordRequest = request;
            return ChangePasswordAsyncHandler is null
                ? Task.FromResult(ChangePasswordResult.Failure(ChangePasswordFailureReason.UserNotFound))
                : ChangePasswordAsyncHandler(userId, request, cancellationToken);
        }

        public Task<TemporaryPasswordResult> IssueTemporaryPasswordAsync(
            Guid actorUserId,
            Guid targetUserId,
            CancellationToken cancellationToken)
        {
            LastIssueTemporaryPasswordActorUserId = actorUserId;
            LastIssueTemporaryPasswordTargetUserId = targetUserId;
            return IssueTemporaryPasswordAsyncHandler is null
                ? Task.FromResult(TemporaryPasswordResult.Failure(TemporaryPasswordFailureReason.Forbidden))
                : IssueTemporaryPasswordAsyncHandler(actorUserId, targetUserId, cancellationToken);
        }
    }

    private sealed class StubSelfRegistrationService : ISelfRegistrationService
    {
        public Task<SelfRegistrationResult> RegisterAsync(SelfRegistrationRequestModel request, CancellationToken cancellationToken)
            => Task.FromResult(SelfRegistrationResult.Fail(SelfRegistrationFailureReason.InvalidInviteCode));
    }
}
