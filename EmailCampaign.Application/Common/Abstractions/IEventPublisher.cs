﻿namespace EmailCampaign.Application.Common.Abstractions
{
    public interface IEventPublisher
    {
        Task PublishAsync<T>(T message, CancellationToken cancellationToken = default)
            where T : class;
    }
}
