using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SargentNexus.API.Controllers;
using SargentNexus.Application.Administration;

namespace SargentNexus.API.Tests;

public sealed class OrganizationsControllerTests
{
    [Fact]
    public async Task ListOrganizations_WhenNoAuthenticatedUser_ReturnsUnauthorizedProblem()
    {
        var controller = CreateController(new StubService());

        var result = await controller.ListOrganizations(new OrganizationListQueryModel(), CancellationToken.None);

        AssertProblem(result, StatusCodes.Status401Unauthorized, "Authentication required.");
    }

    [Fact]
    public async Task CreateOrganization_WhenServiceSucceeds_ReturnsCreated()
    {
        var actorUserId = Guid.NewGuid();
        var response = new OrganizationCreateResponseModel
        {
            OrganizationId = Guid.NewGuid(),
            DefaultBoardId = Guid.NewGuid(),
            DefaultStatusCount = 5
        };

        var service = new StubService
        {
            CreateOrganizationAsyncHandler = (_, _, _) => Task.FromResult(AdministrationResult<OrganizationCreateResponseModel>.Success(response))
        };

        var controller = CreateController(service, actorUserId);

        var result = await controller.CreateOrganization(new OrganizationUpsertRequestModel
        {
            CompanyName = "Acme",
            Address = "Address",
            City = "City",
            State = "ST",
            Zip = "00000",
            Phone = "555",
            PrimaryContactFirstName = "First",
            PrimaryContactLastName = "Last"
        }, CancellationToken.None);

        var created = Assert.IsType<CreatedResult>(result);
        Assert.Equal(StatusCodes.Status201Created, created.StatusCode);
        Assert.Same(response, created.Value);
    }

    [Fact]
    public async Task ArchiveOrganization_WhenServiceReturnsNotFound_ReturnsNotFoundProblem()
    {
        var service = new StubService
        {
            ArchiveOrganizationAsyncHandler = (_, _, _) => Task.FromResult(AdministrationResult.Fail(AdministrationFailureReason.NotFound))
        };

        var controller = CreateController(service, Guid.NewGuid());

        var result = await controller.ArchiveOrganization(Guid.NewGuid(), CancellationToken.None);

        AssertProblem(result, StatusCodes.Status404NotFound, "Not found.");
    }

