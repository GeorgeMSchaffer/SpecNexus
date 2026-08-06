using System.Text;
using SargentNexus.Application.Workflow;
using SargentNexus.Domain;

namespace SargentNexus.Application.Tests;

public sealed class WorkflowManagementServiceTests
{
    [Fact]
    public async Task ListIdeaTypes_WhenSiteAdminTargetsOrganization_ReturnsActiveAndArchivedInConfiguredOrder()
    {
        var fixture = new WorkflowFixture();
        var archived = new IdeaType
        {
            Id = Guid.NewGuid(),
            OrganizationId = fixture.Organization.Id,
            Name = "Archived",
            SortOrder = 0,
            IsDeleted = true
        };
        fixture.IdeaType.SortOrder = 1;
        fixture.DataAccess.SeedIdeaType(archived);

        var result = await fixture.Service.ListIdeaTypesAsync(
            fixture.SiteAdminActor,
            fixture.Organization.Id,
            CancellationToken.None);

        Assert.True(result.Succeeded);
        Assert.Equal(new[] { archived.Id, fixture.IdeaType.Id }, result.Response!.Select(item => item.IdeaTypeId));
        Assert.True(result.Response![0].IsDeleted);
    }

    [Fact]
    public async Task CreateIdeaType_WhenUserAttemptsManagement_ReturnsForbidden()
    {
        var fixture = new WorkflowFixture();

        var result = await fixture.Service.CreateIdeaTypeAsync(
            fixture.StandardUserActor,
            fixture.Organization.Id,
            new IdeaTypeWriteRequestModel { Name = "New Type" },
            CancellationToken.None);

        Assert.False(result.Succeeded);
        Assert.Equal(WorkflowFailureReason.Forbidden, result.FailureReason);
        Assert.Equal(0, fixture.DataAccess.SaveChangesCallCount);
    }

    [Fact]
    public async Task CreateIdeaType_WhenTrimmedNameDuplicatesActiveName_ReturnsValidationError()
    {
        var fixture = new WorkflowFixture();

        var result = await fixture.Service.CreateIdeaTypeAsync(
            fixture.OrgAdminActor,
            fixture.Organization.Id,
            new IdeaTypeWriteRequestModel { Name = "  continuous improvement  " },
            CancellationToken.None);

        Assert.False(result.Succeeded);
        Assert.Equal(WorkflowFailureReason.ValidationError, result.FailureReason);
        Assert.Contains("Idea Type name must be unique within the organization.", result.Errors);
        Assert.Equal(0, fixture.DataAccess.SaveChangesCallCount);
    }

    [Fact]
    public async Task ReorderIdeaTypes_WhenRequestContainsDuplicateAndOmitsOption_ReturnsValidationWithoutMutation()
    {
        var fixture = new WorkflowFixture();
        var second = new IdeaType
        {
            Id = Guid.NewGuid(),
            OrganizationId = fixture.Organization.Id,
            Name = "Process Revision",
            SortOrder = 1
        };
        fixture.DataAccess.SeedIdeaType(second);

        var result = await fixture.Service.ReorderIdeaTypesAsync(
            fixture.OrgAdminActor,
            fixture.Organization.Id,
            new ReorderIdeaTypesRequestModel
            {
                OrderedIdeaTypeIds = new[] { fixture.IdeaType.Id, fixture.IdeaType.Id }
            },
            CancellationToken.None);

        Assert.False(result.Succeeded);
        Assert.Equal(WorkflowFailureReason.ValidationError, result.FailureReason);
        Assert.Equal(0, fixture.IdeaType.SortOrder);
        Assert.Equal(1, second.SortOrder);
        Assert.Equal(0, fixture.DataAccess.SaveChangesCallCount);
    }

    [Fact]
    public async Task SoftDeleteIdeaType_WhenItIsLastActiveOption_ReturnsValidationError()
    {
        var fixture = new WorkflowFixture();

        var result = await fixture.Service.SoftDeleteIdeaTypeAsync(
            fixture.OrgAdminActor,
            fixture.IdeaType.Id,
            CancellationToken.None);

        Assert.False(result.Succeeded);
        Assert.Equal(WorkflowFailureReason.ValidationError, result.FailureReason);
        Assert.False(fixture.IdeaType.IsDeleted);
        Assert.Equal(0, fixture.DataAccess.SaveChangesCallCount);
    }

    [Fact]
    public async Task ListBusinessImpacts_WhenOrgAdminTargetsForeignOrganization_ReturnsForbidden()
    {
        var fixture = new WorkflowFixture();

        var result = await fixture.Service.ListBusinessImpactsAsync(
            fixture.OrgAdminActor,
            fixture.ForeignOrganization.Id,
            CancellationToken.None);

        Assert.False(result.Succeeded);
        Assert.Equal(WorkflowFailureReason.Forbidden, result.FailureReason);
    }

    [Fact]
    public async Task CreateBusinessImpact_WhenColorIsNotRgbHex_ReturnsValidationError()
    {
        var fixture = new WorkflowFixture();

        var result = await fixture.Service.CreateBusinessImpactAsync(
            fixture.OrgAdminActor,
            fixture.Organization.Id,
            new BusinessImpactWriteRequestModel { Name = "Major", Color = "orange" },
            CancellationToken.None);

        Assert.False(result.Succeeded);
        Assert.Equal(WorkflowFailureReason.ValidationError, result.FailureReason);
        Assert.Contains("Business Impact color must be a valid #RRGGBB value.", result.Errors);
        Assert.Equal(0, fixture.DataAccess.SaveChangesCallCount);
    }

    [Fact]
    public async Task UpdateBusinessImpact_WhenTrimmedNameDuplicatesActiveName_ReturnsValidationError()
    {
        var fixture = new WorkflowFixture();
        var duplicate = new BusinessImpact
        {
            Id = Guid.NewGuid(),
            OrganizationId = fixture.Organization.Id,
            Name = "High",
            Color = "#DC2626",
            SortOrder = 2
        };
        fixture.DataAccess.SeedBusinessImpact(duplicate);

        var result = await fixture.Service.UpdateBusinessImpactAsync(
            fixture.OrgAdminActor,
            fixture.BusinessImpact.Id,
            new BusinessImpactWriteRequestModel { Name = "  high  ", Color = "#123456" },
            CancellationToken.None);

        Assert.False(result.Succeeded);
        Assert.Equal(WorkflowFailureReason.ValidationError, result.FailureReason);
        Assert.Contains("Business Impact name must be unique within the organization.", result.Errors);
        Assert.Equal("Medium", fixture.BusinessImpact.Name);
        Assert.Equal(0, fixture.DataAccess.SaveChangesCallCount);
    }

    [Fact]
    public async Task ReorderBusinessImpacts_WhenRequestContainsForeignId_ReturnsValidationWithoutMutation()
    {
        var fixture = new WorkflowFixture();
        var second = new BusinessImpact
        {
            Id = Guid.NewGuid(),
            OrganizationId = fixture.Organization.Id,
            Name = "High",
            Color = "#DC2626",
            SortOrder = 2
        };
        fixture.DataAccess.SeedBusinessImpact(second);

        var result = await fixture.Service.ReorderBusinessImpactsAsync(
            fixture.OrgAdminActor,
            fixture.Organization.Id,
            new ReorderBusinessImpactsRequestModel
            {
                OrderedBusinessImpactIds = new[] { fixture.BusinessImpact.Id, Guid.NewGuid() }
            },
            CancellationToken.None);

        Assert.False(result.Succeeded);
        Assert.Equal(WorkflowFailureReason.ValidationError, result.FailureReason);
        Assert.Equal(1, fixture.BusinessImpact.SortOrder);
        Assert.Equal(2, second.SortOrder);
        Assert.Equal(0, fixture.DataAccess.SaveChangesCallCount);
    }

    [Fact]
    public async Task SoftDeleteBusinessImpact_WhenItIsLastActiveOption_ReturnsValidationError()
    {
        var fixture = new WorkflowFixture();

        var result = await fixture.Service.SoftDeleteBusinessImpactAsync(
            fixture.OrgAdminActor,
            fixture.BusinessImpact.Id,
            CancellationToken.None);

        Assert.False(result.Succeeded);
        Assert.Equal(WorkflowFailureReason.ValidationError, result.FailureReason);
        Assert.False(fixture.BusinessImpact.IsDeleted);
        Assert.Equal(0, fixture.DataAccess.SaveChangesCallCount);
    }

    [Fact]
    public async Task CreateBoard_WhenSwimlaneCountBelowTwo_ReturnsValidationError()
    {
        var fixture = new WorkflowFixture();

        var result = await fixture.Service.CreateBoardAsync(
            fixture.OrgAdminActor,
            fixture.Organization.Id,
            new CreateBoardRequestModel
            {
                Name = "Single Lane",
                StatusIds = new[] { fixture.StatusOne.Id }
            },
            CancellationToken.None);

        Assert.False(result.Succeeded);
        Assert.Equal(WorkflowFailureReason.ValidationError, result.FailureReason);
        Assert.Contains("A board must have at least two swimlanes.", result.Errors);
    }

