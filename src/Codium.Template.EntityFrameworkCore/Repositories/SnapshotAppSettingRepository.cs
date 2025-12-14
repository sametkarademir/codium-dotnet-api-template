using Codium.Template.Domain.Repositories;
using Codium.Template.Domain.SnapshotAppSettings;
using Codium.Template.EntityFrameworkCore.Contexts;
using Codium.Template.EntityFrameworkCore.Repositories.Common;

namespace Codium.Template.EntityFrameworkCore.Repositories;

public class SnapshotAppSettingRepository(ApplicationDbContext dbContext)
    : EfRepositoryBase<SnapshotAppSetting, Guid, ApplicationDbContext>(dbContext), ISnapshotAppSettingRepository;