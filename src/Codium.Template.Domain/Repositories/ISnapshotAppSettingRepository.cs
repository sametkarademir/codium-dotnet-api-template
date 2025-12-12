using Codium.Template.Domain.Shared.Repositories;
using Codium.Template.Domain.SnapshotAppSettings;

namespace Codium.Template.Domain.Repositories;

public interface ISnapshotAppSettingRepository : IRepository<SnapshotAppSetting, Guid>
{
}