    [Fact]
    public async Task CreateBoard_WhenStatusOutsideOrganizationSubset_ReturnsValidationError()
    {
        var fixture = new WorkflowFixture();
        var foreignStatus = new Status
        {
            Id = Guid.NewGuid(),
            OrganizationId = Guid.NewGuid(),
            Name = "Foreign",
            IsDeleted = false
        };
        fixture.DataAccess.SeedStatus(foreignStatus);

        var result = await fixture.Service.CreateBoardAsync(
            fixture.OrgAdminActor,
            fixture.Organization.Id,
            new CreateBoardRequestModel
            {
                Name = "Subset Board",
                StatusIds = new[] { fixture.StatusOne.Id, foreignStatus.Id }
            },
            CancellationToken.None);

        Assert.False(result.Succeeded);
        Assert.Equal(WorkflowFailureReason.ValidationError, result.FailureReason);
        Assert.Contains("Board statuses must exist in the organization and cannot be deleted.", result.Errors);
    }

    [Fact]
    public async Task ReorderSwimlanes_WhenValid_PersistsOrderImmediatelyAndAudits()
    {
        var fixture = new WorkflowFixture();
        var board = fixture.CreateBoardWithTwoSwimlanes();

        var result = await fixture.Service.ReorderSwimlanesAsync(
            fixture.OrgAdminActor,
            board.Id,
            new ReorderSwimlanesRequestModel
            {
                OrderedStatusIds = new[] { fixture.StatusTwo.Id, fixture.StatusOne.Id }
            },
            CancellationToken.None);

        Assert.True(result.Succeeded);
        Assert.Equal(1, fixture.DataAccess.SaveChangesCallCount);
        Assert.Single(fixture.AuditWriter.BoardReorderedEvents);

        var swimlanes = await fixture.DataAccess.ListBoardSwimlanesAsync(board.Id, CancellationToken.None);
        Assert.Equal(fixture.StatusTwo.Id, swimlanes.Single(item => item.Order == 0).StatusId);
        Assert.Equal(fixture.StatusOne.Id, swimlanes.Single(item => item.Order == 1).StatusId);
    }

    [Fact]
    public async Task SoftDeleteStatus_WhenAuthorized_MarksDeletedAndWritesAudit()
    {
        var fixture = new WorkflowFixture();

        var result = await fixture.Service.SoftDeleteStatusAsync(
            fixture.OrgAdminActor,
            fixture.StatusOne.Id,
            CancellationToken.None);

        Assert.True(result.Succeeded);
        Assert.True(fixture.StatusOne.IsDeleted);
        Assert.Equal(1, fixture.DataAccess.SaveChangesCallCount);
        Assert.Single(fixture.AuditWriter.StatusDeletedEvents);
    }

    [Fact]
    public async Task GetBoardDetail_WhenBoardReferencesSoftDeletedStatus_ShowsDeletedLabel()
    {
        var fixture = new WorkflowFixture();
        var board = fixture.CreateBoardWithTwoSwimlanes();
        fixture.StatusOne.IsDeleted = true;

        var result = await fixture.Service.GetBoardDetailAsync(
            fixture.OrgAdminActor,
            board.Id,
            CancellationToken.None);

        Assert.True(result.Succeeded);
        var deletedSwimlane = Assert.Single(result.Response!.Swimlanes.Where(item => item.StatusId == fixture.StatusOne.Id));
        Assert.True(deletedSwimlane.IsDeletedStatus);
        Assert.Equal("New / Pending (Deleted)", deletedSwimlane.StatusName);
    }

    [Fact]
    public async Task ListBoards_WhenBoardReferencesSoftDeletedStatus_ShowsDeletedLabel()
    {
        var fixture = new WorkflowFixture();
        fixture.CreateBoardWithTwoSwimlanes();
        fixture.StatusOne.IsDeleted = true;

        var result = await fixture.Service.ListBoardsAsync(
            fixture.OrgAdminActor,
            fixture.Organization.Id,
            CancellationToken.None);

        Assert.True(result.Succeeded);
        var board = Assert.Single(result.Response!);
        var deletedSwimlane = Assert.Single(board.Swimlanes.Where(item => item.StatusId == fixture.StatusOne.Id));
        Assert.True(deletedSwimlane.IsDeletedStatus);
        Assert.Equal("New / Pending (Deleted)", deletedSwimlane.StatusName);
    }

    [Fact]
    public async Task UpdateStatus_WhenRoleCannotManage_ReturnsForbidden()
    {
        var fixture = new WorkflowFixture();

        var result = await fixture.Service.UpdateStatusAsync(
            fixture.ReadOnlyActor,
            fixture.StatusOne.Id,
            new UpdateStatusRequestModel { Name = "Updated" },
            CancellationToken.None);

        Assert.False(result.Succeeded);
        Assert.Equal(WorkflowFailureReason.Forbidden, result.FailureReason);
        Assert.Equal(0, fixture.DataAccess.SaveChangesCallCount);
    }

    [Fact]
    public async Task CreateIdea_WhenStatusOmitted_UsesFirstBoardSwimlane()
    {
        var fixture = new WorkflowFixture();
        var board = fixture.CreateBoardWithTwoSwimlanes();

        var result = await fixture.Service.CreateIdeaAsync(
            fixture.OrgAdminActor,
            board.Id,
            new IdeaWriteRequestModel
            {
                Title = "Improve onboarding",
                Description = "Add more guided setup steps",
                Priority = "High",
                IdeaTypeId = fixture.IdeaType.Id,
                BusinessImpactId = fixture.BusinessImpact.Id
            },
            CancellationToken.None);

        Assert.True(result.Succeeded);
        Assert.NotNull(result.Response);
        Assert.Equal(fixture.StatusOne.Id, result.Response!.StatusId);
        Assert.Equal("High", result.Response.Priority);
        Assert.Equal(2, fixture.DataAccess.SaveChangesCallCount);
        Assert.Single(fixture.AuditWriter.IdeaCreatedEvents);
        Assert.Equal(result.Response.IdeaId, fixture.AuditWriter.IdeaCreatedEvents[0].Id);
    }

    [Fact]
    public async Task UpdateIdea_WhenValid_WritesAuditEvent()
    {
        var fixture = new WorkflowFixture();
        var idea = fixture.CreateIdeaWithAuthor(fixture.OrgAdminUser);

        var result = await fixture.Service.UpdateIdeaAsync(
            fixture.OrgAdminActor,
            idea.Id,
            new IdeaWriteRequestModel
            {
                Title = "Updated title",
                Description = "Updated description",
                Priority = "Critical",
                IdeaTypeId = fixture.IdeaType.Id,
                BusinessImpactId = fixture.BusinessImpact.Id,
                StatusId = fixture.StatusTwo.Id
            },
            CancellationToken.None);

        Assert.True(result.Succeeded);
        Assert.Single(fixture.AuditWriter.IdeaUpdatedEvents);
        Assert.Equal(idea.Id, fixture.AuditWriter.IdeaUpdatedEvents[0].Id);
        Assert.Equal(fixture.StatusTwo.Id, fixture.AuditWriter.IdeaUpdatedEvents[0].StatusId);
    }

    [Fact]
    public async Task UpdateIdea_WhenNonAuthorLeavesDescriptionAndInactiveAssignmentUnchanged_UpdatesGeneralFields()
    {
        var fixture = new WorkflowFixture();
        var idea = fixture.CreateIdeaWithAuthor(fixture.OrgAdminUser);
        fixture.DataAccess.AddIdeaAssignee(new IdeaAssignee { IdeaId = idea.Id, UserId = fixture.ReadOnlyUser.Id });
        fixture.ReadOnlyUser.Status = UserLifecycleStatus.Inactive;

        var result = await fixture.Service.UpdateIdeaAsync(
            fixture.StandardUserActor,
            idea.Id,
            new IdeaWriteRequestModel
            {
                Title = "Updated by collaborator",
                Description = idea.Description,
                Priority = "High",
                IdeaTypeId = fixture.IdeaType.Id,
                BusinessImpactId = fixture.BusinessImpact.Id,
                StatusId = fixture.StatusOne.Id,
                AssigneeUserIds = new[] { fixture.ReadOnlyUser.Id }
            },
            CancellationToken.None);

        Assert.True(result.Succeeded);
        Assert.Equal("Updated by collaborator", idea.Title);
        Assert.Equal(new[] { fixture.ReadOnlyUser.Id }, result.Response!.Assignees.Select(item => item.UserId).ToArray());
    }

    [Fact]
    public async Task UpdateIdea_WhenReadOnlyUserLeavesRestrictedFieldsUnchanged_ReturnsForbidden()
    {
        var fixture = new WorkflowFixture();
        var idea = fixture.CreateIdeaWithAuthor(fixture.OrgAdminUser);

        var result = await fixture.Service.UpdateIdeaAsync(
            fixture.ReadOnlyActor,
            idea.Id,
            new IdeaWriteRequestModel
            {
                Title = "Read only edit",
                Description = idea.Description,
                Priority = "High",
                IdeaTypeId = fixture.IdeaType.Id,
                BusinessImpactId = fixture.BusinessImpact.Id,
                StatusId = fixture.StatusOne.Id
            },
            CancellationToken.None);

        Assert.False(result.Succeeded);
        Assert.Equal(WorkflowFailureReason.Forbidden, result.FailureReason);
        Assert.Equal("Idea", idea.Title);
    }

