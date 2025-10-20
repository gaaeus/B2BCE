using System.Text.Json;
using BuildingBlocks.Application.Abstractions.Messaging;
using Microsoft.Extensions.Logging;

namespace BuildingBlocks.Infrastructure.Messaging;

/// <summary>
/// Simple publisher that logs the serialized event.
/// Later you can replace this with RabbitMQ, Azure Service Bus, etc.
/// </summary>
public sealed class LoggingIntegrationEventPublisher : IIntegrationEventPublisher
{
    private readonly ILogger<LoggingIntegrationEventPublisher> _logger;

    public LoggingIntegrationEventPublisher(ILogger<LoggingIntegrationEventPublisher> logger)
    {
        _logger = logger;
    }

    public Task PublishAsync(IIntegrationEvent @event, CancellationToken cancellationToken = default)
    {
        var payload = JsonSerializer.Serialize(@event, new JsonSerializerOptions { WriteIndented = true });
        _logger.LogInformation("Publishing IntegrationEvent: {EventType}\n{Payload}", @event.GetType().Name, payload);
        return Task.CompletedTask;
    }

    public Task PublishAsync(IEnumerable<IIntegrationEvent> events, CancellationToken cancellationToken = default)
    {
        foreach (var @event in events)
        {
            var payload = JsonSerializer.Serialize(@event, new JsonSerializerOptions { WriteIndented = true });
            _logger.LogInformation("Publishing IntegrationEvent: {EventType}\n{Payload}", @event.GetType().Name, payload);
        }
        return Task.CompletedTask;
    }
}
