using AutoMapper;
using AutoMapper.QueryableExtensions;
using EmailCampaign.Application.Campaigns.Dtos.Requests;
using EmailCampaign.Application.Campaigns.Dtos.Responses;
using EmailCampaign.Application.Campaigns.Enums;
using EmailCampaign.Application.Campaigns.Messaging;
using EmailCampaign.Application.Common.Abstractions;
using EmailCampaign.Domain.Entities;
using EmailCampaign.Application.Common.Repositories;
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
            .ProjectTo<CampaignListItemDto>(_mapper.ConfigurationProvider) //araştır
            .ToListAsync();
        return items;
    }
    public async Task<CampaignDto?> UpdateAsync(Guid id, UpdateCampaignDto dto)
    {
        var entity = await _repository.GetByIdAsync(id);
        if (entity is null) return null;

        _mapper.Map(dto, entity);
        await _repository.SaveChangesAsync();
        return _mapper.Map<CampaignDto>(entity);  
    }


    public async Task<bool> DeleteAsync(Guid id)
    {
        var entity = await _repository.GetByIdAsync(id); //getbyid private metoda taşı, çok yerde kullanılıyor çünkü
        if (entity is null) return false; //kullanıcı anlamaz exception kullan
        await _repository.DeleteAsync(entity);
        await _repository.SaveChangesAsync();
        return true;
    } 

    public async Task<EnqueueResult> EnqueueSendAsync(Guid id)
    {
        var entity = await _repository.GetByIdAsync(id);
        if (entity is null)
            return EnqueueResult.NotFound;

        // Zaten gönderilmişse kuyruğa tekrar alınmaz
        if (entity.Status == CampaignStatus.Sent)
            return EnqueueResult.AlreadySent;

        // Draft veya Scheduled ise kuyruğa al
        if (entity.Status is CampaignStatus.Draft or CampaignStatus.Scheduled)
        {
            // Idempotent: daha önce hiç set edilmediyse zamanı yaz
            if (entity.ScheduledAt is null)
                entity.ScheduledAt = DateTime.UtcNow;

            entity.Status = CampaignStatus.Scheduled;

            // DB’de status + scheduledAt güncellensin
            await _repository.SaveChangesAsync();

            // Mesajı oluşturup kuyruğa yayınla
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

        return EnqueueResult.Failed;
    } //command sent edilir, event publish farkına bak, dönüş türleri


    public async Task<CampaignReportDto?> GetReportAsync(Guid id)
    {
        var entity = await _repository.GetByIdAsync(id);
        return entity is null ? null : _mapper.Map<CampaignReportDto>(entity);
    }
}