    [Fact]
    public async Task UpdateIdea_WhenNonAuthorChangesDescription_ReturnsForbidden()
    {
        var fixture = new WorkflowFixture();
        var idea = fixture.CreateIdeaWithAuthor(fixture.OrgAdminUser);

        var result = await fixture.Service.UpdateIdeaAsync(
            fixture.StandardUserActor,
            idea.Id,
            new IdeaWriteRequestModel
            {
                Title = idea.Title,
                Description = "Changed by non-author",
                Priority = idea.Priority.ToString(),
                IdeaTypeId = fixture.IdeaType.Id,
                BusinessImpactId = fixture.BusinessImpact.Id,
                StatusId = fixture.StatusOne.Id
            },
            CancellationToken.None);

        Assert.False(result.Succeeded);
        Assert.Equal(WorkflowFailureReason.Forbidden, result.FailureReason);
        Assert.Equal("Description", idea.Description);
    }

    [Fact]
    public async Task UpdateIdea_WhenNonAuthorChangesAssignments_ReturnsForbidden()
    {
        var fixture = new WorkflowFixture();
        var idea = fixture.CreateIdeaWithAuthor(fixture.OrgAdminUser);

        var result = await fixture.Service.UpdateIdeaAsync(
            fixture.StandardUserActor,
            idea.Id,
            new IdeaWriteRequestModel
            {
                Title = idea.Title,
                Description = idea.Description,
                Priority = idea.Priority.ToString(),
                IdeaTypeId = fixture.IdeaType.Id,
                BusinessImpactId = fixture.BusinessImpact.Id,
                StatusId = fixture.StatusOne.Id,
                AssigneeUserIds = new[] { fixture.ReadOnlyUser.Id }
            },
            CancellationToken.None);

        Assert.False(result.Succeeded);
        Assert.Equal(WorkflowFailureReason.Forbidden, result.FailureReason);
        Assert.Empty(idea.Assignees);
    }

    [Fact]
    public async Task SoftDeleteIdea_WhenAuthorized_PersistsMetadataAndWritesAuditEvent()
    {
        var fixture = new WorkflowFixture();
        var idea = fixture.CreateIdeaWithAuthor(fixture.StandardUser);

        var result = await fixture.Service.SoftDeleteIdeaAsync(
            fixture.OrgAdminActor,
            idea.Id,
            CancellationToken.None);

        Assert.True(result.Succeeded);
        Assert.True(idea.IsDeleted);
        Assert.Equal(fixture.OrgAdminUser.Id, idea.DeletedByUserId);
        Assert.NotNull(idea.DeletedAtUtc);
        var auditEvent = Assert.Single(fixture.AuditWriter.IdeaDeletedEvents);
        Assert.Equal(fixture.OrgAdminUser.Id, auditEvent.ActorUserId);
        Assert.Same(idea, auditEvent.Idea);
    }

    [Fact]
    public async Task SoftDeleteIdea_WhenAlreadyDeleted_ReturnsIdeaNotFoundWithoutSecondAuditEvent()
    {
        var fixture = new WorkflowFixture();
        var idea = fixture.CreateIdeaWithAuthor(fixture.StandardUser);
        idea.IsDeleted = true;

        var result = await fixture.Service.SoftDeleteIdeaAsync(
            fixture.OrgAdminActor,
            idea.Id,
            CancellationToken.None);

        Assert.False(result.Succeeded);
        Assert.Equal(WorkflowFailureReason.IdeaNotFound, result.FailureReason);
        Assert.Empty(fixture.AuditWriter.IdeaDeletedEvents);
    }

    [Fact]
    public async Task GetIdeaDetail_WhenIdeaPendingApproval_ReturnsApprovalMetadata()
    {
        var fixture = new WorkflowFixture();
        var board = fixture.CreateBoardWithTwoSwimlanes();
        var idea = fixture.CreateIdeaWithAuthor(fixture.OrgAdminUser, board);
        idea.ApprovalState = IdeaApprovalState.PendingApproval;
        idea.PendingApprovalTargetStatusId = fixture.StatusTwo.Id;
        idea.PendingApprovalPreviousStatusId = fixture.StatusOne.Id;
        idea.PendingApprovalRequestedAtUtc = DateTime.UtcNow.AddHours(-1);
        idea.PendingApprovalExpiresAtUtc = DateTime.UtcNow.AddHours(23);

        var result = await fixture.Service.GetIdeaDetailAsync(
            fixture.OrgAdminActor,
            idea.Id,
            CancellationToken.None);

        Assert.True(result.Succeeded);
        Assert.Equal("PendingApproval", result.Response!.ApprovalState);
        Assert.Equal(fixture.StatusTwo.Id, result.Response.PendingApprovalTargetStatusId);
        Assert.Equal(fixture.StatusOne.Id, result.Response.PendingApprovalPreviousStatusId);
        Assert.NotNull(result.Response.PendingApprovalExpiresAtUtc);
    }

    [Fact]
    public async Task ListAndDetailIdeas_WhenRelationsExist_ReturnCompleteOrderedCallerProjection()
    {
        var fixture = new WorkflowFixture();
        var board = fixture.CreateBoardWithTwoSwimlanes();
        var idea = fixture.CreateIdeaWithAuthor(fixture.OrgAdminUser, board);
        fixture.DataAccess.AddIdeaAssignee(new IdeaAssignee { IdeaId = idea.Id, UserId = fixture.StandardUser.Id });
        fixture.DataAccess.AddIdeaAssignee(new IdeaAssignee { IdeaId = idea.Id, UserId = fixture.ReadOnlyUser.Id });
        var zuluTag = fixture.SeedTag("zulu", "ZULU");
        var alphaTag = fixture.SeedTag("Alpha", "ALPHA");
        fixture.DataAccess.AddIdeaTag(new IdeaTag { IdeaId = idea.Id, TagId = zuluTag.Id });
        fixture.DataAccess.AddIdeaTag(new IdeaTag { IdeaId = idea.Id, TagId = alphaTag.Id });
        fixture.DataAccess.SeedComment(new Comment
        {
            Id = Guid.NewGuid(),
            IdeaId = idea.Id,
            AuthorUserId = fixture.StandardUser.Id,
            Body = "First",
            CreatedAtUtc = DateTime.UtcNow
        });
        fixture.DataAccess.SeedUpvote(new Upvote
        {
            IdeaId = idea.Id,
            UserId = fixture.StandardUser.Id,
            CreatedAtUtc = DateTime.UtcNow
        });

        var listResult = await fixture.Service.ListIdeasAsync(
            fixture.StandardUserActor,
            board.Id,
            new IdeaListQueryModel(),
            CancellationToken.None);
        var detailResult = await fixture.Service.GetIdeaDetailAsync(
            fixture.StandardUserActor,
            idea.Id,
            CancellationToken.None);

        Assert.True(listResult.Succeeded);
        Assert.True(detailResult.Succeeded);
        var listItem = Assert.Single(listResult.Response!.Items);
        var detail = detailResult.Response!;
        Assert.Equal(fixture.IdeaType.Name, listItem.IdeaTypeName);
        Assert.Equal(fixture.BusinessImpact.Name, listItem.BusinessImpactName);
        Assert.Equal(fixture.BusinessImpact.Color, listItem.BusinessImpactColor);
        Assert.Equal(new[] { fixture.ReadOnlyUser.Id, fixture.StandardUser.Id }, listItem.Assignees.Select(item => item.UserId).ToArray());
        Assert.Equal(new[] { "Alpha", "zulu" }, listItem.TagNames);
        Assert.Equal(1, listItem.CommentCount);
        Assert.True(listItem.HasUpvoted);
        Assert.Equal(listItem.Assignees.Select(item => item.UserId), detail.Assignees.Select(item => item.UserId));
        Assert.Equal(listItem.TagNames, detail.TagNames);
        Assert.Equal(1, detail.CommentCount);
        Assert.True(detail.HasUpvoted);
    }

