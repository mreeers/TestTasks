using MassTransit;
using TaskManagement.Domain.Integration;

namespace TaskManagement.API.Services;

/// <summary>
/// Публикует события задач через MassTransit и RabbitMQ.
/// </summary>
public class MassTransitTaskEventPublisher : ITaskEventPublisher
{
    private readonly IPublishEndpoint _publishEndpoint;

    /// <summary>
    /// Инициализирует новый экземпляр класса <see cref="MassTransitTaskEventPublisher"/>.
    /// </summary>
    /// <param name="publishEndpoint">Точка публикации MassTransit.</param>
    public MassTransitTaskEventPublisher(IPublishEndpoint publishEndpoint)
    {
        _publishEndpoint = publishEndpoint;
    }

    /// <inheritdoc />
    public Task PublishAsync(TaskChangedEvent taskChangedEvent, CancellationToken cancellationToken)
    {
        return _publishEndpoint.Publish(taskChangedEvent, cancellationToken);
    }
}
