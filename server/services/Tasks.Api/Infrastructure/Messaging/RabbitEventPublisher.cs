using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
using Tasks.Api.Services;

namespace Tasks.Api.Infrastructure.Messaging;

public sealed class RabbitEventPublisher(
    IConfiguration config,
    ILogger<RabbitEventPublisher> logger) : IEventPublisher
{
    public Task PublishAsync<T>(string routingKey, T message, CancellationToken ct)
    {
        var factory = new ConnectionFactory
        {
            Uri = new Uri(config["RabbitMq:Uri"] ?? "amqp://guest:guest@localhost:5672/")
        };
        using var connection = factory.CreateConnection();
        using var channel = connection.CreateModel();
        channel.ExchangeDeclare("todo.events", ExchangeType.Topic, durable: true);
        var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));
        channel.BasicPublish("todo.events", routingKey, null, body);
        logger.LogInformation("Published event {RoutingKey}", routingKey);
        return Task.CompletedTask;
    }
}