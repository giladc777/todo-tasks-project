namespace Skins.Api.Services;

public interface ISkinEvents
{
    Task PublishAsync<T>(string key, T message, CancellationToken ct);
}