using Codium.Template.Domain.Shared.Repositories;
using Codium.Template.Domain.SnapshotAssemblies;

namespace Codium.Template.Domain.Repositories;

public interface ISnapshotAssemblyRepository : IRepository<SnapshotAssembly, Guid>
{
}