    [Fact]
    public async Task UpdateIdea_WhenTagsAndMentionsChange_ReplacesRelationsAndResolvesMentions()
    {
        var fixture = new WorkflowFixture();
        var idea = fixture.CreateIdeaWithAuthor(fixture.OrgAdminUser);
        var existingTag = fixture.SeedTag("Legacy", "LEGACY");
        fixture.DataAccess.AddIdeaTag(new IdeaTag { IdeaId = idea.Id, TagId = existingTag.Id });
        fixture.DataAccess.AddMention(new Mention
        {
            Id = Guid.NewGuid(),
            OrganizationId = fixture.Organization.Id,
            IdeaId = idea.Id,
            MentionedUserId = fixture.ReadOnlyUser.Id,
            SourceText = fixture.ReadOnlyUser.Email
        });

        var result = await fixture.Service.UpdateIdeaAsync(
            fixture.OrgAdminActor,
            idea.Id,
            new IdeaWriteRequestModel
            {
                Title = "Updated title",
                Description = "Updated description",
                Priority = "Critical",
                IdeaTypeId = fixture.IdeaType.Id,
                BusinessImpactId = fixture.BusinessImpact.Id,
                StatusId = fixture.StatusTwo.Id,
                TagNames = new[] { "Fresh", "fresh" },
                MentionEmails = new[] { fixture.StandardUser.Email }
            },
            CancellationToken.None);

        Assert.True(result.Succeeded);

        var persisted = await fixture.DataAccess.FindIdeaByIdAsync(idea.Id, CancellationToken.None);
        Assert.NotNull(persisted);
        Assert.Equal(new[] { "Fresh" }, persisted!.IdeaTags.Select(item => item.Tag.Name).ToArray());
        Assert.Equal(new[] { fixture.StandardUser.Email }, persisted.Mentions.Select(item => item.MentionedUser.Email).ToArray());
    }

    [Fact]
    public async Task CreateIdea_WhenTagNamesContainMixedCaseDuplicates_CreatesSingleNormalizedTag()
    {
        var fixture = new WorkflowFixture();
        var board = fixture.CreateBoardWithTwoSwimlanes();

        var result = await fixture.Service.CreateIdeaAsync(
            fixture.OrgAdminActor,
            board.Id,
            new IdeaWriteRequestModel
            {
                Title = "Tag normalization",
                Description = "Verify duplicate tag normalization",
                Priority = "Medium",
                IdeaTypeId = fixture.IdeaType.Id,
                BusinessImpactId = fixture.BusinessImpact.Id,
                TagNames = new[] { "Ops", "ops", "  OPS  " }
            },
            CancellationToken.None);

        Assert.True(result.Succeeded);
        Assert.Equal(new[] { "Ops" }, result.Response!.TagNames);

        var persisted = await fixture.DataAccess.FindIdeaByIdAsync(result.Response.IdeaId, CancellationToken.None);
        Assert.NotNull(persisted);
        Assert.Single(persisted!.IdeaTags);
    }

    [Fact]
    public async Task CreateIdea_WhenExistingNormalizedTagPresent_ReusesExistingTagWithoutDuplicateCreation()
    {
        var fixture = new WorkflowFixture();
        var board = fixture.CreateBoardWithTwoSwimlanes();
        fixture.SeedTag("Operations", "OPERATIONS");

        var result = await fixture.Service.CreateIdeaAsync(
            fixture.OrgAdminActor,
            board.Id,
            new IdeaWriteRequestModel
            {
                Title = "Tag reuse",
                Description = "Verify existing tag reuse",
                Priority = "Medium",
                IdeaTypeId = fixture.IdeaType.Id,
                BusinessImpactId = fixture.BusinessImpact.Id,
                TagNames = new[] { "operations" }
            },
            CancellationToken.None);

        Assert.True(result.Succeeded);
        Assert.Equal(1, fixture.DataAccess.TagCount);
        Assert.Equal(1, fixture.DataAccess.IdeaTagCount);
        Assert.Equal(new[] { "Operations" }, result.Response!.TagNames);
    }

    [Fact]
    public async Task CreateIdea_WhenMoreThanTenDistinctNormalizedTags_ReturnsValidationError()
    {
        var fixture = new WorkflowFixture();
        var board = fixture.CreateBoardWithTwoSwimlanes();

        var result = await fixture.Service.CreateIdeaAsync(
            fixture.OrgAdminActor,
            board.Id,
            new IdeaWriteRequestModel
            {
                Title = "Too many tags",
                Description = "Reject the eleventh distinct normalized tag",
                Priority = "Medium",
                IdeaTypeId = fixture.IdeaType.Id,
                BusinessImpactId = fixture.BusinessImpact.Id,
                TagNames = Enumerable.Range(1, 11).Select(item => $"Tag {item}").Append("tag 1").ToArray()
            },
            CancellationToken.None);

        Assert.False(result.Succeeded);
        Assert.Equal(WorkflowFailureReason.ValidationError, result.FailureReason);
        Assert.Contains("An idea can have no more than 10 tags.", result.Errors);
    }

    [Fact]
    public async Task CreateIdea_WhenMentionEmailsIncludeUnknownOrForeignUsers_ReturnsValidationError()
    {
        var fixture = new WorkflowFixture();
        var board = fixture.CreateBoardWithTwoSwimlanes();

        var result = await fixture.Service.CreateIdeaAsync(
            fixture.OrgAdminActor,
            board.Id,
            new IdeaWriteRequestModel
            {
                Title = "Mention filtering",
                Description = "Verify mention scoping",
                Priority = "Medium",
                MentionEmails = new[]
                {
                    fixture.StandardUser.Email,
                    fixture.ForeignUser.Email,
                    "unknown@test.local"
                }
            },
            CancellationToken.None);

        Assert.False(result.Succeeded);
        Assert.Equal(WorkflowFailureReason.ValidationError, result.FailureReason);
        Assert.Contains(result.Errors, error => error.Contains("Mention email", StringComparison.OrdinalIgnoreCase));
        Assert.Equal(0, fixture.DataAccess.SaveChangesCallCount);
        Assert.Empty(fixture.AuditWriter.IdeaCreatedEvents);
    }

    [Fact]
    public async Task CreateIdea_WhenMentionEmailsAreAllSameOrganization_PersistsMentions()
    {
        var fixture = new WorkflowFixture();
        var board = fixture.CreateBoardWithTwoSwimlanes();

        var result = await fixture.Service.CreateIdeaAsync(
            fixture.OrgAdminActor,
            board.Id,
            new IdeaWriteRequestModel
            {
                Title = "Mention filtering",
                Description = "Verify mention scoping",
                Priority = "Medium",
                IdeaTypeId = fixture.IdeaType.Id,
                BusinessImpactId = fixture.BusinessImpact.Id,
                MentionEmails = new[] { "user@test.local" }
            },
            CancellationToken.None);

        Assert.True(result.Succeeded, $"Failure: {result.FailureReason}; Errors: {string.Join(" | ", result.Errors)}");
        Assert.Equal(new[] { "user@test.local" }, result.Response!.Mentions);
    }

    [Fact]
    public async Task CreateIdea_WhenActorBelongsToDifferentOrganization_ReturnsForbidden()
    {
        var fixture = new WorkflowFixture();
        var board = fixture.CreateBoardWithTwoSwimlanes();

        var result = await fixture.Service.CreateIdeaAsync(
            fixture.ForeignUserActor,
            board.Id,
            new IdeaWriteRequestModel
            {
                Title = "Cross org attempt",
                Description = "Should be blocked",
                Priority = "Low"
            },
            CancellationToken.None);

        Assert.False(result.Succeeded);
        Assert.Equal(WorkflowFailureReason.Forbidden, result.FailureReason);
        Assert.Empty(fixture.AuditWriter.IdeaCreatedEvents);
    }

    [Fact]
    public async Task ToggleIdeaUpvote_WhenCalledTwice_TogglesOnThenOff()
    {
        var fixture = new WorkflowFixture();
        var board = fixture.CreateBoardWithTwoSwimlanes();

        var createResult = await fixture.Service.CreateIdeaAsync(
            fixture.OrgAdminActor,
            board.Id,
            new IdeaWriteRequestModel
            {
                Title = "Reduce handoff delays",
                Description = "Automate assignment notifications",
                Priority = "Medium",
                IdeaTypeId = fixture.IdeaType.Id,
                BusinessImpactId = fixture.BusinessImpact.Id
            },
            CancellationToken.None);

        Assert.True(createResult.Succeeded);

        var firstToggle = await fixture.Service.ToggleIdeaUpvoteAsync(
            fixture.OrgAdminActor,
            createResult.Response!.IdeaId,
            CancellationToken.None);

        Assert.True(firstToggle.Succeeded);
        Assert.True(firstToggle.Response!.HasUpvoted);
        Assert.Equal(1, firstToggle.Response.UpvoteCount);

        var secondToggle = await fixture.Service.ToggleIdeaUpvoteAsync(
            fixture.OrgAdminActor,
            createResult.Response.IdeaId,
            CancellationToken.None);

        Assert.True(secondToggle.Succeeded);
        Assert.False(secondToggle.Response!.HasUpvoted);
        Assert.Equal(0, secondToggle.Response.UpvoteCount);
        Assert.Equal(4, fixture.DataAccess.SaveChangesCallCount);
        Assert.Equal(2, fixture.AuditWriter.IdeaUpvoteToggledEvents.Count);
        Assert.Equal(createResult.Response.IdeaId, fixture.AuditWriter.IdeaUpvoteToggledEvents[0].Idea.Id);
        Assert.True(fixture.AuditWriter.IdeaUpvoteToggledEvents[0].HasUpvoted);
        Assert.Equal(1, fixture.AuditWriter.IdeaUpvoteToggledEvents[0].UpvoteCount);
        Assert.False(fixture.AuditWriter.IdeaUpvoteToggledEvents[1].HasUpvoted);
        Assert.Equal(0, fixture.AuditWriter.IdeaUpvoteToggledEvents[1].UpvoteCount);
    }

