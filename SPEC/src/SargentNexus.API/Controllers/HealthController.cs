using Microsoft.AspNetCore.Mvc;

namespace SargentNexus.API.Controllers;

public sealed class HealthController : ApiControllerBase
{
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult Get()
    {
        return Ok(new
        {
            status = "ok"
        });
    }
}