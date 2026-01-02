using System.Linq.Dynamic.Core;
using Codium.Template.Application.Contracts.Users;
using Codium.Template.Domain.Shared.Permissions;
using Codium.Template.HttpApi.Attributes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Codium.Template.HttpApi.Controllers.v1;

[ApiController]
[Route("api/v1/users")]
[Authorize]
[EnableRateLimiting("api")]
public class UserController : ControllerBase
{
    private readonly IUserAppService _userAppService;

    public UserController(IUserAppService userAppService)
    {
        _userAppService = userAppService;
    }
    
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(UserResponseDto), StatusCodes.Status200OK)]
    [PermissionAuthorize(PermissionConsts.User.GetById)]
    public async Task<IActionResult> GetByIdAsync([FromRoute(Name = "id")] Guid id, CancellationToken cancellationToken = default)
    {
        var response = await _userAppService.GetByIdAsync(id, cancellationToken);
        return Ok(response);
    }
    
    [HttpGet]
    [ProducesResponseType(typeof(List<UserResponseDto>), StatusCodes.Status200OK)]
    [PermissionAuthorize(PermissionConsts.User.GetAll)]
    public async Task<IActionResult> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var response = await _userAppService.GetAllAsync(cancellationToken);
        return Ok(response);
    }
    
    [HttpGet("paged")]
    [ProducesResponseType(typeof(PagedResult<UserResponseDto>), StatusCodes.Status200OK)]
    [PermissionAuthorize(PermissionConsts.User.Paged)]
    public async Task<IActionResult> GetPageableAndFilterAsync([FromQuery] GetListUsersRequestDto request, CancellationToken cancellationToken = default)
    {
        var response = await _userAppService.GetPageableAndFilterAsync(request, cancellationToken);
        return Ok(response);
    }
    
    [HttpPost]
    [ProducesResponseType(typeof(UserResponseDto), StatusCodes.Status200OK)]
    [PermissionAuthorize(PermissionConsts.User.Create)]
    public async Task<IActionResult> CreateAsync([FromBody] CreateUserRequestDto request, CancellationToken cancellationToken = default)
    {
        var response = await _userAppService.CreateAsync(request, cancellationToken);
        return Ok(response);
    }
    
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(UserResponseDto), StatusCodes.Status200OK)]
    [PermissionAuthorize(PermissionConsts.User.Update)]
    public async Task<IActionResult> UpdateAsync(
        [FromRoute(Name = "id")] Guid id,
        [FromBody] UpdateUserRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var response = await _userAppService.UpdateAsync(id, request, cancellationToken);
        return Ok(response);
    }
    
    [HttpPatch("{id:guid}/email-confirmation")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [PermissionAuthorize(PermissionConsts.User.Update)]
    public async Task<IActionResult> ToggleEmailConfirmationAsync([FromRoute(Name = "id")] Guid id, CancellationToken cancellationToken = default)
    {
        await _userAppService.ToggleEmailConfirmationAsync(id, cancellationToken);
        return NoContent();
    }
    
    [HttpPatch("{id:guid}/phone-number-confirmation")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [PermissionAuthorize(PermissionConsts.User.Update)]
    public async Task<IActionResult> TogglePhoneNumberConfirmationAsync([FromRoute(Name = "id")] Guid id, CancellationToken cancellationToken = default)
    {
        await _userAppService.TogglePhoneNumberConfirmationAsync(id, cancellationToken);
        return NoContent();
    }
    
    [HttpPatch("{id:guid}/two-factor-enabled")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [PermissionAuthorize(PermissionConsts.User.Update)]
    public async Task<IActionResult> ToggleTwoFactorEnabledAsync([FromRoute(Name = "id")] Guid id, CancellationToken cancellationToken = default)
    {
        await _userAppService.ToggleTwoFactorEnabledAsync(id, cancellationToken);
        return NoContent();
    }
    
    [HttpPatch("{id:guid}/is-active")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [PermissionAuthorize(PermissionConsts.User.Update)]
    public async Task<IActionResult> ToggleIsActiveAsync([FromRoute(Name = "id")] Guid id, CancellationToken cancellationToken = default)
    {
        await _userAppService.ToggleIsActiveAsync(id, cancellationToken);
        return NoContent();
    }
    
    [HttpPatch("{id:guid}/assign/{roleId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [PermissionAuthorize(PermissionConsts.User.AssignRole)]
    public async Task<IActionResult> AddToRoleAsync(
        [FromRoute(Name = "id")] Guid id, 
        [FromRoute(Name = "roleId")] Guid roleId,
        CancellationToken cancellationToken = default)
    {
        await _userAppService.AddToRoleAsync(id, roleId, cancellationToken);
        return NoContent();
    }
    
    [HttpPatch("{id:guid}/assign")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [PermissionAuthorize(PermissionConsts.User.AssignRole)]
    public async Task<IActionResult> AddToRolesAsync(
        [FromRoute(Name = "id")] Guid id,
        [FromBody] List<Guid> roleIds,
        CancellationToken cancellationToken = default)
    {
        await _userAppService.AddToRolesAsync(id, roleIds, cancellationToken);
        return NoContent();
    }
    
    [HttpPatch("{id:guid}/unassign/{roleId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [PermissionAuthorize(PermissionConsts.User.UnAssignRole)]
    public async Task<IActionResult> RemoveFromRoleAsync(
        [FromRoute(Name = "id")] Guid id,
        [FromRoute(Name = "roleId")] Guid roleId,
        CancellationToken cancellationToken = default)
    {
        await _userAppService.RemoveFromRoleAsync(id, roleId, cancellationToken);
        return NoContent();
    }
    
    [HttpPatch("{id:guid}/unassign")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [PermissionAuthorize(PermissionConsts.User.UnAssignRole)]
    public async Task<IActionResult> RemoveFromRolesAsync(
        [FromRoute(Name = "id")] Guid id,
        [FromBody] List<Guid> roleIds,
        CancellationToken cancellationToken = default)
    {
        await _userAppService.RemoveFromRolesAsync(id, roleIds, cancellationToken);
        return NoContent();
    }
    
    [HttpPatch("{id:guid}/lock")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [PermissionAuthorize(PermissionConsts.User.Lock)]
    public async Task<IActionResult> LockAsync([FromRoute(Name = "id")] Guid id, CancellationToken cancellationToken = default)
    {
        await _userAppService.LockAsync(id, cancellationToken: cancellationToken);
        return NoContent();
    }
    
    [HttpPatch("{id:guid}/unlock")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [PermissionAuthorize(PermissionConsts.User.Unlock)]
    public async Task<IActionResult> UnlockAsync([FromRoute(Name = "id")] Guid id, CancellationToken cancellationToken = default)
    {
        await _userAppService.UnlockAsync(id, cancellationToken);
        return NoContent();
    }
    
    [HttpPatch("{id:guid}/reset-password")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [PermissionAuthorize(PermissionConsts.User.ResetPassword)]
    public async Task<IActionResult> ResetPasswordAsync(
        [FromRoute(Name = "id")] Guid id,
        [FromBody] ResetPasswordUserRequestDto request,
        CancellationToken cancellationToken = default)
    {
        await _userAppService.ResetPasswordAsync(id, request, cancellationToken);
        return NoContent();
    }
    
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [PermissionAuthorize(PermissionConsts.User.Delete)]
    public async Task<IActionResult> DeleteAsync([FromRoute(Name = "id")] Guid id, CancellationToken cancellationToken = default)
    {
        await _userAppService.DeleteAsync(id, cancellationToken);
        return Ok();
    }
}