using System.Linq.Dynamic.Core;
using Codium.Template.Application.Contracts.Roles;
using Codium.Template.Domain.Shared.Permissions;
using Codium.Template.HttpApi.Attributes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Codium.Template.HttpApi.Controllers.v1;

[ApiController]
[Route("api/v1/roles")]
[Authorize]
public class RoleController : ControllerBase
{
    private readonly IRoleAppService _roleAppService;

    public RoleController(IRoleAppService roleAppService)
    {
        _roleAppService = roleAppService;
    }
    
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(RoleResponseDto), StatusCodes.Status200OK)]
    [PermissionAuthorize(PermissionConsts.Role.GetById)]
    public async Task<IActionResult> GetByIdAsync([FromRoute(Name = "id")] Guid id, CancellationToken cancellationToken = default)
    {
        var response = await _roleAppService.GetByIdAsync(id, cancellationToken);
        return Ok(response);
    }
    
    [HttpGet]
    [ProducesResponseType(typeof(List<RoleResponseDto>), StatusCodes.Status200OK)]
    [PermissionAuthorize(PermissionConsts.Role.GetAll)]
    public async Task<IActionResult> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var response = await _roleAppService.GetAllAsync(cancellationToken);
        return Ok(response);
    }
    
    [HttpGet("paged")]
    [ProducesResponseType(typeof(PagedResult<RoleResponseDto>), StatusCodes.Status200OK)]
    [PermissionAuthorize(PermissionConsts.Role.Paged)]
    public async Task<IActionResult> GetPageableAndFilterAsync([FromQuery] GetListRolesRequestDto request, CancellationToken cancellationToken = default)
    {
        var response = await _roleAppService.GetPageableAndFilterAsync(request, cancellationToken);
        return Ok(response);
    }
    
    [HttpPost]
    [ProducesResponseType(typeof(RoleResponseDto), StatusCodes.Status200OK)]
    [PermissionAuthorize(PermissionConsts.Role.Create)]
    public async Task<IActionResult> CreateAsync([FromBody] CreateRoleRequestDto request, CancellationToken cancellationToken = default)
    {
        var response = await _roleAppService.CreateAsync(request, cancellationToken);
        return Ok(response);
    }
    
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(RoleResponseDto), StatusCodes.Status200OK)]
    [PermissionAuthorize(PermissionConsts.Role.Update)]
    public async Task<IActionResult> UpdateAsync(
        [FromRoute(Name = "id")] Guid id,
        [FromBody] UpdateRoleRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var response = await _roleAppService.UpdateAsync(id, request, cancellationToken);
        return Ok(response);
    }
    
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [PermissionAuthorize(PermissionConsts.Role.Delete)]
    public async Task<IActionResult> DeleteAsync([FromRoute(Name = "id")] Guid id, CancellationToken cancellationToken = default)
    {
        await _roleAppService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }
    
    [HttpPatch("{id:guid}/assign/{permissionId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [PermissionAuthorize(PermissionConsts.Role.AssignPermission)]
    public async Task<IActionResult> AddToPermissionAsync(
        [FromRoute(Name = "id")] Guid id,
        [FromRoute(Name = "permissionId")] Guid permissionId,
        CancellationToken cancellationToken = default)
    {
        await _roleAppService.AddToPermissionAsync(id, permissionId, cancellationToken);
        return NoContent();
    }
    
    [HttpPatch("{id:guid}/assign")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [PermissionAuthorize(PermissionConsts.Role.AssignPermission)]
    public async Task<IActionResult> AddToPermissionsAsync(
        [FromRoute(Name = "id")] Guid id, 
        [FromBody] List<Guid> permissionIds,
        CancellationToken cancellationToken = default)
    {
        await _roleAppService.AddToPermissionsAsync(id, permissionIds, cancellationToken);
        return NoContent();
    }
    
    [HttpPatch("{id:guid}/unassign/{permissionId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [PermissionAuthorize(PermissionConsts.Role.UnAssignPermission)]
    public async Task<IActionResult> RemoveFromPermissionAsync(
        [FromRoute(Name = "id")] Guid id, 
        [FromRoute(Name = "permissionId")] Guid permissionId,
        CancellationToken cancellationToken = default)
    {
        await _roleAppService.RemoveFromPermissionAsync(id, permissionId, cancellationToken);
        return NoContent();
    }
    
    [HttpPatch("{id:guid}/unassign")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [PermissionAuthorize(PermissionConsts.Role.UnAssignPermission)]
    public async Task<IActionResult> RemoveFromPermissionsAsync(
        [FromRoute(Name = "id")] Guid id,
        [FromBody] List<Guid> permissionIds,
        CancellationToken cancellationToken = default)
    {
        await _roleAppService.RemoveFromPermissionsAsync(id, permissionIds, cancellationToken);
        return NoContent();
    }
}