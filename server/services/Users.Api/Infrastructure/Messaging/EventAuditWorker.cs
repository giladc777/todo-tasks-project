using System.Text;
using Microsoft.EntityFrameworkCore;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Users.Api.Data;
using Users.Api.Models;

namespace Users.Api.Infrastructure.Messaging;

public sealed class EventAuditWorker(
    IServiceScopeFactory scopes,
    IConfiguration config,
    ILogger<EventAuditWorker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var factory = new ConnectionFactory
                {
                    Uri = new Uri(config["RabbitMq:Uri"] ?? "amqp://guest:guest@localhost:5672/"),
                    DispatchConsumersAsync = true,
                    AutomaticRecoveryEnabled = true
                };
                using var connection = factory.CreateConnection();
                using var channel = connection.CreateModel();
                channel.ExchangeDeclare("todo.events", ExchangeType.Topic, durable: true);
                var queue = channel.QueueDeclare(
                    "users.audit",
                    durable: true,
                    exclusive: false,
                    autoDelete: false).QueueName;
                channel.QueueBind(queue, "todo.events", "task.created");
                channel.QueueBind(queue, "todo.events", "skin.updated");

                var consumer = new AsyncEventingBasicConsumer(channel);
                consumer.Received += async (_, eventArgs) =>
                {
                    var type = eventArgs.RoutingKey;
                    var message = Encoding.UTF8.GetString(eventArgs.Body.ToArray());
                    using var scope = scopes.CreateScope();
                    var db = scope.ServiceProvider.GetRequiredService<UserDb>();
                    db.AuditEvents.Add(new AuditRow
                    {
                        Id = Guid.NewGuid(),
                        Type = type,
                        Message = message,
                        OccurredAt = DateTimeOffset.UtcNow
                    });
                    await db.SaveChangesAsync(stoppingToken);
                    channel.BasicAck(eventArgs.DeliveryTag, multiple: false);
                    logger.LogInformation("Recorded RabbitMQ event {EventType}", type);
                };

                channel.BasicConsume(queue, autoAck: false, consumer);
                logger.LogInformation("Listening for task and skin events");
                await Task.Delay(Timeout.Infinite, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "RabbitMQ audit consumer failed; retrying in five seconds");
                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
        }
    }
}