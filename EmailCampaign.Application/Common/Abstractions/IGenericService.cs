namespace EmailCampaign.Application.Common.Abstractions
{
    public interface IGenericService<TDto, TKey>
    {
        Task<TKey> CreateAsync(TDto dto);
        Task<TDto> GetByIdAsync(TKey id);
        Task<IEnumerable<TDto>> GetAllAsync();
        Task<bool> UpdateAsync(TKey id, TDto dto);
        Task<bool> DeleteAsync(TKey id);
    }
}
