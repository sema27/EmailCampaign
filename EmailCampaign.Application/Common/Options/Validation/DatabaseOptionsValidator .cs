using Microsoft.Extensions.Options;

namespace EmailCampaign.Application.Common.Options.Validation;

public sealed class DatabaseOptionsValidator : IValidateOptions<DatabaseOptions>
{
    public ValidateOptionsResult Validate(string? name, DatabaseOptions options)
    {
        if (string.IsNullOrWhiteSpace(options.Default))
            return ValidateOptionsResult.Fail("ConnectionStrings:Default is required");

        return ValidateOptionsResult.Success;
    }
}
