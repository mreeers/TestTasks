using TaskManagement.Domain.Integration;

namespace TaskManagement.API.Services;

/// <summary>
/// Публикует события задач в асинхронный транспорт.
/// </summary>
public interface ITaskEventPublisher
{
    /// <summary>
    /// Публикует событие изменения задачи.
    /// </summary>
    /// <param name="taskChangedEvent">Полезная нагрузка события.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    Task PublishAsync(TaskChangedEvent taskChangedEvent, CancellationToken cancellationToken);
}