    [Fact]
    public async Task MoveIdeaStatus_WhenSubmitForApproval_PersistsPendingRequestWithoutChangingStatus()
    {
        var fixture = new WorkflowFixture();
        var board = fixture.CreateBoardWithTwoSwimlanes();
        var idea = fixture.CreateIdeaWithAuthor(fixture.OrgAdminUser, board);

        var result = await fixture.Service.MoveIdeaStatusAsync(
            fixture.OrgAdminActor,
            idea.Id,
            new MoveIdeaStatusRequestModel
            {
                StatusId = fixture.StatusTwo.Id,
                SubmitForApproval = true
            },
            CancellationToken.None);

        Assert.True(result.Succeeded);
        Assert.Equal(fixture.StatusOne.Id, idea.StatusId);
        Assert.Equal(IdeaApprovalState.PendingApproval, idea.ApprovalState);
        Assert.Equal(fixture.StatusTwo.Id, idea.PendingApprovalTargetStatusId);
        Assert.NotNull(idea.PendingApprovalExpiresAtUtc);
        Assert.Equal(idea.PendingApprovalRequestedAtUtc, idea.PendingApprovalExpiresAtUtc!.Value.AddHours(-24));
    }

    [Fact]
    public async Task MoveIdeaStatus_WhenApprovePendingApproval_TransitionsToTargetStatus()
    {
        var fixture = new WorkflowFixture();
        var board = fixture.CreateBoardWithTwoSwimlanes();
        var idea = fixture.CreateIdeaWithAuthor(fixture.OrgAdminUser, board);
        idea.ApprovalState = IdeaApprovalState.PendingApproval;
        idea.PendingApprovalTargetStatusId = fixture.StatusTwo.Id;
        idea.PendingApprovalPreviousStatusId = fixture.StatusOne.Id;
        idea.PendingApprovalRequestedAtUtc = DateTime.UtcNow.AddHours(-1);
        idea.PendingApprovalExpiresAtUtc = DateTime.UtcNow.AddHours(23);

        var result = await fixture.Service.MoveIdeaStatusAsync(
            fixture.OrgAdminActor,
            idea.Id,
            new MoveIdeaStatusRequestModel
            {
                StatusId = fixture.StatusTwo.Id,
                Approve = true
            },
            CancellationToken.None);

        Assert.True(result.Succeeded);
        Assert.Equal(fixture.StatusTwo.Id, idea.StatusId);
        Assert.Equal(IdeaApprovalState.None, idea.ApprovalState);
        Assert.Null(idea.PendingApprovalTargetStatusId);
    }

    [Fact]
    public async Task MoveIdeaStatus_WhenPendingApprovalExpires_AutoRestoresPreviousStatus()
    {
        var fixture = new WorkflowFixture();
        var board = fixture.CreateBoardWithTwoSwimlanes();
        var idea = fixture.CreateIdeaWithAuthor(fixture.OrgAdminUser, board);
        idea.ApprovalState = IdeaApprovalState.PendingApproval;
        idea.PendingApprovalTargetStatusId = fixture.StatusTwo.Id;
        idea.PendingApprovalPreviousStatusId = fixture.StatusOne.Id;
        idea.PendingApprovalRequestedAtUtc = DateTime.UtcNow.AddHours(-25);
        idea.PendingApprovalExpiresAtUtc = DateTime.UtcNow.AddHours(-1);

        var result = await fixture.Service.MoveIdeaStatusAsync(
            fixture.OrgAdminActor,
            idea.Id,
            new MoveIdeaStatusRequestModel { StatusId = fixture.StatusTwo.Id },
            CancellationToken.None);

        Assert.True(result.Succeeded);
        Assert.Equal(fixture.StatusOne.Id, idea.StatusId);
        Assert.Equal(IdeaApprovalState.None, idea.ApprovalState);
    }

    [Fact]
    public async Task MoveIdeaStatus_WhenRoleUserAndBoardDisallows_ReturnsForbidden()
    {
        var fixture = new WorkflowFixture();
        var board = fixture.CreateBoardWithTwoSwimlanes(allowUserStatusUpdate: false);
        var idea = fixture.CreateIdeaWithAuthor(fixture.OrgAdminUser, board);
        var savesBefore = fixture.DataAccess.SaveChangesCallCount;

        var result = await fixture.Service.MoveIdeaStatusAsync(
            fixture.StandardUserActor,
            idea.Id,
            new MoveIdeaStatusRequestModel { StatusId = fixture.StatusTwo.Id },
            CancellationToken.None);

        Assert.False(result.Succeeded);
        Assert.Equal(WorkflowFailureReason.Forbidden, result.FailureReason);
        Assert.Equal(fixture.StatusOne.Id, idea.StatusId);
        Assert.Equal(savesBefore, fixture.DataAccess.SaveChangesCallCount);
        Assert.Empty(fixture.AuditWriter.IdeaStatusMovedEvents);
    }

    [Fact]
    public async Task MoveIdeaStatus_WhenRoleUserAndBoardAllows_Succeeds()
    {
        var fixture = new WorkflowFixture();
        var board = fixture.CreateBoardWithTwoSwimlanes(allowUserStatusUpdate: true);
        var idea = fixture.CreateIdeaWithAuthor(fixture.OrgAdminUser, board);
        var savesBefore = fixture.DataAccess.SaveChangesCallCount;

        var result = await fixture.Service.MoveIdeaStatusAsync(
            fixture.StandardUserActor,
            idea.Id,
            new MoveIdeaStatusRequestModel { StatusId = fixture.StatusTwo.Id },
            CancellationToken.None);

        Assert.True(result.Succeeded);
        Assert.Equal(fixture.StatusTwo.Id, idea.StatusId);
        Assert.Equal(savesBefore + 1, fixture.DataAccess.SaveChangesCallCount);
        Assert.Single(fixture.AuditWriter.IdeaStatusMovedEvents);
        Assert.Equal(idea.Id, fixture.AuditWriter.IdeaStatusMovedEvents[0].Idea.Id);
        Assert.Equal(fixture.StatusOne.Id, fixture.AuditWriter.IdeaStatusMovedEvents[0].PreviousStatusId);
    }

    [Fact]
    public async Task MoveIdeaStatus_WhenRoleOrgAdminAndBoardDisallows_Succeeds()
    {
        var fixture = new WorkflowFixture();
        var board = fixture.CreateBoardWithTwoSwimlanes(allowUserStatusUpdate: false);
        var idea = fixture.CreateIdeaWithAuthor(fixture.StandardUser, board);

        var result = await fixture.Service.MoveIdeaStatusAsync(
            fixture.OrgAdminActor,
            idea.Id,
            new MoveIdeaStatusRequestModel { StatusId = fixture.StatusTwo.Id },
            CancellationToken.None);

        Assert.True(result.Succeeded);
        Assert.Equal(fixture.StatusTwo.Id, idea.StatusId);
    }

    [Fact]
    public async Task MoveIdeaStatus_WhenRoleSiteAdminAndBoardDisallows_Succeeds()
    {
        var fixture = new WorkflowFixture();
        var board = fixture.CreateBoardWithTwoSwimlanes(allowUserStatusUpdate: false);
        var idea = fixture.CreateIdeaWithAuthor(fixture.StandardUser, board);

        var result = await fixture.Service.MoveIdeaStatusAsync(
            fixture.SiteAdminActor,
            idea.Id,
            new MoveIdeaStatusRequestModel { StatusId = fixture.StatusTwo.Id },
            CancellationToken.None);

        Assert.True(result.Succeeded);
        Assert.Equal(fixture.StatusTwo.Id, idea.StatusId);
    }

    [Fact]
    public async Task CreateIdea_WhenMentionEmailsAreResolved_WritesMentionNotificationEvent()
    {
        var fixture = new WorkflowFixture();
        var board = fixture.CreateBoardWithTwoSwimlanes();

        var result = await fixture.Service.CreateIdeaAsync(
            fixture.OrgAdminActor,
            board.Id,
            new IdeaWriteRequestModel
            {
                Title = "Mentioned idea",
                Description = "Notify the mentioned user",
                Priority = "Medium",
                IdeaTypeId = fixture.IdeaType.Id,
                BusinessImpactId = fixture.BusinessImpact.Id,
                MentionEmails = new[] { fixture.StandardUser.Email }
            },
            CancellationToken.None);

        Assert.True(result.Succeeded);
        var notification = Assert.Single(fixture.NotificationWriter.Events);
        Assert.Equal("IdeaMention", notification.EventType);
        Assert.Equal(fixture.StandardUser.Id, notification.RecipientUserId);
        Assert.Equal($"/ideas/{result.Response!.IdeaId}/edit", notification.IdeaLink);
    }

