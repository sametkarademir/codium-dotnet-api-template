using Codium.Template.Domain.Repositories;
using Codium.Template.Domain.SnapshotLogs;
using Codium.Template.EntityFrameworkCore.Contexts;
using Codium.Template.EntityFrameworkCore.Repositories.Common;
using Microsoft.EntityFrameworkCore;

namespace Codium.Template.EntityFrameworkCore.Repositories;

public class SnapshotLogRepository : EfRepositoryBase<SnapshotLog, Guid, ApplicationDbContext>, ISnapshotLogRepository
{
    public SnapshotLogRepository(ApplicationDbContext dbContext) : base(dbContext)
    {
    }

    public async Task<SnapshotLog?> GetLatestSnapshotLogAsync()
    {
        return await AsQueryable()
            .OrderByDescending(s => s.CreationTime)
            .FirstOrDefaultAsync();
    }
}