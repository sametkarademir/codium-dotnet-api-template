using Codium.Template.Domain.Shared.Repositories;
using Codium.Template.Domain.UserRoles;

namespace Codium.Template.Domain.Repositories;

public interface IUserRoleRepository : IRepository<UserRole, Guid>
{

}