    [Fact]
    public async Task CreateIdea_WhenMentionEmailMatchesActor_DoesNotWriteSelfNotificationEvent()
    {
        var fixture = new WorkflowFixture();
        var board = fixture.CreateBoardWithTwoSwimlanes();

        var result = await fixture.Service.CreateIdeaAsync(
            fixture.OrgAdminActor,
            board.Id,
            new IdeaWriteRequestModel
            {
                Title = "Self mention",
                Description = "No self notification",
                Priority = "Medium",
                IdeaTypeId = fixture.IdeaType.Id,
                BusinessImpactId = fixture.BusinessImpact.Id,
                MentionEmails = new[] { fixture.OrgAdminUser.Email }
            },
            CancellationToken.None);

        Assert.True(result.Succeeded);
        Assert.Empty(fixture.NotificationWriter.Events);
    }

    [Fact]
    public async Task MoveIdeaStatus_WhenStatusChanges_WritesStatusChangeNotificationEvent()
    {
        var fixture = new WorkflowFixture();
        var board = fixture.CreateBoardWithTwoSwimlanes();
        var idea = fixture.CreateIdeaWithAuthor(fixture.StandardUser, board);

        var result = await fixture.Service.MoveIdeaStatusAsync(
            fixture.OrgAdminActor,
            idea.Id,
            new MoveIdeaStatusRequestModel { StatusId = fixture.StatusTwo.Id },
            CancellationToken.None);

        Assert.True(result.Succeeded);
        var notification = Assert.Single(fixture.NotificationWriter.Events);
        Assert.Equal("IdeaStatusChanged", notification.EventType);
        Assert.Equal(fixture.StandardUser.Id, notification.RecipientUserId);
        Assert.Equal($"/ideas/{idea.Id}/edit", notification.IdeaLink);
    }

    [Fact]
    public async Task MoveIdeaStatus_WhenAuthorAndAssigneeAreDifferent_WritesTwoStatusChangeNotifications()
    {
        var fixture = new WorkflowFixture();
        var board = fixture.CreateBoardWithTwoSwimlanes();
        var idea = fixture.CreateIdeaWithAuthor(fixture.StandardUser, board);
        fixture.DataAccess.AddIdeaAssignee(new IdeaAssignee { IdeaId = idea.Id, UserId = fixture.ReadOnlyUser.Id });

        var result = await fixture.Service.MoveIdeaStatusAsync(
            fixture.OrgAdminActor,
            idea.Id,
            new MoveIdeaStatusRequestModel { StatusId = fixture.StatusTwo.Id },
            CancellationToken.None);

        Assert.True(result.Succeeded);
        var notifications = fixture.NotificationWriter.Events.Where(item => item.EventType == "IdeaStatusChanged").ToArray();
        Assert.Equal(2, notifications.Length);
        Assert.Contains(notifications, item => item.RecipientUserId == fixture.StandardUser.Id);
        Assert.Contains(notifications, item => item.RecipientUserId == fixture.ReadOnlyUser.Id);
        Assert.All(notifications, item => Assert.Equal($"/ideas/{idea.Id}/edit", item.IdeaLink));
    }

    [Fact]
    public async Task MoveIdeaStatus_WhenAuthorIsAlsoAssignee_WritesOneNotification()
    {
        var fixture = new WorkflowFixture();
        var board = fixture.CreateBoardWithTwoSwimlanes();
        var idea = fixture.CreateIdeaWithAuthor(fixture.StandardUser, board);
        fixture.DataAccess.AddIdeaAssignee(new IdeaAssignee { IdeaId = idea.Id, UserId = fixture.StandardUser.Id });

        var result = await fixture.Service.MoveIdeaStatusAsync(
            fixture.OrgAdminActor,
            idea.Id,
            new MoveIdeaStatusRequestModel { StatusId = fixture.StatusTwo.Id },
            CancellationToken.None);

        Assert.True(result.Succeeded);
        var notification = Assert.Single(fixture.NotificationWriter.Events.Where(item => item.EventType == "IdeaStatusChanged"));
        Assert.Equal(fixture.StandardUser.Id, notification.RecipientUserId);
    }

    [Fact]
    public async Task CreateComment_WhenBodyWhitespace_ReturnsValidationError()
    {
        var fixture = new WorkflowFixture();
        var idea = fixture.CreateIdeaWithAuthor(fixture.OrgAdminUser);

        var result = await fixture.Service.CreateCommentAsync(
            fixture.OrgAdminActor,
            idea.Id,
            new CommentWriteRequestModel { Body = "   " },
            CancellationToken.None);

        Assert.False(result.Succeeded);
        Assert.Equal(WorkflowFailureReason.ValidationError, result.FailureReason);
        Assert.Contains("Comment body is required.", result.Errors);
        Assert.Equal(0, fixture.DataAccess.SaveChangesCallCount);
        Assert.Empty(fixture.AuditWriter.CommentCreatedEvents);
    }

    [Fact]
    public async Task CreateComment_WhenBodyContainsMention_ResolvesAndNotifiesMentionedUser()
    {
        var fixture = new WorkflowFixture();
        var idea = fixture.CreateIdeaWithAuthor(fixture.OrgAdminUser);

        var result = await fixture.Service.CreateCommentAsync(
            fixture.OrgAdminActor,
            idea.Id,
            new CommentWriteRequestModel { Body = "Please review @user@test.local" },
            CancellationToken.None);

        Assert.True(result.Succeeded);
        Assert.Contains(fixture.NotificationWriter.Events, item => item.EventType == "CommentMention");
    }

    [Fact]
    public async Task CreateComment_WhenMentionTargetsActor_DoesNotWriteSelfNotificationEvents()
    {
        var fixture = new WorkflowFixture();
        var idea = fixture.CreateIdeaWithAuthor(fixture.OrgAdminUser);

        var result = await fixture.Service.CreateCommentAsync(
            fixture.OrgAdminActor,
            idea.Id,
            new CommentWriteRequestModel { Body = $"Self mention @{fixture.OrgAdminUser.Email}" },
            CancellationToken.None);

        Assert.True(result.Succeeded);
        Assert.Empty(fixture.NotificationWriter.Events);
    }

    [Fact]
    public async Task CreateComment_WhenAuthorAndAssigneeAreDifferent_WritesTwoCommentAddedNotifications()
    {
        var fixture = new WorkflowFixture();
        var board = fixture.CreateBoardWithTwoSwimlanes();
        var idea = fixture.CreateIdeaWithAuthor(fixture.StandardUser, board);
        fixture.DataAccess.AddIdeaAssignee(new IdeaAssignee { IdeaId = idea.Id, UserId = fixture.ReadOnlyUser.Id });

        var result = await fixture.Service.CreateCommentAsync(
            fixture.OrgAdminActor,
            idea.Id,
            new CommentWriteRequestModel { Body = "New collaboration comment" },
            CancellationToken.None);

        Assert.True(result.Succeeded);
        var notifications = fixture.NotificationWriter.Events.Where(item => item.EventType == "CommentAdded").ToArray();
        Assert.Equal(2, notifications.Length);
        Assert.Contains(notifications, item => item.RecipientUserId == fixture.StandardUser.Id);
        Assert.Contains(notifications, item => item.RecipientUserId == fixture.ReadOnlyUser.Id);
        Assert.All(notifications, item => Assert.Equal($"/ideas/{idea.Id}/edit", item.IdeaLink));
    }

    [Fact]
    public async Task CreateComment_WhenValid_WritesAuditEvent()
    {
        var fixture = new WorkflowFixture();
        var idea = fixture.CreateIdeaWithAuthor(fixture.OrgAdminUser);

        var result = await fixture.Service.CreateCommentAsync(
            fixture.OrgAdminActor,
            idea.Id,
            new CommentWriteRequestModel { Body = "First comment" },
            CancellationToken.None);

        Assert.True(result.Succeeded);
        Assert.Single(fixture.AuditWriter.CommentCreatedEvents);
        Assert.Equal(idea.OrganizationId, fixture.AuditWriter.CommentCreatedEvents[0].OrganizationId);
        Assert.Equal(idea.Id, fixture.AuditWriter.CommentCreatedEvents[0].Comment.IdeaId);
    }

    [Fact]
    public async Task CreateComment_WhenActorIsReadOnly_Succeeds()
    {
        var fixture = new WorkflowFixture();
        var idea = fixture.CreateIdeaWithAuthor(fixture.OrgAdminUser);

        var result = await fixture.Service.CreateCommentAsync(
            fixture.ReadOnlyActor,
            idea.Id,
            new CommentWriteRequestModel { Body = "Read-only note" },
            CancellationToken.None);

        Assert.True(result.Succeeded);
        Assert.Single(fixture.AuditWriter.CommentCreatedEvents);
    }

