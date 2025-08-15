using AutoMapper;
using AutoMapper.QueryableExtensions;
using EmailCampaign.Application.Campaigns.Dtos.Requests;
using EmailCampaign.Application.Campaigns.Dtos.Responses;
using EmailCampaign.Application.Campaigns.Enums;
using EmailCampaign.Application.Campaigns.Messaging;
using EmailCampaign.Application.Common.Abstractions;
using EmailCampaign.Domain.Entities;
using EmailCampaign.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
namespace EmailCampaign.Application.Campaigns.Services;
public sealed class CampaignService : ICampaignService
{
    private readonly IGenericRepository<Campaign, Guid> _repository;
    private readonly IEventPublisher _publisher;
    private readonly IMapper _mapper;
    public CampaignService(
        IGenericRepository<Campaign, Guid> repository,
        IEventPublisher publisher,
        IMapper mapper)
    {
        _repository = repository;
        _publisher = publisher;
        _mapper = mapper;
    }
    public async Task<CampaignDto> CreateAsync(CreateCampaignDto dto)
    {
        var entity = _mapper.Map<Campaign>(dto);
        await _repository.AddAsync(entity);
        await _repository.SaveChangesAsync();
        return _mapper.Map<CampaignDto>(entity);
    }
    public async Task<CampaignDto?> GetByIdAsync(Guid id)
    {
        var entity = await _repository.GetByIdAsync(id);
        return entity is null ? null : _mapper.Map<CampaignDto>(entity);
    }
    public async Task<IReadOnlyList<CampaignListItemDto>> GetAllAsync(int page, int pageSize)
    {
        var skip = (page - 1) * pageSize;
        var items = await _repository.Query()
            .AsNoTracking()
            .OrderByDescending(x => x.SentAt ?? x.CreatedAt)
            .Skip(skip)
            .Take(pageSize)
            .ProjectTo<CampaignListItemDto>(_mapper.ConfigurationProvider)
            .ToListAsync();
        return items;
    }
    public async Task<bool> UpdateAsync(Guid id, UpdateCampaignDto dto)
    {
        var entity = await _repository.GetByIdAsync(id);
        if (entity is null) return false;

        _mapper.Map(dto, entity);       // yalnıza null olmayanlar uygulanır
        await _repository.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var entity = await _repository.GetByIdAsync(id);
        if (entity is null) return false;
        await _repository.DeleteAsync(entity);
        await _repository.SaveChangesAsync();
        return true;
    }

    public async Task<EnqueueResult> EnqueueSendAsync(Guid id)
    {
        var entity = await _repository.GetByIdAsync(id);
        if (entity is null)
            return EnqueueResult.NotFound;

        if (entity.Status == CampaignStatus.Sent)
            return EnqueueResult.AlreadySent;

        var msg = _mapper.Map<SendEmailCommand>(entity);

        try
        {
            await _publisher.PublishAsync(msg);
            return EnqueueResult.Enqueued;
        }
        catch
        {
            return EnqueueResult.Failed;
        }
    }

    public async Task<CampaignReportDto?> GetReportAsync(Guid id)
    {
        var entity = await _repository.GetByIdAsync(id);
        return entity is null ? null : _mapper.Map<CampaignReportDto>(entity);
    }
}