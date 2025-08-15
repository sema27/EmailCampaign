namespace EmailCampaign.Domain.Repositories;

public interface IGenericRepository<TEntity, TKey> where TEntity : class
{

    IQueryable<TEntity> Query();

    Task<TEntity?> GetByIdAsync(TKey id);

    Task AddAsync(TEntity entity);

    Task UpdateAsync(TEntity entity);

    Task DeleteAsync(TEntity entity);
    Task<IEnumerable<TEntity>> GetAllAsync();

    Task SaveChangesAsync();

}


