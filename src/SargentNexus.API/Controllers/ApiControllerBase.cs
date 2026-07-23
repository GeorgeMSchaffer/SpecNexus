using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

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
}