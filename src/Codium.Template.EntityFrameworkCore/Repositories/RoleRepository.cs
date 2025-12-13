using Codium.Template.Domain.Repositories;
using Codium.Template.Domain.Roles;
using Codium.Template.EntityFrameworkCore.Contexts;
using Codium.Template.EntityFrameworkCore.Repositories.Common;

namespace Codium.Template.EntityFrameworkCore.Repositories;

public class RoleRepository : EfRepositoryBase<Role, Guid, ApplicationDbContext>, IRoleRepository 
{
    public RoleRepository(ApplicationDbContext context) : base(context)
    {
        
    }
}