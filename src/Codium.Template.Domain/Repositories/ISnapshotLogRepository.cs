using Codium.Template.Domain.Shared.Repositories;
using Codium.Template.Domain.SnapshotLogs;

namespace Codium.Template.Domain.Repositories;

public interface ISnapshotLogRepository : IRepository<SnapshotLog, Guid>
{
    Task<SnapshotLog?> GetLatestSnapshotLogAsync();
}