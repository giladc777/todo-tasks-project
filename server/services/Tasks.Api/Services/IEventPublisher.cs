namespace Tasks.Api.Services;

public interface IEventPublisher
{
    Task PublishAsync<T>(string routingKey, T message, CancellationToken ct);
}