using EmailCampaign.Application.Campaigns.Dtos.Requests;
using FluentValidation;

namespace EmailCampaign.Application.Campaigns.Validators;

public sealed class ScheduleCampaignDtoValidator : AbstractValidator<ScheduleCampaignDto>
{
    public ScheduleCampaignDtoValidator()
    {
        RuleFor(x => x.ScheduledAtUtc)
            .Must(t => t > DateTime.UtcNow.AddMinutes(1))
            .WithMessage("Seçilen zaman gelecekte olmalıdır.");
    }
}
