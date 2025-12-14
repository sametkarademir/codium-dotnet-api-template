using Codium.Template.Application.Contracts.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Codium.Template.HttpApi.Controllers.v1;

[ApiController]
[Route("api/v1/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthAppService _authAppService;

    public AuthController(IAuthAppService authAppService)
    {
        _authAppService = authAppService;
    }

    [HttpPost("login")]
    [ProducesResponseType(typeof(LoginResponseDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> LoginAsync([FromBody] LoginRequestDto request, CancellationToken cancellationToken = default)
    {
        var response = await _authAppService.LoginAsync(request, cancellationToken);
        return Ok(response);
    }

    [HttpPost("refresh-token")]
    [ProducesResponseType(typeof(LoginResponseDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> RefreshTokenAsync([FromBody] RefreshTokenRequestDto request, CancellationToken cancellationToken = default)
    {
        var response = await _authAppService.RefreshTokenAsync(request, cancellationToken);
        return Ok(response);
    }
    
    [HttpPost("logout")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [Authorize]
    public async Task<IActionResult> LogoutAsync(CancellationToken cancellationToken = default)
    {
        await _authAppService.LogoutAsync(cancellationToken);
        return NoContent();
    }
    
    [HttpPost("register")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> RegisterAsync([FromBody] RegisterRequestDto request, CancellationToken cancellationToken = default)
    {
        await _authAppService.RegisterAsync(request, cancellationToken);
        return NoContent();
    }
    
    [HttpPost("resend-email-confirmation/{email}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> ResendEmailConfirmationLinkAsync([FromRoute(Name = "email")] string email, CancellationToken cancellationToken = default)
    {
        await _authAppService.ResendEmailConfirmationLinkAsync(email, cancellationToken);
        return NoContent();
    }
    
    [HttpPost("confirm-email")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> ConfirmEmailAsync([FromBody] ConfirmEmailRequestDto request, CancellationToken cancellationToken = default)
    {
        await _authAppService.ConfirmEmailAsync(request, cancellationToken);
        return NoContent();
    }
    
    [HttpPost("forgot-password/{email}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> ForgotPasswordAsync([FromRoute(Name = "email")] string email, CancellationToken cancellationToken = default)
    {
        await _authAppService.ForgotPasswordAsync(email, cancellationToken);
        return NoContent();
    }
    
    [HttpPost("reset-password")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> ResetPasswordAsync([FromBody] ResetPasswordRequestDto request, CancellationToken cancellationToken = default)
    {
        await _authAppService.ResetPasswordAsync(request, cancellationToken);
        return NoContent();
    }
}