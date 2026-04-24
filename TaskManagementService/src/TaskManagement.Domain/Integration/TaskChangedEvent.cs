namespace TaskManagement.Domain.Integration;

/// <summary>
/// Описывает событие изменения задачи, публикуемое в интеграционные каналы.
/// </summary>
public class TaskChangedEvent
{
    /// <summary>
    /// Возвращает или задает идентификатор задачи.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Возвращает или задает тип события (Created, Updated, Deleted).
    /// </summary>
    public string EventType { get; set; } = string.Empty;

    /// <summary>
    /// Возвращает или задает время события в UTC.
    /// </summary>
    public DateTime OccurredAt { get; set; }
}
