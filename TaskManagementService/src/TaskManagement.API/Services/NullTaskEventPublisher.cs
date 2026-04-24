using TaskManagement.Domain.Integration;

namespace TaskManagement.API.Services;

/// <summary>
/// Пустой публикатор, используемый при недоступном RabbitMQ.
/// </summary>
public class NullTaskEventPublisher : ITaskEventPublisher
{
    /// <inheritdoc />
    public Task PublishAsync(TaskChangedEvent taskChangedEvent, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
