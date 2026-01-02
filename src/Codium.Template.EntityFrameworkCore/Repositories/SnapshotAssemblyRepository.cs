using Codium.Template.Domain.Repositories;
using Codium.Template.Domain.SnapshotAssemblies;
using Codium.Template.EntityFrameworkCore.Contexts;
using Codium.Template.EntityFrameworkCore.Repositories.Common;

namespace Codium.Template.EntityFrameworkCore.Repositories;

public class SnapshotAssemblyRepository(ApplicationDbContext dbContext)
    : EfRepositoryBase<SnapshotAssembly, Guid, ApplicationDbContext>(dbContext), ISnapshotAssemblyRepository;