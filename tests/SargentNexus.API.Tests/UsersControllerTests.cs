using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SargentNexus.API.Controllers;
using SargentNexus.Application.Administration;

namespace SargentNexus.API.Tests;

public sealed class UsersControllerTests
{
    [Fact]
    public async Task CreateUser_WhenServiceSucceeds_ReturnsCreatedWithLocation()
    {
        var actorUserId = Guid.NewGuid();
        var response = new UserCreateResponseModel
        {
            UserId = Guid.NewGuid(),
            OrganizationId = Guid.NewGuid(),
            Email = "user@sargentnexus.test",
            Role = "User",
            Status = "Active"
        };

        var service = new StubService
        {
            CreateUserAsyncHandler = (_, _, _, _) =>
                Task.FromResult(AdministrationResult<UserCreateResponseModel>.Success(response))
        };

        var controller = CreateController(service, actorUserId);

        var result = await controller.CreateUser(
            response.OrganizationId,
            new UserCreateRequestModel
            {
                FirstName = "First",
                LastName = "Last",
                Email = response.Email,
                Role = response.Role,
                InitialPassword = "Password1!",
                Status = response.Status
            },
            CancellationToken.None);

        var created = Assert.IsType<CreatedResult>(result);
        Assert.Equal(StatusCodes.Status201Created, created.StatusCode);
        Assert.Equal($"/api/v1/users/{response.UserId}", created.Location);
        Assert.Same(response, created.Value);
    }

    [Fact]
    public async Task GetUser_WhenServiceReturnsForbidden_ReturnsForbiddenProblem()
    {
        var service = new StubService
        {
            GetUserAsyncHandler = (_, _, _) =>
                Task.FromResult(AdministrationResult<UserDetailModel>.Fail(AdministrationFailureReason.Forbidden))
        };

        var controller = CreateController(service, Guid.NewGuid());

        var result = await controller.GetUser(Guid.NewGuid(), CancellationToken.None);

        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(StatusCodes.Status403Forbidden, objectResult.StatusCode);

        var problem = Assert.IsType<ProblemDetails>(objectResult.Value);
        Assert.Equal(StatusCodes.Status403Forbidden, problem.Status);
        Assert.Equal("Forbidden.", problem.Title);
    }

    [Fact]
    public async Task CreateUser_WhenServiceReturnsNotFound_ReturnsNotFoundProblem()
    {
        var service = new StubService
        {
            CreateUserAsyncHandler = (_, _, _, _) =>
                Task.FromResult(AdministrationResult<UserCreateResponseModel>.Fail(AdministrationFailureReason.NotFound))
        };

        var controller = CreateController(service, Guid.NewGuid());

        var result = await controller.CreateUser(
            Guid.NewGuid(),
            new UserCreateRequestModel
            {
                FirstName = "First",
                LastName = "Last",
                Email = "user@sargentnexus.test",
                Role = "User",
                InitialPassword = "Password1!",
                Status = "Active"
            },
            CancellationToken.None);

        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(StatusCodes.Status404NotFound, objectResult.StatusCode);

        var problem = Assert.IsType<ProblemDetails>(objectResult.Value);
        Assert.Equal(StatusCodes.Status404NotFound, problem.Status);
        Assert.Equal("Not found.", problem.Title);
    }

    [Fact]
    public async Task DownloadImportTemplate_WhenServiceSucceeds_ReturnsCsvFile()
    {
        var organizationId = Guid.NewGuid();
        const string csv = "firstName,lastName,email,role,status,initialPassword\nJane,Doe,jane@example.com,User,Active,Password1!";

        var service = new StubService
        {
            GetUserImportTemplateAsyncHandler = (_, _, _) =>
                Task.FromResult(AdministrationResult<string>.Success(csv))
        };

        var controller = CreateController(service, Guid.NewGuid());
        var result = await controller.DownloadImportTemplate(organizationId, CancellationToken.None);

        var file = Assert.IsType<FileContentResult>(result);
        Assert.Equal("text/csv; charset=utf-8", file.ContentType);
        Assert.Equal("user-import-template.csv", file.FileDownloadName);
        Assert.Equal(csv, Encoding.UTF8.GetString(file.FileContents));
    }

