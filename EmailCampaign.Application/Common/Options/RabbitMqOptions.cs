namespace EmailCampaign.Application.Common.Options;

public sealed class RabbitMqOptions
{
    public const string SectionName = "RabbitMq";

    public string Host { get; init; } = "";
    public string VirtualHost { get; init; } = "/";
    public string Username { get; init; } = "";
    public string Password { get; init; } = "";
}
