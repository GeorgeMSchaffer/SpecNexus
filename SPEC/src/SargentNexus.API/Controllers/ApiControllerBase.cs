using Microsoft.AspNetCore.Mvc;

namespace SargentNexus.API.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public abstract class ApiControllerBase : ControllerBase
{
}