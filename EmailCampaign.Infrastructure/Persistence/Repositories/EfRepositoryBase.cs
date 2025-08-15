using EmailCampaign.Application.Common.Repositories;
using Microsoft.EntityFrameworkCore;

namespace EmailCampaign.Infrastructure.Persistence.Repositories;

public class EfRepositoryBase<TEntity, TKey> : IGenericRepository<TEntity, TKey> where TEntity : class
{
    private readonly DbContext _context;
    private readonly DbSet<TEntity> _dbSet;

    public EfRepositoryBase(DbContext context)
    {
        _context = context;
        _dbSet = _context.Set<TEntity>();
    }

    public IQueryable<TEntity> Query() => _dbSet.AsQueryable();

    public async Task<TEntity?> GetByIdAsync(TKey id) => await _dbSet.FindAsync(id);

    public async Task AddAsync(TEntity entity) => await _dbSet.AddAsync(entity);

    public async Task UpdateAsync(TEntity entity)
    {
        _dbSet.Update(entity);
        await Task.CompletedTask;
    }

    public async Task DeleteAsync(TEntity entity)
    {
        _dbSet.Remove(entity);
        await Task.CompletedTask;
    }

    public async Task<IEnumerable<TEntity>> GetAllAsync() => await _dbSet.ToListAsync();

    public async Task SaveChangesAsync() => await _context.SaveChangesAsync();
}
