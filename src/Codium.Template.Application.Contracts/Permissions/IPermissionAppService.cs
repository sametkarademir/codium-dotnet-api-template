namespace Codium.Template.Application.Contracts.Permissions;

public interface IPermissionAppService
{
    Task<List<PermissionResponseDto>> GetAllAsync(CancellationToken cancellationToken = default);
}