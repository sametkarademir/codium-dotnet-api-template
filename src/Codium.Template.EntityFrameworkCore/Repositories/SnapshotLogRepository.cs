using Codium.Template.Domain.Repositories;
using Codium.Template.Domain.SnapshotLogs;
using Codium.Template.EntityFrameworkCore.Contexts;
using Codium.Template.EntityFrameworkCore.Repositories.Common;
using Microsoft.EntityFrameworkCore;

namespace Codium.Template.EntityFrameworkCore.Repositories;

public class SnapshotLogRepository(ApplicationDbContext dbContext)
    : EfRepositoryBase<SnapshotLog, Guid, ApplicationDbContext>(dbContext), ISnapshotLogRepository
{
    public async Task<SnapshotLog?> GetLatestSnapshotLogAsync()
    {
        return await AsQueryable()
            .OrderByDescending(s => s.CreationTime)
            .FirstOrDefaultAsync();
    }
}