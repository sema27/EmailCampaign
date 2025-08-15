using EmailCampaign.Application.Campaigns.Dtos.Requests;
using FluentValidation;

namespace EmailCampaign.Application.Campaigns.Validators;

public sealed class CreateCampaignDtoValidator : AbstractValidator<CreateCampaignDto>
{
    public CreateCampaignDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Kampanya adı zorunludur.")
            .MaximumLength(100).WithMessage("Kampanya adı en fazla 100 karakter olabilir.");

        RuleFor(x => x.Subject)
            .NotEmpty().WithMessage("Konu başlığı zorunludur.")
            .MaximumLength(200).WithMessage("Konu başlığı en fazla 200 karakter olabilir.");

        RuleFor(x => x.Content)
            .NotEmpty().WithMessage("İçerik zorunludur.")
            .MinimumLength(10).WithMessage("İçerik en az 10 karakter olmalıdır.");
    }
}
