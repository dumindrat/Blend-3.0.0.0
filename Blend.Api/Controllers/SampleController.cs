using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Blend.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public sealed class SampleController : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {
        return Ok(new
        {
            message = "Authenticated request received by the self-hosted API.",
            user = User.Identity?.Name
        });
    }
}
