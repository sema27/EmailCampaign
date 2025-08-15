using EmailCampaign.Application.Campaigns.Dtos.Requests;
using FluentValidation;

namespace EmailCampaign.Application.Campaigns.Validators;

public sealed class UpdateCampaignDtoValidator : AbstractValidator<UpdateCampaignDto>
{
    public UpdateCampaignDtoValidator()
    {

        RuleFor(x => x.Name)
          .Must(v => v is null || (!string.IsNullOrWhiteSpace(v) && v != "string"))
          .WithMessage("Name geçersiz.");

        RuleFor(x => x.Subject)
          .Must(v => v is null || (!string.IsNullOrWhiteSpace(v) && v != "string"))
          .WithMessage("Subject geçersiz.");

        RuleFor(x => x.Content)
          .Must(v => v is null || (!string.IsNullOrWhiteSpace(v) && v != "string"))
          .WithMessage("Content geçersiz.");

    }
}
