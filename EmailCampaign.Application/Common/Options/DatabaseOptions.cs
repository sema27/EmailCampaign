namespace EmailCampaign.Application.Common.Options;

public sealed class DatabaseOptions
{
    public const string SectionName = "ConnectionStrings";

    public string Default { get; init; } = "";
}
