using Codium.Template.Domain.AuditLogs;
using Codium.Template.Domain.Repositories;
using Codium.Template.EntityFrameworkCore.Contexts;
using Codium.Template.EntityFrameworkCore.Repositories.Common;

namespace Codium.Template.EntityFrameworkCore.Repositories;

public class AuditLogRepository : EfRepositoryBase<AuditLog, Guid, ApplicationDbContext>, IAuditLogRepository 
{
    public AuditLogRepository(ApplicationDbContext dbContext) : base(dbContext)
    {
    }
}