using CentralPark.Application.Common.Interfaces;
using CentralPark.Domain.Entities;
using CentralPark.Infrastructure.Persistence.Repositories;

namespace CentralPark.Infrastructure.Persistence;

public sealed class UnitOfWork(ApplicationDbContext context) : IUnitOfWork
{
    private readonly Dictionary<Type, object> _repositories = [];

    public IRepository<T> Repository<T>() where T : BaseEntity
    {
        var type = typeof(T);
        if (!_repositories.TryGetValue(type, out var repo))
        {
            repo = new Repository<T>(context);
            _repositories[type] = repo;
        }
        return (IRepository<T>)repo;
    }

    public async Task<int> CommitAsync(CancellationToken ct = default)
        => await context.SaveChangesAsync(ct);

    public async Task RollbackAsync(CancellationToken ct = default)
        => await context.Database.RollbackTransactionAsync(ct);

    public async ValueTask DisposeAsync()
        => await context.DisposeAsync();
}
