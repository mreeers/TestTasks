using TaskManagement.Domain.Entities;

namespace TaskManagement.API.Services;

/// <summary>
/// Уведомляет внешние системы об изменениях задач.
/// </summary>
public interface ITaskEventNotifier
{
    /// <summary>
    /// Отправляет интеграционные уведомления об изменении задачи.
    /// </summary>
    /// <param name="taskItem">Сущность задачи.</param>
    /// <param name="eventType">Тип события.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    Task NotifyAsync(TaskItem taskItem, string eventType, CancellationToken cancellationToken);
}
