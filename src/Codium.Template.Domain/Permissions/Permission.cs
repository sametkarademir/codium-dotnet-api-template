using Codium.Template.Domain.RolePermissions;
using Codium.Template.Domain.Shared.BaseEntities.Abstractions;

namespace Codium.Template.Domain.Permissions;

public class Permission : FullAuditedEntity<Guid>
{
    public string Name { get; set; } = null!;
    public string NormalizedName { get; set; } = null!;
    
    public virtual ICollection<RolePermission> RolePermissions { get; set; } = [];
}