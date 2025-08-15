using EmailCampaign.Application.Common.Abstractions;
using MassTransit;

public sealed class MassTransitPublisher : IEventPublisher
{
    private readonly IPublishEndpoint _publish;
    public MassTransitPublisher(IPublishEndpoint publish) => _publish = publish;
    public Task PublishAsync<T>(T message, CancellationToken ct = default) where T : class
        => _publish.Publish(message, ct);
}
