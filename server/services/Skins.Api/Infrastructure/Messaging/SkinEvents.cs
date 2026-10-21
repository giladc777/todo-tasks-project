using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
using Skins.Api.Services;

namespace Skins.Api.Infrastructure.Messaging;

public sealed class SkinEvents(IConfiguration config) : ISkinEvents
{
    public Task PublishAsync<T>(string key, T message, CancellationToken ct)
    {
        var factory = new ConnectionFactory
        {
            Uri = new Uri(config["RabbitMq:Uri"] ?? "amqp://guest:guest@localhost:5672/")
        };
        using var connection = factory.CreateConnection();
        using var channel = connection.CreateModel();
        channel.ExchangeDeclare("todo.events", ExchangeType.Topic, durable: true);
        channel.BasicPublish("todo.events", key, null, Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message)));
        return Task.CompletedTask;
    }
}