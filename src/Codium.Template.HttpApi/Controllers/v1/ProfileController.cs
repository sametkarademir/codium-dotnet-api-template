using System.Linq.Dynamic.Core;
using Codium.Template.Application.Contracts.Profiles;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Codium.Template.HttpApi.Controllers.v1;

[ApiController]
[Route("api/v1/profile")]
[Authorize]
[EnableRateLimiting("api")]
public class ProfileController : ControllerBase
{
    private readonly IProfileAppService _profileAppService;

    public ProfileController(IProfileAppService profileAppService)
    {
        _profileAppService = profileAppService;
    }
    
    [HttpGet]
    [ProducesResponseType(typeof(ProfileResponseDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetProfileAsync(CancellationToken cancellationToken = default)
    {
        var response = await _profileAppService.GetProfileAsync(cancellationToken);
        return Ok(response);
    }
    
    [HttpGet("sessions")]
    [ProducesResponseType(typeof(PagedResult<SessionResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPageableAndFilterAsync([FromQuery] GetListSessionsRequestDto request, CancellationToken cancellationToken = default)
    {
        var response = await _profileAppService.GetPageableAndFilterAsync(request, cancellationToken);
        return Ok(response);
    }
    
    [HttpPost("change-password")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> ChangePasswordAsync([FromBody] ChangePasswordUserRequestDto request, CancellationToken cancellationToken = default)
    {
        await _profileAppService.ChangePasswordAsync(request, cancellationToken);
        return NoContent();
    }
    
    [HttpDelete("sessions/{sessionId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> InvalidateSessionAsync([FromRoute(Name = "sessionId")] Guid sessionId, CancellationToken cancellationToken = default)
    {
        await _profileAppService.InvalidateSessionAsync(sessionId, cancellationToken);
        return NoContent();
    }
}