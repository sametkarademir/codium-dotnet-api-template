using Microsoft.EntityFrameworkCore.Storage;

namespace Codium.Template.Domain.Shared.Repositories;

public interface IUnitOfWork : IDisposable
{
    IDbContextTransaction BeginTransaction();
    Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default);

    IDbContextTransaction? CurrentTransaction { get; }
    bool HasActiveTransaction { get; }

    int SaveChanges();
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}