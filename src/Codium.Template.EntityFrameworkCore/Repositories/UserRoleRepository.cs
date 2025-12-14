using Codium.Template.Domain.Repositories;
using Codium.Template.Domain.UserRoles;
using Codium.Template.EntityFrameworkCore.Contexts;
using Codium.Template.EntityFrameworkCore.Repositories.Common;

namespace Codium.Template.EntityFrameworkCore.Repositories;

public class UserRoleRepository(ApplicationDbContext context)
    : EfRepositoryBase<UserRole, Guid, ApplicationDbContext>(context), IUserRoleRepository;