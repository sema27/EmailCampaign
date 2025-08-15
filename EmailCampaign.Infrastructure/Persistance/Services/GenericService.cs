using AutoMapper;
using EmailCampaign.Application.Common.Abstractions;
using EmailCampaign.Domain.Repositories;

namespace EmailCampaign.Application.Persistance.Services
{
    public class GenericService<TEntity, TDto, TKey> : IGenericService<TDto, TKey>
        where TEntity : class
    {
        protected readonly IGenericRepository<TEntity, TKey> _repository;
        protected readonly IMapper _mapper;

        public GenericService(IGenericRepository<TEntity, TKey> repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public virtual async Task<TKey> CreateAsync(TDto dto)
        {
            var entity = _mapper.Map<TEntity>(dto);
            await _repository.AddAsync(entity);
            await _repository.SaveChangesAsync();

            var idProp = typeof(TEntity).GetProperty("Id");
            if (idProp is null)
                throw new Exception("Entity must have an Id property");

            var id = (TKey)idProp.GetValue(entity)!;
            return id;
        }

        public virtual async Task<TDto> GetByIdAsync(TKey id)
        {
            var entity = await _repository.GetByIdAsync(id);
            return _mapper.Map<TDto>(entity);
        }

        public virtual async Task<IEnumerable<TDto>> GetAllAsync()
        {
            var entities = await _repository.GetAllAsync();
            return _mapper.Map<IEnumerable<TDto>>(entities);
        }

        public virtual async Task<bool> UpdateAsync(TKey id, TDto dto)
        {
            var entity = await _repository.GetByIdAsync(id);
            if (entity is null) return false;

            _mapper.Map(dto, entity);
            await _repository.SaveChangesAsync();
            return true;
        }

        public virtual async Task<bool> DeleteAsync(TKey id)
        {
            var entity = await _repository.GetByIdAsync(id);
            if (entity is null) return false;

            await _repository.DeleteAsync(entity);
            await _repository.SaveChangesAsync();
            return true;
        }
    }
}