    private static OrganizationsController CreateController(IOrganizationUserAdministrationService service, Guid? actorUserId = null)
    {
        var controller = new OrganizationsController(service)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            }
        };

        if (actorUserId.HasValue)
        {
            controller.ControllerContext.HttpContext.User = CreatePrincipal(actorUserId.Value);
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

    private sealed class StubService : IOrganizationUserAdministrationService
    {
        public Func<Guid, OrganizationListQueryModel, CancellationToken, Task<AdministrationResult<PagedResultModel<OrganizationListItemModel>>>>? ListOrganizationsAsyncHandler { get; init; }

        public Func<Guid, OrganizationUpsertRequestModel, CancellationToken, Task<AdministrationResult<OrganizationCreateResponseModel>>>? CreateOrganizationAsyncHandler { get; init; }

        public Func<Guid, Guid, CancellationToken, Task<AdministrationResult<OrganizationDetailModel>>>? GetOrganizationAsyncHandler { get; init; }

        public Func<Guid, Guid, OrganizationUpsertRequestModel, CancellationToken, Task<AdministrationResult<OrganizationDetailModel>>>? UpdateOrganizationAsyncHandler { get; init; }

        public Func<Guid, Guid, CancellationToken, Task<AdministrationResult>>? ArchiveOrganizationAsyncHandler { get; init; }

        public Func<Guid, Guid, OrganizationUsersListQueryModel, CancellationToken, Task<AdministrationResult<PagedResultModel<UserSummaryModel>>>>? ListUsersAsyncHandler { get; init; }

        public Func<Guid, Guid, UserCreateRequestModel, CancellationToken, Task<AdministrationResult<UserCreateResponseModel>>>? CreateUserAsyncHandler { get; init; }

        public Func<Guid, Guid, CancellationToken, Task<AdministrationResult<UserDetailModel>>>? GetUserAsyncHandler { get; init; }

        public Func<Guid, Guid, UserUpdateRequestModel, CancellationToken, Task<AdministrationResult<UserDetailModel>>>? UpdateUserAsyncHandler { get; init; }

        public Task<AdministrationResult> ArchiveOrganizationAsync(Guid actorUserId, Guid organizationId, CancellationToken cancellationToken)
        {
            return ArchiveOrganizationAsyncHandler is null
                ? Task.FromResult(AdministrationResult.Success())
                : ArchiveOrganizationAsyncHandler(actorUserId, organizationId, cancellationToken);
        }

        public Task<AdministrationResult<OrganizationCreateResponseModel>> CreateOrganizationAsync(Guid actorUserId, OrganizationUpsertRequestModel request, CancellationToken cancellationToken)
        {
            return CreateOrganizationAsyncHandler is null
                ? Task.FromResult(AdministrationResult<OrganizationCreateResponseModel>.Fail(AdministrationFailureReason.Forbidden))
                : CreateOrganizationAsyncHandler(actorUserId, request, cancellationToken);
        }

        public Task<AdministrationResult<UserCreateResponseModel>> CreateUserAsync(Guid actorUserId, Guid organizationId, UserCreateRequestModel request, CancellationToken cancellationToken)
        {
            return CreateUserAsyncHandler is null
                ? Task.FromResult(AdministrationResult<UserCreateResponseModel>.Fail(AdministrationFailureReason.Forbidden))
                : CreateUserAsyncHandler(actorUserId, organizationId, request, cancellationToken);
        }

        public Task<AdministrationResult<OrganizationDetailModel>> GetOrganizationAsync(Guid actorUserId, Guid organizationId, CancellationToken cancellationToken)
        {
            return GetOrganizationAsyncHandler is null
                ? Task.FromResult(AdministrationResult<OrganizationDetailModel>.Fail(AdministrationFailureReason.NotFound))
                : GetOrganizationAsyncHandler(actorUserId, organizationId, cancellationToken);
        }

        public Task<AdministrationResult<UserDetailModel>> GetUserAsync(Guid actorUserId, Guid userId, CancellationToken cancellationToken)
        {
            return GetUserAsyncHandler is null
                ? Task.FromResult(AdministrationResult<UserDetailModel>.Fail(AdministrationFailureReason.NotFound))
                : GetUserAsyncHandler(actorUserId, userId, cancellationToken);
        }

        public Task<AdministrationResult<PagedResultModel<OrganizationListItemModel>>> ListOrganizationsAsync(Guid actorUserId, OrganizationListQueryModel request, CancellationToken cancellationToken)
        {
            return ListOrganizationsAsyncHandler is null
                ? Task.FromResult(AdministrationResult<PagedResultModel<OrganizationListItemModel>>.Fail(AdministrationFailureReason.Forbidden))
                : ListOrganizationsAsyncHandler(actorUserId, request, cancellationToken);
        }

        public Task<AdministrationResult<PagedResultModel<UserSummaryModel>>> ListUsersAsync(Guid actorUserId, Guid organizationId, OrganizationUsersListQueryModel request, CancellationToken cancellationToken)
        {
            return ListUsersAsyncHandler is null
                ? Task.FromResult(AdministrationResult<PagedResultModel<UserSummaryModel>>.Fail(AdministrationFailureReason.Forbidden))
                : ListUsersAsyncHandler(actorUserId, organizationId, request, cancellationToken);
        }

        public Task<AdministrationResult<OrganizationDetailModel>> UpdateOrganizationAsync(Guid actorUserId, Guid organizationId, OrganizationUpsertRequestModel request, CancellationToken cancellationToken)
        {
            return UpdateOrganizationAsyncHandler is null
                ? Task.FromResult(AdministrationResult<OrganizationDetailModel>.Fail(AdministrationFailureReason.NotFound))
                : UpdateOrganizationAsyncHandler(actorUserId, organizationId, request, cancellationToken);
        }

        public Task<AdministrationResult<UserDetailModel>> UpdateUserAsync(Guid actorUserId, Guid userId, UserUpdateRequestModel request, CancellationToken cancellationToken)
        {
            return UpdateUserAsyncHandler is null
                ? Task.FromResult(AdministrationResult<UserDetailModel>.Fail(AdministrationFailureReason.NotFound))
                : UpdateUserAsyncHandler(actorUserId, userId, request, cancellationToken);
        }
    }
}
