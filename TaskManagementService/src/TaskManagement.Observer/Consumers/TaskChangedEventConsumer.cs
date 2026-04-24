using MassTransit;
using TaskManagement.Domain.Integration;

namespace TaskManagement.Observer.Consumers;

/// <summary>
/// Обрабатывает task changed events from RabbitMQ and writes audit logs.
/// </summary>
public class TaskChangedEventConsumer : IConsumer<TaskChangedEvent>
{
    private readonly ILogger<TaskChangedEventConsumer> _logger;

    /// <summary>
    /// Инициализирует новый экземпляр класса <see cref="TaskChangedEventConsumer"/>.
    /// </summary>
    /// <param name="logger">Логгер.</param>
    public TaskChangedEventConsumer(ILogger<TaskChangedEventConsumer> logger)
    {
        _logger = logger;
    }

    /// <inheritdoc />
    public Task Consume(ConsumeContext<TaskChangedEvent> context)
    {
        var message = context.Message;
        _logger.LogInformation(
            "[Event Log]: Task {TaskId} updated at {Time}. EventType={EventType}",
            message.Id,
            message.OccurredAt,
            message.EventType);

        return Task.CompletedTask;
    }
}