    [Fact]
    public async Task UpdateComment_WhenAuthor_WritesAuditEvent()
    {
        var fixture = new WorkflowFixture();
        var idea = fixture.CreateIdeaWithAuthor(fixture.StandardUser);
        var comment = fixture.CreateComment(idea, fixture.StandardUser, "Original");

        var result = await fixture.Service.UpdateCommentAsync(
            fixture.StandardUserActor,
            comment.Id,
            new CommentWriteRequestModel { Body = "Updated" },
            CancellationToken.None);

        Assert.True(result.Succeeded);
        Assert.Single(fixture.AuditWriter.CommentUpdatedEvents);
        Assert.Equal(idea.OrganizationId, fixture.AuditWriter.CommentUpdatedEvents[0].OrganizationId);
        Assert.Equal(comment.Id, fixture.AuditWriter.CommentUpdatedEvents[0].Comment.Id);
        Assert.Equal("Original", fixture.AuditWriter.CommentUpdatedEvents[0].PreviousBody);
    }

    [Fact]
    public async Task UpdateComment_WhenAuthorIsReadOnly_Succeeds()
    {
        var fixture = new WorkflowFixture();
        var idea = fixture.CreateIdeaWithAuthor(fixture.ReadOnlyUser);
        var comment = fixture.CreateComment(idea, fixture.ReadOnlyUser, "Original");

        var result = await fixture.Service.UpdateCommentAsync(
            fixture.ReadOnlyActor,
            comment.Id,
            new CommentWriteRequestModel { Body = "Updated" },
            CancellationToken.None);

        Assert.True(result.Succeeded);
        Assert.Single(fixture.AuditWriter.CommentUpdatedEvents);
        Assert.Equal(comment.Id, fixture.AuditWriter.CommentUpdatedEvents[0].Comment.Id);
    }

    [Fact]
    public async Task UpdateComment_WhenActorIsNotAuthor_ReturnsForbidden()
    {
        var fixture = new WorkflowFixture();
        var idea = fixture.CreateIdeaWithAuthor(fixture.StandardUser);
        var comment = fixture.CreateComment(idea, fixture.StandardUser, "Original");

        var result = await fixture.Service.UpdateCommentAsync(
            fixture.OrgAdminActor,
            comment.Id,
            new CommentWriteRequestModel { Body = "Updated" },
            CancellationToken.None);

        Assert.False(result.Succeeded);
        Assert.Equal(WorkflowFailureReason.Forbidden, result.FailureReason);
        Assert.Equal("Original", comment.Body);
        Assert.Empty(fixture.AuditWriter.CommentUpdatedEvents);
    }

    [Fact]
    public async Task DeleteComment_WhenNonAuthorUserRoleUser_ReturnsForbidden()
    {
        var fixture = new WorkflowFixture();
        var idea = fixture.CreateIdeaWithAuthor(fixture.OrgAdminUser);
        var comment = fixture.CreateComment(idea, fixture.OrgAdminUser, "Original");

        var result = await fixture.Service.DeleteCommentAsync(
            fixture.StandardUserActor,
            comment.Id,
            CancellationToken.None);

        Assert.False(result.Succeeded);
        Assert.Equal(WorkflowFailureReason.Forbidden, result.FailureReason);
        Assert.Empty(fixture.AuditWriter.CommentDeletedEvents);
    }

    [Fact]
    public async Task DeleteComment_WhenActorIsOrgAdminAndNotAuthor_Succeeds()
    {
        var fixture = new WorkflowFixture();
        var idea = fixture.CreateIdeaWithAuthor(fixture.StandardUser);
        var comment = fixture.CreateComment(idea, fixture.StandardUser, "Original");

        var result = await fixture.Service.DeleteCommentAsync(
            fixture.OrgAdminActor,
            comment.Id,
            CancellationToken.None);

        Assert.True(result.Succeeded);
        Assert.Equal(1, fixture.DataAccess.SaveChangesCallCount);
        Assert.Single(fixture.AuditWriter.CommentDeletedEvents);
        Assert.Equal(idea.OrganizationId, fixture.AuditWriter.CommentDeletedEvents[0].OrganizationId);
        Assert.Equal(comment.Id, fixture.AuditWriter.CommentDeletedEvents[0].Comment.Id);

        var persisted = await fixture.DataAccess.FindCommentByIdAsync(comment.Id, CancellationToken.None);
        Assert.Null(persisted);
    }

    [Fact]
    public async Task DeleteComment_WhenAuthorIsReadOnly_Succeeds()
    {
        var fixture = new WorkflowFixture();
        var idea = fixture.CreateIdeaWithAuthor(fixture.ReadOnlyUser);
        var comment = fixture.CreateComment(idea, fixture.ReadOnlyUser, "Original");

        var result = await fixture.Service.DeleteCommentAsync(
            fixture.ReadOnlyActor,
            comment.Id,
            CancellationToken.None);

        Assert.True(result.Succeeded);
        Assert.Single(fixture.AuditWriter.CommentDeletedEvents);
    }

    [Fact]
    public async Task ToggleIdeaUpvote_WhenActorIsReadOnly_Succeeds()
    {
        var fixture = new WorkflowFixture();
        var idea = fixture.CreateIdeaWithAuthor(fixture.OrgAdminUser);

        var result = await fixture.Service.ToggleIdeaUpvoteAsync(
            fixture.ReadOnlyActor,
            idea.Id,
            CancellationToken.None);

        Assert.True(result.Succeeded);
        Assert.True(result.Response!.HasUpvoted);
        Assert.Equal(1, result.Response.UpvoteCount);
    }

    [Fact]
    public async Task ToggleIdeaUpvote_WhenActorFromForeignOrganization_ReturnsForbidden()
    {
        var fixture = new WorkflowFixture();
        var idea = fixture.CreateIdeaWithAuthor(fixture.OrgAdminUser);

        var result = await fixture.Service.ToggleIdeaUpvoteAsync(
            fixture.ForeignUserActor,
            idea.Id,
            CancellationToken.None);

        Assert.False(result.Succeeded);
        Assert.Equal(WorkflowFailureReason.Forbidden, result.FailureReason);
    }

    [Fact]
    public async Task CreateIdea_WhenActorIsReadOnly_ReturnsForbidden()
    {
        var fixture = new WorkflowFixture();
        var board = fixture.CreateBoardWithTwoSwimlanes();

        var result = await fixture.Service.CreateIdeaAsync(
            fixture.ReadOnlyActor,
            board.Id,
            new IdeaWriteRequestModel
            {
                Title = "Blocked",
                Description = "Read-only cannot create",
                Priority = "Low"
            },
            CancellationToken.None);

        Assert.False(result.Succeeded);
        Assert.Equal(WorkflowFailureReason.Forbidden, result.FailureReason);
    }

    [Fact]
    public async Task ListTagSuggestions_WhenSearchShorterThanTwo_ReturnsValidationError()
    {
        var fixture = new WorkflowFixture();

        var result = await fixture.Service.ListTagSuggestionsAsync(
            fixture.OrgAdminActor,
            fixture.Organization.Id,
            new TagAutocompleteQueryModel { Search = "A" },
            CancellationToken.None);

        Assert.False(result.Succeeded);
        Assert.Equal(WorkflowFailureReason.ValidationError, result.FailureReason);
        Assert.Contains("Tag autocomplete requires at least 2 characters.", result.Errors);
    }

    [Fact]
    public async Task ListTagSuggestions_WhenSearchMatches_ReturnsOrganizationScopedSortedSuggestions()
    {
        var fixture = new WorkflowFixture();
        fixture.SeedTag("Security", "SECURITY");
        fixture.SeedTag("Service", "SERVICE");
        fixture.SeedTag("Billing", "BILLING");

        var result = await fixture.Service.ListTagSuggestionsAsync(
            fixture.OrgAdminActor,
            fixture.Organization.Id,
            new TagAutocompleteQueryModel { Search = "se", Limit = 10 },
            CancellationToken.None);

        Assert.True(result.Succeeded);
        Assert.NotNull(result.Response);
        Assert.Equal(new[] { "Security", "Service" }, result.Response!);
    }

    [Fact]
    public async Task ImportIdeasCsv_WhenValidRows_ReturnsImportedAndSkippedCountsAndAudits()
    {
        var fixture = new WorkflowFixture();
        var board = fixture.CreateBoardWithTwoSwimlanes();
        fixture.CreateIdeaWithAuthor(fixture.OrgAdminUser, board).Title = "Existing Idea";

        var csv = "Title,Description,Priority,IdeaType,BusinessImpact,DueDate,Status,AssignedTo,Tags\n"
            + "Existing Idea,Already there,Low,,,,,,\n"
            + "New Idea,Brand new item,High,,,2026-08-10,In Progress,user@test.local,alpha|beta\n";

        var result = await fixture.Service.ImportIdeasCsvAsync(
            fixture.OrgAdminActor,
            board.Id,
            Encoding.UTF8.GetBytes(csv),
            CancellationToken.None);

        Assert.True(result.Succeeded);
        Assert.NotNull(result.Response);
        Assert.Equal(1, result.Response!.ImportedCount);
        Assert.Equal(1, result.Response.SkippedCount);
        Assert.Empty(result.Response.Errors);
        Assert.Single(fixture.AuditWriter.IdeasImportedEvents);
        Assert.Single(fixture.AuditWriter.IdeaCreatedEvents.Where(i => i.Title == "New Idea"));
    }