    [Fact]
    public async Task ImportUsers_WhenServiceSucceeds_ReturnsCreated()
    {
        var organizationId = Guid.NewGuid();
        var response = new UserImportResponseModel
        {
            OrganizationId = organizationId,
            CreatedCount = 2
        };

        var service = new StubService
        {
            ImportUsersCsvAsyncHandler = (_, _, _, _) =>
                Task.FromResult(AdministrationResult<UserImportResponseModel>.Success(response))
        };

        var controller = CreateController(service, Guid.NewGuid());
        var formFile = new FormFile(new MemoryStream(Encoding.UTF8.GetBytes("firstName,lastName,email,role,status,initialPassword")), 0, 57, "csvFile", "users.csv");

        var result = await controller.ImportUsers(organizationId, formFile, CancellationToken.None);

        var created = Assert.IsType<CreatedResult>(result);
        Assert.Equal(StatusCodes.Status201Created, created.StatusCode);
        Assert.Same(response, created.Value);
    }

    private static UsersController CreateController(IOrganizationUserAdministrationService service, Guid? actorUserId = null)
    {
        var controller = new UsersController(service)
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

    private sealed class StubService : IOrganizationUserAdministrationService
    {
        public Func<Guid, OrganizationListQueryModel, CancellationToken, Task<AdministrationResult<PagedResultModel<OrganizationListItemModel>>>>? ListOrganizationsAsyncHandler { get; init; }

        public Func<Guid, OrganizationUpsertRequestModel, CancellationToken, Task<AdministrationResult<OrganizationCreateResponseModel>>>? CreateOrganizationAsyncHandler { get; init; }

        public Func<Guid, Guid, CancellationToken, Task<AdministrationResult<OrganizationDetailModel>>>? GetOrganizationAsyncHandler { get; init; }

        public Func<Guid, Guid, OrganizationUpsertRequestModel, CancellationToken, Task<AdministrationResult<OrganizationDetailModel>>>? UpdateOrganizationAsyncHandler { get; init; }

        public Func<Guid, Guid, CancellationToken, Task<AdministrationResult>>? ArchiveOrganizationAsyncHandler { get; init; }

        public Func<Guid, Guid, OrganizationUsersListQueryModel, CancellationToken, Task<AdministrationResult<PagedResultModel<UserSummaryModel>>>>? ListUsersAsyncHandler { get; init; }

        public Func<Guid, Guid, UserCreateRequestModel, CancellationToken, Task<AdministrationResult<UserCreateResponseModel>>>? CreateUserAsyncHandler { get; init; }

        public Func<Guid, Guid, CancellationToken, Task<AdministrationResult<string>>>? GetUserImportTemplateAsyncHandler { get; init; }

        public Func<Guid, Guid, byte[], CancellationToken, Task<AdministrationResult<UserImportResponseModel>>>? ImportUsersCsvAsyncHandler { get; init; }

        public Func<Guid, Guid, CancellationToken, Task<AdministrationResult<UserDetailModel>>>? GetUserAsyncHandler { get; init; }

        public Func<Guid, Guid, UserUpdateRequestModel, CancellationToken, Task<AdministrationResult<UserDetailModel>>>? UpdateUserAsyncHandler { get; init; }

        public Task<AdministrationResult<PagedResultModel<OrganizationListItemModel>>> ListOrganizationsAsync(Guid actorUserId, OrganizationListQueryModel request, CancellationToken cancellationToken)
        {
            return ListOrganizationsAsyncHandler is null
                ? Task.FromResult(AdministrationResult<PagedResultModel<OrganizationListItemModel>>.Fail(AdministrationFailureReason.Forbidden))
                : ListOrganizationsAsyncHandler(actorUserId, request, cancellationToken);
        }

        public Task<AdministrationResult<OrganizationCreateResponseModel>> CreateOrganizationAsync(Guid actorUserId, OrganizationUpsertRequestModel request, CancellationToken cancellationToken)
        {
            return CreateOrganizationAsyncHandler is null
                ? Task.FromResult(AdministrationResult<OrganizationCreateResponseModel>.Fail(AdministrationFailureReason.Forbidden))
                : CreateOrganizationAsyncHandler(actorUserId, request, cancellationToken);
        }

        public Task<AdministrationResult<OrganizationDetailModel>> GetOrganizationAsync(Guid actorUserId, Guid organizationId, CancellationToken cancellationToken)
        {
            return GetOrganizationAsyncHandler is null
                ? Task.FromResult(AdministrationResult<OrganizationDetailModel>.Fail(AdministrationFailureReason.NotFound))
                : GetOrganizationAsyncHandler(actorUserId, organizationId, cancellationToken);
        }

        public Task<AdministrationResult<OrganizationDetailModel>> UpdateOrganizationAsync(Guid actorUserId, Guid organizationId, OrganizationUpsertRequestModel request, CancellationToken cancellationToken)
        {
            return UpdateOrganizationAsyncHandler is null
                ? Task.FromResult(AdministrationResult<OrganizationDetailModel>.Fail(AdministrationFailureReason.NotFound))
                : UpdateOrganizationAsyncHandler(actorUserId, organizationId, request, cancellationToken);
        }

        public Task<AdministrationResult> ArchiveOrganizationAsync(Guid actorUserId, Guid organizationId, CancellationToken cancellationToken)
        {
            return ArchiveOrganizationAsyncHandler is null
                ? Task.FromResult(AdministrationResult.Success())
                : ArchiveOrganizationAsyncHandler(actorUserId, organizationId, cancellationToken);
        }

        public Task<AdministrationResult<string>> RegenerateInviteCodeAsync(Guid actorUserId, Guid organizationId, CancellationToken cancellationToken) =>
            Task.FromResult(AdministrationResult<string>.Fail(AdministrationFailureReason.NotFound));

        public Task<AdministrationResult<OrganizationLogoModel>> UploadOrganizationLogoAsync(
            Guid actorUserId,
            Guid organizationId,
            string contentType,
            byte[] logoBytes,
            CancellationToken cancellationToken)
        {
            return Task.FromResult(AdministrationResult<OrganizationLogoModel>.Fail(AdministrationFailureReason.NotFound));
        }

        public Task<AdministrationResult<PagedResultModel<UserSummaryModel>>> ListUsersAsync(Guid actorUserId, Guid organizationId, OrganizationUsersListQueryModel request, CancellationToken cancellationToken)
        {
            return ListUsersAsyncHandler is null
                ? Task.FromResult(AdministrationResult<PagedResultModel<UserSummaryModel>>.Fail(AdministrationFailureReason.Forbidden))
                : ListUsersAsyncHandler(actorUserId, organizationId, request, cancellationToken);
        }

        public Task<AdministrationResult<UserCreateResponseModel>> CreateUserAsync(Guid actorUserId, Guid organizationId, UserCreateRequestModel request, CancellationToken cancellationToken)
        {
            return CreateUserAsyncHandler is null
                ? Task.FromResult(AdministrationResult<UserCreateResponseModel>.Fail(AdministrationFailureReason.Forbidden))
                : CreateUserAsyncHandler(actorUserId, organizationId, request, cancellationToken);
        }

        public Task<AdministrationResult<string>> GetUserImportTemplateAsync(Guid actorUserId, Guid organizationId, CancellationToken cancellationToken)
        {
            return GetUserImportTemplateAsyncHandler is null
                ? Task.FromResult(AdministrationResult<string>.Fail(AdministrationFailureReason.Forbidden))
                : GetUserImportTemplateAsyncHandler(actorUserId, organizationId, cancellationToken);
        }

        public Task<AdministrationResult<UserImportResponseModel>> ImportUsersCsvAsync(Guid actorUserId, Guid organizationId, byte[] csvBytes, CancellationToken cancellationToken)
        {
            return ImportUsersCsvAsyncHandler is null
                ? Task.FromResult(AdministrationResult<UserImportResponseModel>.Fail(AdministrationFailureReason.Forbidden))
                : ImportUsersCsvAsyncHandler(actorUserId, organizationId, csvBytes, cancellationToken);
        }

        public Task<AdministrationResult<UserDetailModel>> GetUserAsync(Guid actorUserId, Guid userId, CancellationToken cancellationToken)
        {
            return GetUserAsyncHandler is null
                ? Task.FromResult(AdministrationResult<UserDetailModel>.Fail(AdministrationFailureReason.NotFound))
                : GetUserAsyncHandler(actorUserId, userId, cancellationToken);
        }

        public Task<AdministrationResult<UserDetailModel>> UpdateUserAsync(Guid actorUserId, Guid userId, UserUpdateRequestModel request, CancellationToken cancellationToken)
        {
            return UpdateUserAsyncHandler is null
                ? Task.FromResult(AdministrationResult<UserDetailModel>.Fail(AdministrationFailureReason.NotFound))
                : UpdateUserAsyncHandler(actorUserId, userId, request, cancellationToken);
        }
    }
}
