using EmailCampaign.Application.Common.Repositories;
using Microsoft.EntityFrameworkCore;

public class GenericRepository<TEntity, TKey> : IGenericRepository<TEntity, TKey>
   where TEntity : class
{
    private readonly DbContext _context;
    public GenericRepository(DbContext context)
    {
        _context = context;
    }
    public IQueryable<TEntity> Query()
    {
        return _context.Set<TEntity>();
    }
    public async Task<TEntity?> GetByIdAsync(TKey id)
    {
        return await _context.Set<TEntity>().FindAsync(id);
    }
    public async Task AddAsync(TEntity entity)
    {
        await _context.Set<TEntity>().AddAsync(entity);
    }
    public async Task UpdateAsync(TEntity entity)
    {
        _context.Set<TEntity>().Update(entity);
        await Task.CompletedTask;
    }
    public async Task DeleteAsync(TEntity entity)
    {
        _context.Set<TEntity>().Remove(entity);
        await Task.CompletedTask;
    }
    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<TEntity>> GetAllAsync()
    {
        return await _context.Set<TEntity>().ToListAsync();
    }

}