    [Fact]
    public async Task ImportIdeasCsv_WhenDuplicateTitleInFile_ReturnsValidationError()
    {
        var fixture = new WorkflowFixture();
        var board = fixture.CreateBoardWithTwoSwimlanes();

        var csv = "Title,Description,Priority,IdeaType,BusinessImpact,DueDate,Status,AssignedTo,Tags\n"
            + "Dup,First row,Low,,,,,,\n"
            + "Dup,Second row,Medium,,,,,,\n";

        var result = await fixture.Service.ImportIdeasCsvAsync(
            fixture.OrgAdminActor,
            board.Id,
            Encoding.UTF8.GetBytes(csv),
            CancellationToken.None);

        Assert.False(result.Succeeded);
        Assert.Equal(WorkflowFailureReason.ValidationError, result.FailureReason);
        Assert.True(result.Errors.ContainsKey("2"));
        Assert.Contains("Title is duplicated within the import file.", result.Errors["2"]);
    }

    private sealed class WorkflowFixture
    {
        public WorkflowFixture()
        {
            Organization = new Organization
            {
                Id = Guid.NewGuid(),
                CompanyName = "Org",
                Address = "1 Main",
                City = "Austin",
                State = "TX",
                Zip = "78701",
                Phone = "512-555-0101",
                PrimaryContactFirstName = "A",
                PrimaryContactLastName = "B"
            };

            OrgAdminUser = new User
            {
                Id = Guid.NewGuid(),
                OrganizationId = Organization.Id,
                Email = "orgadmin@test.local",
                FirstName = "Org",
                LastName = "Admin",
                PasswordHash = "hash",
                Role = UserRole.OrgAdmin,
                Status = UserLifecycleStatus.Active
            };

            ReadOnlyUser = new User
            {
                Id = Guid.NewGuid(),
                OrganizationId = Organization.Id,
                Role = UserRole.ReadOnly,
                Status = UserLifecycleStatus.Active
            };

            StandardUser = new User
            {
                Id = Guid.NewGuid(),
                OrganizationId = Organization.Id,
                Email = "user@test.local",
                FirstName = "Standard",
                LastName = "User",
                PasswordHash = "hash",
                Role = UserRole.User,
                Status = UserLifecycleStatus.Active
            };

            ForeignOrganization = new Organization
            {
                Id = Guid.NewGuid(),
                CompanyName = "OtherOrg",
                Address = "2 Main",
                City = "Dallas",
                State = "TX",
                Zip = "75001",
                Phone = "214-555-0101",
                PrimaryContactFirstName = "F",
                PrimaryContactLastName = "O"
            };

            ForeignUser = new User
            {
                Id = Guid.NewGuid(),
                OrganizationId = ForeignOrganization.Id,
                Email = "foreign@test.local",
                FirstName = "Foreign",
                LastName = "User",
                PasswordHash = "hash",
                Role = UserRole.User,
                Status = UserLifecycleStatus.Active
            };

            SiteAdminUser = new User
            {
                Id = Guid.NewGuid(),
                OrganizationId = null,
                Email = "siteadmin@test.local",
                FirstName = "Site",
                LastName = "Admin",
                PasswordHash = "hash",
                Role = UserRole.SiteAdmin,
                Status = UserLifecycleStatus.Active
            };

            StatusOne = new Status
            {
                Id = Guid.NewGuid(),
                OrganizationId = Organization.Id,
                Name = "New / Pending",
                IsDeleted = false
            };

            StatusTwo = new Status
            {
                Id = Guid.NewGuid(),
                OrganizationId = Organization.Id,
                Name = "In Progress",
                IsDeleted = false
            };

            IdeaType = new IdeaType
            {
                Id = Guid.NewGuid(),
                OrganizationId = Organization.Id,
                Name = "Continuous Improvement",
                SortOrder = 0
            };

            BusinessImpact = new BusinessImpact
            {
                Id = Guid.NewGuid(),
                OrganizationId = Organization.Id,
                Name = "Medium",
                Color = "#F59E0B",
                SortOrder = 1
            };

            DataAccess = new FakeWorkflowDataAccess();
            AuditWriter = new FakeWorkflowAuditWriter();
            NotificationWriter = new FakeNotificationWriter();

            DataAccess.SeedOrganization(Organization);
            DataAccess.SeedOrganization(ForeignOrganization);
            DataAccess.SeedUser(OrgAdminUser);
            DataAccess.SeedUser(ReadOnlyUser);
            DataAccess.SeedUser(StandardUser);
            DataAccess.SeedUser(ForeignUser);
            DataAccess.SeedUser(SiteAdminUser);
            DataAccess.SeedStatus(StatusOne);
            DataAccess.SeedStatus(StatusTwo);
            DataAccess.SeedIdeaType(IdeaType);
            DataAccess.SeedBusinessImpact(BusinessImpact);

            Service = new WorkflowManagementService(DataAccess, AuditWriter, NotificationWriter);

            OrgAdminActor = new WorkflowActorContext
            {
                UserId = OrgAdminUser.Id,
                OrganizationId = Organization.Id,
                Role = UserRole.OrgAdmin.ToString()
            };

            ReadOnlyActor = new WorkflowActorContext
            {
                UserId = ReadOnlyUser.Id,
                OrganizationId = Organization.Id,
                Role = UserRole.ReadOnly.ToString()
            };

            StandardUserActor = new WorkflowActorContext
            {
                UserId = StandardUser.Id,
                OrganizationId = Organization.Id,
                Role = UserRole.User.ToString()
            };

            SiteAdminActor = new WorkflowActorContext
            {
                UserId = SiteAdminUser.Id,
                OrganizationId = Organization.Id,
                Role = UserRole.SiteAdmin.ToString()
            };

            ForeignUserActor = new WorkflowActorContext
            {
                UserId = ForeignUser.Id,
                OrganizationId = ForeignOrganization.Id,
                Role = UserRole.User.ToString()
            };
        }

        public FakeWorkflowDataAccess DataAccess { get; }

        public FakeWorkflowAuditWriter AuditWriter { get; }

        public FakeNotificationWriter NotificationWriter { get; }

        public WorkflowManagementService Service { get; }

        public Organization Organization { get; }

        public User OrgAdminUser { get; }

        public User ReadOnlyUser { get; }

        public User StandardUser { get; }

        public Organization ForeignOrganization { get; }

        public User ForeignUser { get; }

        public User SiteAdminUser { get; }

        public Status StatusOne { get; }

        public Status StatusTwo { get; }

        public IdeaType IdeaType { get; }

        public BusinessImpact BusinessImpact { get; }

        public WorkflowActorContext OrgAdminActor { get; }

        public WorkflowActorContext ReadOnlyActor { get; }

        public WorkflowActorContext StandardUserActor { get; }

        public WorkflowActorContext SiteAdminActor { get; }

        public WorkflowActorContext ForeignUserActor { get; }

        public Board CreateBoardWithTwoSwimlanes(bool allowUserStatusUpdate = false)
        {
            var board = new Board
            {
                Id = Guid.NewGuid(),
                OrganizationId = Organization.Id,
                Name = "Delivery",
                AllowUserStatusUpdate = allowUserStatusUpdate
            };

            DataAccess.SeedBoard(board);
            DataAccess.SeedSwimlane(new BoardSwimlane
            {
                BoardId = board.Id,
                StatusId = StatusOne.Id,
                Order = 0
            });
            DataAccess.SeedSwimlane(new BoardSwimlane
            {
                BoardId = board.Id,
                StatusId = StatusTwo.Id,
                Order = 1
            });

            return board;
        }

        public Idea CreateIdeaWithAuthor(User authorUser, Board? board = null)
        {
            board ??= CreateBoardWithTwoSwimlanes();
            var idea = new Idea
            {
                Id = Guid.NewGuid(),
                BoardId = board.Id,
                OrganizationId = Organization.Id,
                AuthorUserId = authorUser.Id,
                Title = "Idea",
                Description = "Description",
                Priority = IdeaPriority.Medium,
                IdeaTypeId = IdeaType.Id,
                BusinessImpactId = BusinessImpact.Id,
                StatusId = StatusOne.Id,
                CreatedAtUtc = DateTime.UtcNow
            };

            DataAccess.SeedIdea(idea);
            return idea;
        }

        public Comment CreateComment(Idea idea, User authorUser, string body)
        {
            var comment = new Comment
            {
                Id = Guid.NewGuid(),
                IdeaId = idea.Id,
                AuthorUserId = authorUser.Id,
                Body = body,
                CreatedAtUtc = DateTime.UtcNow
            };

            DataAccess.SeedComment(comment);
            return comment;
        }

        public Tag SeedTag(string name, string normalizedName)
        {
            var tag = new Tag
            {
                Id = Guid.NewGuid(),
                OrganizationId = Organization.Id,
                Name = name,
                NormalizedName = normalizedName
            };

            DataAccess.SeedTag(tag);
            return tag;
        }
    }
}
