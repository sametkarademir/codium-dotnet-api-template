using Codium.Template.Domain.Repositories;
using Codium.Template.Domain.UserRoles;
using Codium.Template.EntityFrameworkCore.Contexts;
using Codium.Template.EntityFrameworkCore.Repositories.Common;

namespace Codium.Template.EntityFrameworkCore.Repositories;

public class UserRoleRepository : EfRepositoryBase<UserRole, Guid, ApplicationDbContext>, IUserRoleRepository 
{
    public UserRoleRepository(ApplicationDbContext context) : base(context)
    {
        
    }
}