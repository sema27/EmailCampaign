using Microsoft.Extensions.Options;

namespace EmailCampaign.Application.Common.Options.Validation;

public sealed class RabbitMqOptionsValidator : IValidateOptions<RabbitMqOptions>
{
    public ValidateOptionsResult Validate(string? name, RabbitMqOptions options)
    {
        if (string.IsNullOrWhiteSpace(options.Host))
            return ValidateOptionsResult.Fail("RabbitMq:Host is required");

        if (string.IsNullOrWhiteSpace(options.Username))
            return ValidateOptionsResult.Fail("RabbitMq:Username is required");

        if (string.IsNullOrWhiteSpace(options.Password))
            return ValidateOptionsResult.Fail("RabbitMq:Password is required");

        return ValidateOptionsResult.Success;
    }
}
