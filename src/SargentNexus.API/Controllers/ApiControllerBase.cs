using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using SargentNexus.Application.Workflow;

namespace SargentNexus.API.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public abstract class ApiControllerBase : ControllerBase
{
	protected Guid? GetCurrentUserId()
	{
		var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

		return Guid.TryParse(userIdClaim, out var userId)
			? userId
			: null;
	}

	protected Guid? GetCurrentOrganizationId()
	{
		var organizationIdClaim = User.FindFirstValue("organization_id");

		return Guid.TryParse(organizationIdClaim, out var organizationId)
			? organizationId
			: null;
	}

	protected WorkflowActorContext GetWorkflowActorContext()
	{
		return new WorkflowActorContext
		{
			UserId = GetCurrentUserId() ?? Guid.Empty,
			OrganizationId = GetCurrentOrganizationId(),
			Role = User.FindFirstValue(ClaimTypes.Role) ?? string.Empty
		};
	}
}